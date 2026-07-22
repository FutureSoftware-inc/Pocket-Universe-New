using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Алгоритм переключения состояний на основе очереди с полной поддержкой синхронных и асинхронных состояний.
    /// Позволяет выстраивать последовательности выполняемых стейтов, корректно управляя асинхронными процессами входа и выхода.
    /// <br/><br/>
    /// A queue-based state switching algorithm with full support for synchronous and asynchronous states.
    /// Allows queuing sequences of states to be executed sequentially while correctly managing asynchronous entry and exit processes.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний. / The type of the data context class for the states.</typeparam>
    public sealed class QueueSwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitcher<TContext> _switcher;
        private IState<TContext> _current;

        /// <summary>
        /// Очередь состояний, запланированных на последовательное выполнение.
        /// <br/><br/>
        /// The queue of states scheduled for sequential execution.
        /// </summary>
        private readonly Queue<IState<TContext>> _stateQueue = new();

        /// <summary>
        /// Внутренний реестр, связывающий системный тип состояния с его конкретным экземпляром.
        /// <br/><br/>
        /// Internal registry mapping a system state type to its specific instance.
        /// </summary>
        private readonly Dictionary<Type, IState<TContext>> _registry = new();

        /// <summary>
        /// Источник токенов отмены для контроля и прерывания текущих асинхронных операций входа и выхода.
        /// <br/><br/>
        /// The cancellation token source to control and interrupt current asynchronous entry and exit operations.
        /// </summary>
        private CancellationTokenSource _transitionCts;

        /// <summary>
        /// Возвращает текущее активное состояние.
        /// <br/><br/>
        /// Gets the current active state.
        /// </summary>
        public IState<TContext> Current => _current;

        /// <summary>
        /// Регистрирует экземпляр состояния в общем словаре алгоритма для его последующего добавления в очередь.
        /// Использует Fluent API, возвращая ссылку на самого себя.
        /// <br/><br/>
        /// Registers a state instance in the algorithm's internal dictionary for future queuing.
        /// Uses Fluent API by returning a reference to itself.
        /// </summary>
        /// <param name="state">Регистрируемый экземпляр состояния. / The state instance to register.</param>
        /// <returns>Текущий экземпляр алгоритма переключения. / The current switching algorithm instance.</returns>
        public QueueSwitchingAlgorithm<TContext> RegisterState(IState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
            return this;
        }

        /// <summary>
        /// Кэширует контекст данных и переключатель для внутренней работы алгоритма.
        /// <br/><br/>
        /// Caches the data context and the switcher for the algorithm's internal operations.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний. / The data context of the state machine.</param>
        /// <param name="switcher">Компонент переключения состояний. / The state switcher component.</param>
        public void Initialize(TContext context, IStateSwitcher<TContext> switcher)
        {
            _context = context;
            _switcher = switcher;
        }

        /// <summary>
        /// Добавляет состояние типа <typeparamref name="TState"/> в конец очереди выполнения.
        /// Если в данный момент нет активного состояния, автоматически запускает очередь.
        /// <br/><br/>
        /// Adds a state of type <typeparamref name="TState"/> to the end of the execution queue.
        /// If there is no active state currently, automatically advances and starts the queue.
        /// </summary>
        /// <typeparam name="TState">Тип состояния для добавления в очередь. / The type of the state to enqueue.</typeparam>
        public void EnqueueState<TState>() where TState : IState<TContext>
        {
            if (!_registry.TryGetValue(typeof(TState), out var newState)) return;

            _stateQueue.Enqueue(newState);

            if (_current == null)
            {
                AdvanceQueue();
            }
        }
        /// <summary>
        /// Извлекает и активирует следующее состояние из очереди, предварительно отменив прошлые операции и завершив работу текущего стейта.
        /// <br/><br/>
        /// Dequeues and activates the next state in the queue, after cancelling previous operations and exiting the current state.
        /// </summary>
        public void AdvanceQueue()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();

            ExecuteExitLifecycle(_current, _transitionCts.Token);

            if (_stateQueue.Count > 0)
            {
                _current = _stateQueue.Dequeue();
                ExecuteEntryLifecycle(_current, _transitionCts.Token);
            }
            else
            {
                _current = null;
            }
        }

        /// <summary>
        /// Полностью очищает очередь запланированных состояний, прерывает текущие переходы и выключает активное состояние.
        /// <br/><br/>
        /// Completely clears the queue of scheduled states, interrupts current transitions, and exits the active state.
        /// </summary>
        public void ClearQueue()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = new CancellationTokenSource();

            ExecuteExitLifecycle(_current, _transitionCts.Token);
            _current = null;
            _stateQueue.Clear();
        }

        /// <summary>
        /// Проверяет условия переходов текущего состояния (поддерживает State и AsyncState).
        /// При срабатывании перехода очередь очищается, и осуществляется безопасная смена активного стейта.
        /// <br/><br/>
        /// Evaluates transition conditions of the current state (supports both State and AsyncState).
        /// If a transition triggers, the queue is cleared and a safe active state transition is executed.
        /// </summary>
        public void Update()
        {
            if (_current == null) return;

            IReadOnlyList<Transition<TContext>> transitions = null;

            if (_current is State<TContext> polymorphicState)
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
                        _stateQueue.Clear();
                        ExecuteTransition(transition.TargetState);
                        break;
                    }
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
        /// Запускает мгновенную смену состояния на <typeparamref name="TState"/> в обход очереди, предварительно полностью очистив её.
        /// <br/><br/>
        /// Triggers an immediate state switch to <typeparamref name="TState"/> bypassing the queue, completely clearing it beforehand.
        /// </summary>
        /// <typeparam name="TState">Целевой тип состояния для перехода. / The target state type to transition into.</typeparam>
        public void ExecuteSwitch<TState>() where TState : IState<TContext>
        {
            if (!_registry.TryGetValue(typeof(TState), out var newState)) return;
            _stateQueue.Clear();
            ExecuteTransition(newState);
        }

        /// <summary>
        /// Внутренний метод смены состояний. Отменяет прошлые переходы, обрабатывает Exit/ExitAsync и запускает Entry/EntryAsync.
        /// <br/><br/>
        /// Internal state transition method. Cancels previous transitions, processes Exit/ExitAsync, and triggers Entry/EntryAsync.
        /// </summary>
        /// <param name="newState">Новое целевое состояние. / The new target state.</param>
        private void ExecuteTransition(IState<TContext> newState)
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
        /// <param name="state">Целевое состояние для выполнения входа. / The target state to execute entry for.</param>
        /// <param name="token">Токен отмены для асинхронной операции. / The cancellation token for the asynchronous operation.</param>
        private void ExecuteEntryLifecycle(IState<TContext> state, CancellationToken token = default)
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
        /// <param name="state">Целевое состояние для выполнения выхода. / The target state to execute exit for.</param>
        /// <param name="token">Токен отмены для асинхронной операции. / The cancellation token for the asynchronous operation.</param>
        private void ExecuteExitLifecycle(IState<TContext> state, CancellationToken token = default)
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
        /// <param name="task">Выполняемая асинхронная задача. / The asynchronous task to be executed.</param>
        /// <returns>Асинхронная задача. / An asynchronous task.</returns>
        private async Task ForgetTask(Task task)
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