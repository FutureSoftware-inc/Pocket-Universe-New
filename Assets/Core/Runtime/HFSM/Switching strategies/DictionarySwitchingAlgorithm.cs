using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Алгоритм переключения состояний на основе словаря с полной поддержкой синхронных и асинхронных состояний.
    /// Использует типы C# в качестве ключей для мгновенного поиска и корректно обрабатывает асинхронные переходы входа и выхода.
    /// <br/><br/>
    /// A dictionary-based state switching algorithm with full support for synchronous and asynchronous states.
    /// Uses C# types as keys for instant lookup and correctly processes asynchronous entry and exit transitions.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public sealed class DictionarySwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitcher<TContext> _switcher;
        private ISyncState<TContext> _current;

        private readonly Dictionary<Type, ISyncState<TContext>> _registry = new();
        private CancellationTokenSource _transitionCts;

        /// <summary>
        /// Возвращает текущее активное состояние.
        /// <br/><br/>
        /// Gets the current active state.
        /// </summary>
        public ISyncState<TContext> Current => _current;

        /// <summary>
        /// Регистрирует экземпляр состояния во внутреннем реестре алгоритма.
        /// Использует Fluent API, возвращая ссылку на самого себя.
        /// <br/><br/>
        /// Registers a state instance within the algorithm's internal registry.
        /// Uses Fluent API by returning a reference to itself.
        /// </summary>
        /// <param name="state">Регистрируемый экземпляр состояния.<br/><br/>The state instance to register.</param>
        /// <returns>Текущий экземпляр алгоритма переключения.<br/><br/>The current switching algorithm instance.</returns>
        public DictionarySwitchingAlgorithm<TContext> RegisterState(ISyncState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
            return this;
        }

        /// <summary>
        /// Задает начальное состояние машины и запускает его жизненный цикл (включая асинхронный вход).
        /// <br/><br/>
        /// Sets the initial state of the machine and starts its lifecycle (including asynchronous entry).
        /// </summary>
        /// <typeparam name="TState">Тип стартового состояния.<br/><br/>The type of the starting state.</typeparam>
        public void StartWith<TState>() where TState : ISyncState<TContext>
        {
            if (_registry.TryGetValue(typeof(TState), out var state))
            {
                _current = state;
                ExecuteEntryLifecycle(_current);
            }
        }

        /// <summary>
        /// Кэширует контекст данных и переключатель для внутренней работы алгоритма.
        /// <br/><br/>
        /// Caches the data context and the switcher for the algorithm's internal operations.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <param name="switcher">Компонент переключения состояний.<br/><br/>The state switcher component.</param>
        public void Initialize(TContext context, IStateSwitcher<TContext> switcher)
        {
            _context = context;
            _switcher = switcher;
        }

        /// <summary>
        /// Проверяет условия переходов текущего состояния (считывает переходы из State и AsyncState).
        /// При выполнении условий осуществляет безопасную смену активного стейта.
        /// <br/><br/>
        /// Evaluates transition conditions of the current state (reads transitions from both State and AsyncState).
        /// If conditions are met, executes a safe active state transition.
        /// </summary>
        public void Update()
        {
            if (_current == null) return;

            IReadOnlyList<Transition<TContext>> transitions = null;

            if (_current is SyncState<TContext> polymorphicState)
            {
                transitions = polymorphicState.Transitions;
            }
            else if (_current is AsyncState<TContext> asyncPolymorphicState)
            {
                transitions = asyncPolymorphicState.Transitions;
            }

            if (transitions != null)
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    var transition = transitions[i];
                    if (transition.ToTargetState(_context) && transition.TargetState != null)
                    {
                        PerformTransition(transition.TargetState);
                        break;
                    }
                }
            }

            _current.Update(_context);
        }

        /// <summary>
        /// Вызывает физический апдейт текущего активного состояния.
        /// <br/><br/>
        /// Invokes the physics update of the current active state.
        /// </summary>
        public void FixedUpdate() => _current?.FixedUpdate(_context);

        /// <summary>
        /// Вызывает позднее обновление текущего активного состояния.
        /// <br/><br/>
        /// Invokes the late update of the current active state.
        /// </summary>
        public void LateUpdate() => _current?.LateUpdate(_context);

        /// <summary>
        /// Запускает принудительную смену текущего стейта по явному запросу через тип <typeparamref name="TState"/>.
        /// <br/><br/>
        /// Triggers a forced change of the current state via an explicit request using type <typeparamref name="TState"/>.
        /// </summary>
        /// <typeparam name="TState">Целевой тип состояния для перехода.<br/><br/>The target state type to transition into.</typeparam>
        public void ExecuteSwitch<TState>() where TState : ISyncState<TContext>
        {
            if (_registry.TryGetValue(typeof(TState), out var newState))
            {
                PerformTransition(newState);
            }
        }

        /// <summary>
        /// Внутренний метод смены состояний. Отменяет прошлые переходы, обрабатывает Exit/ExitAsync и запускает Entry/EntryAsync.
        /// <br/><br/>
        /// Internal state transition method. Cancels previous transitions, processes Exit/ExitAsync, and triggers Entry/EntryAsync.
        /// </summary>
        /// <param name="newState">Новое целевое состояние.<br/><br/>The new target state.</param>
        private void PerformTransition(ISyncState<TContext> newState)
        {
            if (newState == null || _current == newState) return;

            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();

            ExecuteExitLifecycle(_current, _transitionCts.Token);
            _current = newState;
            ExecuteEntryLifecycle(_current, _transitionCts.Token);
        }

        /// <summary>
        /// Запускает выполнение логики входа в состояние, автоматически разделяя синхронный и асинхронный контексты.
        /// <br/><br/>
        /// Starts the execution of the state entry logic, automatically separating synchronous and asynchronous contexts.
        /// </summary>
        /// <param name="state">Целевое состояние для выполнения входа.<br/><br/>The target state to execute entry for.</param>
        /// <param name="token">Токен отмены для асинхронной операции.<br/><br/>The cancellation token for the asynchronous operation.</param>
        private void ExecuteEntryLifecycle(ISyncState<TContext> state, CancellationToken token = default)
        {
            if (state is IAsyncState<TContext> asyncState)
            {
                _ = ForgetTask(asyncState.EntryAsync(_context, token));
            }
            else
            {
                state?.Entry(_context);
            }
        }

        /// <summary>
        /// Запускает выполнение логики выхода из состояния, автоматически разделяя синхронный и асинхронный контексты.
        /// <br/><br/>
        /// Starts the execution of the state exit logic, automatically separating synchronous and asynchronous contexts.
        /// </summary>
        /// <param name="state">Целевое состояние для выполнения выхода.<br/><br/>The target state to execute exit for.</param>
        /// <param name="token">Токен отмены для асинхронной операции.<br/><br/>The cancellation token for the asynchronous operation.</param>
        private void ExecuteExitLifecycle(ISyncState<TContext> state, CancellationToken token = default)
        {
            if (state is IAsyncState<TContext> asyncState)
            {
                _ = ForgetTask(asyncState.ExitAsync(_context, token));
            }
            else
            {
                state?.Exit(_context);
            }
        }

        /// <summary>
        /// Безопасно обрабатывает выполнение асинхронной задачи, перехватывая отмену и логируя критические исключения.
        /// <br/><br/>
        /// Safely processes the execution of an asynchronous task, catching cancellation and logging critical exceptions.
        /// </summary>
        /// <param name="task">Выполняемая асинхронная задача.<br/><br/>The asynchronous task to be executed.</param>
        /// <returns>Асинхронная задача.<br/><br/>An asynchronous task.</returns>
        private async UniTask ForgetTask(UniTask task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}