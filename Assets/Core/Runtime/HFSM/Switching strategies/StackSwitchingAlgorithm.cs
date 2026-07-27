using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Алгоритм переключения состояний на основе стека с полной поддержкой синхронных и асинхронных состояний.
    /// Позволяет временно перекрывать текущие стейты новыми и возвращаться к ним, корректно управляя асинхронными процессами входа и выхода.
    /// <br/><br/>
    /// A stack-based state switching algorithm with full support for synchronous and asynchronous states.
    /// Allows temporarily overriding current states with new ones and returning to them, while correctly managing asynchronous entry and exit processes.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public sealed class StackSwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitcher<TContext> _switcher;

        /// <summary>
        /// Стек состояний, где верхний элемент является текущим активным состоянием.
        /// <br/><br/>
        /// The stack of states, where the top element represents the currently active state.
        /// </summary>
        private readonly Stack<ISyncState<TContext>> _stateStack = new();

        /// <summary>
        /// Внутренний реестр, связывающий системный тип состояния с его конкретным экземпляром.
        /// <br/><br/>
        /// Internal registry mapping a system state type to its specific instance.
        /// </summary>
        private readonly Dictionary<Type, ISyncState<TContext>> _registry = new();

        /// <summary>
        /// Источник токенов отмены для контроля и прерывания текущих асинхронных операций входа и выхода.
        /// <br/><br/>
        /// The cancellation token source to control and interrupt current asynchronous entry and exit operations.
        /// </summary>
        private CancellationTokenSource _transitionCts;

        /// <summary>
        /// Возвращает текущее активное состояние на вершине стека. Если стек пуст, возвращает null.
        /// <br/><br/>
        /// Gets the current active state at the top of the stack. Returns null if the stack is empty.
        /// </summary>
        public ISyncState<TContext> Current => _stateStack.Count > 0 ? _stateStack.Peek() : null;

        /// <summary>
        /// Регистрирует экземпляр состояния в общем словаре алгоритма для его последующего добавления в стек.
        /// Использует Fluent API, возвращая ссылку на самого себя.
        /// <br/><br/>
        /// Registers a state instance in the algorithm's internal dictionary for future stacking.
        /// Uses Fluent API by returning a reference to itself.
        /// </summary>
        /// <param name="state">Регистрируемый экземпляр состояния.<br/><br/>The state instance to register.</param>
        /// <returns>Текущий экземпляр алгоритма переключения.<br/><br/>The current switching algorithm instance.</returns>
        public StackSwitchingAlgorithm<TContext> RegisterState(ISyncState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
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
        /// Помещает новое состояние типа <typeparamref name="TState"/> на вершину стека.
        /// Перед активацией нового состояния прерывает текущие таски и вызывает метод выхода у предыдущего активного стейта.
        /// <br/><br/>
        /// Pushes a new state of type <typeparamref name="TState"/> onto the top of the stack.
        /// Interrupts current tasks and invokes the exit method of the previous active state before entering the new one.
        /// </summary>
        /// <typeparam name="TState">Тип состояния для добавления на вершину стека.<br/><br/>The type of the state to push onto the stack.</typeparam>
        public void PushState<TState>() where TState : ISyncState<TContext>
        {
            if (!_registry.TryGetValue(typeof(TState), out var newState)) return;

            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();

            ExecuteExitLifecycle(Current, _transitionCts.Token);
            _stateStack.Push(newState);
            ExecuteEntryLifecycle(newState, _transitionCts.Token);
        }

        /// <summary>
        /// Удаляет текущее состояние с вершины стека и вызывает его метод выхода.
        /// Автоматически возвращает машину к предыдущему состоянию в стеке, запуская логику входа.
        /// Предотвращает удаление последнего базового состояния.
        /// <br/><br/>
        /// Pops the current state from the top of the stack and invokes its exit method.
        /// Automatically returns the machine to the previous state in the stack, triggering its entry logic.
        /// Prevents removing the last remaining base state.
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count <= 1) return;

            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();

            var removedState = _stateStack.Pop();
            ExecuteExitLifecycle(removedState, _transitionCts.Token);

            ExecuteEntryLifecycle(Current, _transitionCts.Token);
        }
        /// <summary>
        /// Проверяет условия переходов текущего активного состояния (поддерживает State и AsyncState).
        /// При срабатывании перехода безопасно подменяет верхний элемент стека на новое целевое состояние.
        /// <br/><br/>
        /// Evaluates transition conditions of the current active state (supports both State and AsyncState).
        /// If a transition triggers, safely replaces the top element of the stack with the new target state.
        /// </summary>
        public void Update()
        {
            ISyncState<TContext> active = Current;
            if (active == null) return;

            IReadOnlyList<Transition<TContext>> transitions = null;

            if (active is SyncState<TContext> polymorphicState)
            {
                transitions = polymorphicState.Transitions;
            }
            else if (active is AsyncState<TContext> asyncPolymorphicState)
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
                        ExecuteTransition(transition.TargetState);
                        break;
                    }
                }
            }

            Current?.Update(_context);
        }

        /// <summary>
        /// Вызывает физический апдейт текущего активного состояния на вершине стека.
        /// <br/><br/>
        /// Invokes the physics update of the current active state at the top of the stack.
        /// </summary>
        public void FixedUpdate() => Current?.FixedUpdate(_context);

        /// <summary>
        /// Вызывает позднее обновление текущего активного состояния на вершине стека.
        /// <br/><br/>
        /// Invokes the late update of the current active state at the top of the stack.
        /// </summary>
        public void LateUpdate() => Current?.LateUpdate(_context);

        /// <summary>
        /// Запускает мгновенную замену текущего состояния на <typeparamref name="TState"/>, подменяя верхний элемент стека.
        /// <br/><br/>
        /// Triggers an immediate state switch to <typeparamref name="TState"/>, replacing the top element of the stack.
        /// </summary>
        /// <typeparam name="TState">Целевой тип состояния для перехода.<br/><br/>The target state type to transition into.</typeparam>
        public void ExecuteSwitch<TState>() where TState : ISyncState<TContext>
        {
            if (!_registry.TryGetValue(typeof(TState), out var newState)) return;
            ExecuteTransition(newState);
        }

        /// <summary>
        /// Внутренний метод смены состояний в стеке. Отменяет текущие таски, вызывает выход из текущего стейта,
        /// подменяет верхний элемент стека и запускает логику входа для нового состояния.
        /// <br/><br/>
        /// Internal state transition method within the stack. Cancels current tasks, invokes exit of the current state,
        /// replaces the top element of the stack, and triggers the entry logic for the new state.
        /// </summary>
        /// <param name="newState">Новое целевое состояние.<br/><br/>The new target state.</param>
        private void ExecuteTransition(ISyncState<TContext> newState)
        {
            if (newState == null || Current == newState) return;

            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();

            ExecuteExitLifecycle(Current, _transitionCts.Token);

            if (_stateStack.Count > 0) _stateStack.Pop();
            _stateStack.Push(newState);

            ExecuteEntryLifecycle(newState, _transitionCts.Token);
        }

        /// <summary>
        /// Запускает выполнение логики входа в состояние, автоматически разделяя синхронный и асинхронный контексты.
        /// <br/><br/>
        /// Starts the execution of the state entry logic, automatically separating synchronous and asynchronous contexts.
        /// </summary>
        /// <param name="state">Целевой состояние для выполнения входа.<br/><br/>The target state to execute entry for.</param>
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