using System;
using System.Collections.Generic;
using UnityEngine;
using CrystalEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Базовая реализация синхронного состояния для иерархической машины состояний (HFSM).
    /// Поддерживает выполнение списков команд на каждом этапе жизненного цикла и инкапсулирует вложенную под-машину состояний.
    /// <br/><br/>
    /// Base implementation of a synchronous state for a hierarchical finite state machine (HFSM).
    /// Supports executing lists of commands at each lifecycle stage and encapsulates a nested sub-state machine.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    [Serializable]
    public class SyncState<TContext> : ISyncState<TContext> where TContext : class
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
        /// Список команд, выполняемых один раз при входе в состояние.
        /// <br/><br/>
        /// The list of commands executed once upon entering the state.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeReference] private List<IBehaviourCommand<TContext>> _entryCommands;

        /// <summary>
        /// Список команд, выполняемых один раз при выходе из состояния.
        /// <br/><br/>
        /// The list of commands executed once upon exiting the state.
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
        /// Вызывается сразу после выполнения логики входа в состояние.
        /// <br/><br/>
        /// Invoked immediately after the state entry logic is executed.
        /// </summary>
        public event Action<ISyncState<TContext>, TContext> OnEntered;

        /// <summary>
        /// Вызывается сразу после выполнения логики выхода из состояния.
        /// <br/><br/>
        /// Invoked immediately after the state exit logic is executed.
        /// </summary>
        public event Action<ISyncState<TContext>, TContext> OnExited;

        /// <summary>
        /// Вызывается каждый кадр после выполнения регулярных команд обновления состояния.
        /// <br/><br/>
        /// Invoked every frame after the regular state update commands are executed.
        /// </summary>
        public event Action<ISyncState<TContext>, TContext> OnUpdated;

        /// <summary>
        /// Вызывается каждый физический кадр после выполнения команд физического обновления состояния.
        /// <br/><br/>
        /// Invoked every physics frame after the state physics update commands are executed.
        /// </summary>
        public event Action<ISyncState<TContext>, TContext> OnFixedUpdated;

        /// <summary>
        /// Вызывается в конце кадра после выполнения команд финального обновления состояния.
        /// <br/><br/>
        /// Invoked at the end of the frame after the final state update commands are executed.
        /// </summary>
        public event Action<ISyncState<TContext>, TContext> OnLateUpdated;

        /// <summary>
        /// Активирует состояние, последовательно выполняя входные команды, логику вложенной машины и вызывая событие входа.
        /// <br/><br/>
        /// Activates the state, sequentially executing entry commands, nested machine logic, and invoking the entry event.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        public virtual void Entry(TContext context)
        {
            for (int i = 0; i < _entryCommands.Count; i++)
            {
                _entryCommands[i]?.Execute(context);
            }
            _subStateMachine?.ActiveState?.Entry(context);
            OnEntered?.Invoke(this, context);
        }

        /// <summary>
        /// Деактивирует состояние, последовательно выполняя команды выхода, логику вложенной машины и вызывая событие выхода.
        /// <br/><br/>
        /// Deactivates the state, sequentially executing exit commands, nested machine logic, and invoking the exit event.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        public virtual void Exit(TContext context)
        {
            for (int i = 0; i < _exitCommands.Count; i++)
            {
                _exitCommands[i]?.Execute(context);
            }
            _subStateMachine?.ActiveState?.Exit(context);
            OnExited?.Invoke(this, context);
        }

        /// <summary>
        /// Обновляет логику состояния каждый кадр, выполняя регулярные команды и обновляя вложенную машину состояний.
        /// <br/><br/>
        /// Updates the state logic every frame, executing regular commands and updating the nested state machine.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        public virtual void Update(TContext context)
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
