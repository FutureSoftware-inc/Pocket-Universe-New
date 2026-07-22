using System;
using System.Collections.Generic;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    public class State<TContext> : IState<TContext> where TContext : class
    {
        [SerializeField] private string _stateName;

        [SerializeReferenceSelector]
        [SerializeField] private List<Transition<TContext>> _transitions = new();

        [SerializeReferenceSelector]
        [SerializeField] private List<IBehaviourCommand<TContext>> _entryCommands;

        [SerializeReferenceSelector]
        [SerializeField] private List<IBehaviourCommand<TContext>> _exitCommands;

        [SerializeReferenceSelector]
        [SerializeField] private List<IBehaviourCommand<TContext>> _updateCommands;

        [SerializeReferenceSelector]
        [SerializeField] private List<IBehaviourCommand<TContext>> _fixedUpdateCommands;

        [SerializeReferenceSelector]
        [SerializeField] private List<IBehaviourCommand<TContext>> _lateUpdateCommands;

        private StateMachine<TContext> _subStateMachine;

        public string StateName => _stateName;
        public IReadOnlyList<Transition<TContext>> Transitions => _transitions;

        public event Action<IState<TContext>, TContext> OnEntered;
        public event Action<IState<TContext>, TContext> OnExited;
        public event Action<IState<TContext>, TContext> OnUpdated;
        public event Action<IState<TContext>, TContext> OnFixedUpdated;
        public event Action<IState<TContext>, TContext> OnLateUpdated;

        public virtual void Entry(TContext context)
        {
            for (int i = 0; i < _entryCommands.Count; i++)
            {
                _entryCommands[i]?.Execute(context);
            }
            _subStateMachine?.ActiveState?.Entry(context);
            OnEntered?.Invoke(this, context);
        }

        public virtual void Exit(TContext context)
        {
            for (int i = 0; i < _exitCommands.Count; i++)
            {
                _exitCommands[i]?.Execute(context);
            }
            _subStateMachine?.ActiveState?.Exit(context);
            OnExited?.Invoke(this, context);
        }

        public virtual void Update(TContext context)
        {
            for (int i = 0; i < _updateCommands.Count; i++)
            {
                _updateCommands[i]?.Execute(context);
            }
            _subStateMachine?.Update();
            OnUpdated?.Invoke(this, context);
        }

        public virtual void FixedUpdate(TContext context)
        {
            for (int i = 0; i < _fixedUpdateCommands.Count; i++)
            {
                _fixedUpdateCommands[i]?.Execute(context);
            }
            _subStateMachine?.FixedUpdate();
            OnFixedUpdated?.Invoke(this, context);
        }

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