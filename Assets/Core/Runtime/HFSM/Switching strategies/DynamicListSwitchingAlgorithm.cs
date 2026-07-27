using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Алгоритм переключения состояний на основе динамического списка глобальных приоритетов с полной поддержкой синхронных и асинхронных состояний.
    /// Каждый кадр проверяет правила сверху вниз, корректно управляя асинхронными процессами входа и выхода при смене стейтов.
    /// <br/><br/>
    /// A priority list-based state switching algorithm with full support for synchronous and asynchronous states.
    /// Evaluates rules from top to bottom every frame while correctly managing asynchronous entry and exit processes during transitions.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public sealed class DynamicListSwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitcher<TContext> _switcher;
        private ISyncState<TContext> _current;

        private readonly List<DynamicRule> _rules = new();
        private readonly Dictionary<Type, ISyncState<TContext>> _registry = new();
        private CancellationTokenSource _transitionCts;

        /// <summary>
        /// Возвращает текущее активное состояние.
        /// <br/><br/>
        /// Gets the current active state.
        /// </summary>
        public ISyncState<TContext> Current => _current;

        /// <summary>
        /// Внутренняя структура, связывающая целевое состояние со списком условий его активации.
        /// <br/><br/>
        /// Internal structure linking a target state to the list of conditions required for its activation.
        /// </summary>
        private struct DynamicRule
        {
            public ISyncState<TContext> TargetState;
            public List<Condition<TContext>> Conditions;
        }

        /// <summary>
        /// Регистрирует экземпляр состояния во внутреннем реестре алгоритма.
        /// Использует Fluent API, возвращая ссылку на самого себя.
        /// <br/><br/>
        /// Registers a state instance within the algorithm's internal registry.
        /// Uses Fluent API by returning a reference to itself.
        /// </summary>
        /// <param name="state">Регистрируемый экземпляр состояния.<br/><br/>The state instance to register.</param>
        /// <returns>Текущий экземпляр алгоритма переключения.<br/><br/>The current switching algorithm instance.</returns>
        public DynamicListSwitchingAlgorithm<TContext> RegisterState(ISyncState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
            return this;
        }

        /// <summary>
        /// Добавляет новое приоритетное правило в конец цепочки опроса.
        /// Использует Fluent API, позволяя выстраивать иерархию условий сверху вниз.
        /// <br/><br/>
        /// Adds a new priority rule to the end of the evaluation chain.
        /// Uses Fluent API, allowing conditions to be structured from highest to lowest priority.
        /// </summary>
        /// <typeparam name="TState">Тип целевого состояния.<br/><br/>The type of the target state.</typeparam>
        /// <param name="conditions">Список условий, необходимых для перехода.<br/><br/>The list of conditions required for the transition.</param>
        /// <returns>Текущий экземпляр алгоритма переключения.<br/><br/>The current switching algorithm instance.</returns>
        public DynamicListSwitchingAlgorithm<TContext> AddPriorityRule<TState>(List<Condition<TContext>> conditions) where TState : ISyncState<TContext>
        {
            if (_registry.TryGetValue(typeof(TState), out var state))
            {
                _rules.Add(new DynamicRule { TargetState = state, Conditions = conditions });
            }
            return this;
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
        /// Производит сквозной ежекадровый опрос правил приоритета. При выполнении условий осуществляет безопасный переход
        /// на более приоритетное состояние и затем вызывает регулярный Update активного стейта.
        /// <br/><br/>
        /// Performs a complete frame-by-frame evaluation of priority rules. If conditions are met, executes
        /// a safe transition to the higher priority state and then invokes the active state's Update.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                var rule = _rules[i];
                if (EvaluateConditions(rule.Conditions))
                {
                    PerformTransition(rule.TargetState);
                    break;
                }
            }

            _current?.Update(_context);
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
        /// Проверяет выполнение списка условий. Возвращает истину, если все условия в списке выполнены успешно.
        /// <br/><br/>
        /// Evaluates a list of conditions. Returns true if all conditions in the list evaluate to true.
        /// </summary>
        /// <param name="conditions">Список проверяемых условий.<br/><br/>The list of conditions to check.</param>
        /// <returns>True, если условий нет или все они истинны; иначе false.<br/><br/>True if there are no conditions or all are true; otherwise, false.</returns>
        private bool EvaluateConditions(List<Condition<TContext>> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            for (int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].Check(_context)) return false;
            }
            return true;
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