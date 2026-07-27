using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Абстрактная базовая реализация асинхронного состояния для иерархической машины состояний (HFSM).
    /// Позволяет интегрировать асинхронные операции и команды с поддержкой отмены через CancellationToken на этапах входа и выхода.
    /// <br/><br/>
    /// Abstract base implementation of an asynchronous state for a hierarchical finite state machine (HFSM).
    /// Integrates asynchronous operations and commands with cancellation support via a CancellationToken during entry and exit stages.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    [Serializable]
    public abstract class AsyncState<TContext> : IAsyncState<TContext> where TContext : class
    {
        /// <summary>
        /// Имя состояния для его идентификации и отладки в инспекторе или графе.
        /// <br/><br/>
        /// The name of the state for identification and debugging in the Inspector or graph.
        /// </summary>
        [SerializeReference] private string _stateName;

        /// <summary>
        /// Список переходов, ведущих из данного состояния в другие.
        /// <br/><br/>
        /// The list of transitions leading from this state to others.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<Transition<TContext>> _transitions = new();

        /// <summary>
        /// Список команд, выполняемых при асинхронном входе в состояние.
        /// <br/><br/>
        /// The list of commands executed upon asynchronous state entry.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<IBehaviourCommand<TContext>> _entryCommands;

        /// <summary>
        /// Список команд, выполняемых при асинхронном выходе из состояния.
        /// <br/><br/>
        /// The list of commands executed upon asynchronous state exit.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<IBehaviourCommand<TContext>> _exitCommands;

        /// <summary>
        /// Список команд, выполняемых каждый кадр в цикле Update.
        /// <br/><br/>
        /// The list of commands executed every frame during the Update cycle.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<IBehaviourCommand<TContext>> _updateCommands;

        /// <summary>
        /// Список команд, выполняемых каждый физический кадр в цикле FixedUpdate.
        /// <br/><br/>
        /// The list of commands executed every physics frame during the FixedUpdate cycle.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<IBehaviourCommand<TContext>> _fixedUpdateCommands;

        /// <summary>
        /// Список команд, выполняемых в конце каждого кадра в цикле LateUpdate.
        /// <br/><br/>
        /// The list of commands executed at the end of every frame during the LateUpdate cycle.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<IBehaviourCommand<TContext>> _lateUpdateCommands;

        /// <summary>
        /// Вложенная дочерняя машина состояний (обеспечивает иерархичность структуры HFSM).
        /// <br/><br/>
        /// The nested child state machine (provides the hierarchical structure of the HFSM).
        /// </summary>
        private StateMachine<TContext> _subStateMachine;

        /// <summary>
        /// Возвращает имя состояния.
        /// <br/><br/>
        /// Gets the name of the state.
        /// </summary>
        public string StateName => _stateName;

        /// <summary>
        /// Возвращает доступный только для чтения список исходящих переходов.
        /// <br/><br/>
        /// Gets the read-only list of outgoing transitions.
        /// </summary>
        public IReadOnlyList<Transition<TContext>> Transitions => _transitions;

        /// <summary>
        /// Вызывается сразу после завершения логики асинхронного входа в состояние.
        /// <br/><br/>
        /// Invoked immediately after the asynchronous state entry logic is completed.
        /// </summary>
        public event Action<IAsyncState<TContext>, TContext> OnEntered;

        /// <summary>
        /// Вызывается сразу после завершения логики асинхронного выхода из состояния.
        /// <br/><br/>
        /// Invoked immediately after the asynchronous state exit logic is completed.
        /// </summary>
        public event Action<IAsyncState<TContext>, TContext> OnExited;

        /// <summary>
        /// Вызывается каждый кадр после выполнения регулярных команд обновления состояния.
        /// <br/><br/>
        /// Invoked every frame after the regular state update commands are executed.
        /// </summary>
        public event Action<IAsyncState<TContext>, TContext> OnUpdated;

        /// <summary>
        /// Вызывается каждый физический кадр после выполнения команд физического обновления состояния.
        /// <br/><br/>
        /// Invoked every physics frame after the state physics update commands are executed.
        /// </summary>
        public event Action<IAsyncState<TContext>, TContext> OnFixedUpdated;

        /// <summary>
        /// Вызывается в конце кадра после выполнения команд финального обновления состояния.
        /// <br/><br/>
        /// Invoked at the end of the frame after the final state update commands are executed.
        /// </summary>
        public event Action<IAsyncState<TContext>, TContext> OnLateUpdated;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AsyncState{TContext}"/> с указанным именем.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="AsyncState{TContext}"/> class with the specified name.
        /// </summary>
        /// <param name="stateName">Имя состояния.<br/><br/>The name of the state.</param>
        public AsyncState(string stateName) => _stateName = stateName;

        /// <summary>
        /// Асинхронно активирует состояние, поочередно проверяя токен отмены, выполняя входные команды и логику вложенной машины.
        /// <br/><br/>
        /// Asynchronously activates the state, checking the cancellation token, executing entry commands, and nested machine logic.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <param name="token">Токен отмены асинхронной операции.<br/><br/>The cancellation token for the operation.</param>
        /// <returns>Задача, представляющая процесс входа в состояние.<br/><br/>A task representing the state entry process.</returns>
        public async UniTask EntryAsync(TContext context, CancellationToken token)
        {
            for (int i = 0; i < _entryCommands.Count; i++)
            {
                if (token.IsCancellationRequested) return;
                _entryCommands[i]?.Execute(context);
            }
            _subStateMachine?.ActiveState?.Entry(context);
            OnEntered?.Invoke(this, context);
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Асинхронно деактивирует состояние, поочередно проверяя токен отмены, выполняя команды выхода и логику вложенной машины.
        /// <br/><br/>
        /// Asynchronously deactivates the state, checking the cancellation token, executing exit commands, and nested machine logic.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <param name="token">Токен отмены асинхронной операции.<br/><br/>The cancellation token for the operation.</param>
        /// <returns>Задача, представляющая процесс выхода из состояния.<br/><br/>A task representing the state exit process.</returns>
        public async UniTask ExitAsync(TContext context, CancellationToken token)
        {
            for (int i = 0; i < _exitCommands.Count; i++)
            {
                if (token.IsCancellationRequested) return;
                _exitCommands[i]?.Execute(context);
            }
            _subStateMachine?.ActiveState?.Exit(context);
            OnExited?.Invoke(this, context);
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Обновляет логику состояния каждый кадр, выполняя регулярные команды и обновляя вложенную машину состояний.
        /// <br/><br/>
        /// Updates the state logic every frame, executing regular commands and updating the nested state machine.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        public void Update(TContext context)
        {
            for (int i = 0; i < _updateCommands.Count; i++)
            {
                _updateCommands[i]?.Execute(context);
            }
            _subStateMachine?.Update();
            OnUpdated?.Invoke(this, context);
        }

        /// <summary>
        /// Физический апдейт состояния, выполняющий команды физики и обновляющий вложенную машину состояний.
        /// <br/><br/>
        /// Physics update of the state, executing physics commands and updating the nested state machine.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        public virtual void FixedUpdate(TContext context)
        {
            for (int i = 0; i < _fixedUpdateCommands.Count; i++)
            {
                _fixedUpdateCommands[i]?.Execute(context);
            }
            _subStateMachine?.FixedUpdate();
            OnFixedUpdated?.Invoke(this, context);
        }

        /// <summary>
        /// Позднее обновление состояния, выполняющее финальные кадры логики и обновляющее вложенную машину состояний.
        /// <br/><br/>
        /// Late update of the state, executing final frame logic and updating the nested state machine.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        public virtual void LateUpdate(TContext context)
        {
            for (int i = 0; i < _lateUpdateCommands.Count; i++)
            {
                _lateUpdateCommands[i]?.Execute(context);
            }
            _subStateMachine?.LateUpdate();
            OnLateUpdated?.Invoke(this, context);
        }
    }
}

