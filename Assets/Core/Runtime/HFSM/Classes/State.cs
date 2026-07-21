using System;
using System.Collections.Generic;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    public abstract class State<TContext> : IState<TContext> where TContext : class
    {
        [SerializeReferenceSelector]
        [SerializeField] private List<Transition<TContext>> _transitions = new();

        public IReadOnlyList<Transition<TContext>> Transitions => _transitions;

        public event Action<IState<TContext>, TContext> OnEntered;
        public event Action<IState<TContext>, TContext> OnExited;
        public event Action<IState<TContext>, TContext> OnUpdated;
        public event Action<IState<TContext>, TContext> OnFixedUpdated;
        public event Action<IState<TContext>, TContext> OnLateUpdated;

        public virtual void Entry(TContext context)
        {
            OnEntered?.Invoke(this, context);
        }

        public virtual void Exit(TContext context)
        {
            OnExited?.Invoke(this, context);
        }

        public virtual void Update(TContext context)
        {
            OnUpdated?.Invoke(this, context);
        }

        public virtual void FixedUpdate(TContext context)
        {
            OnFixedUpdated?.Invoke(this, context);
        }

        public virtual void LateUpdate(TContext context)
        {
            OnLateUpdated?.Invoke(this, context);
        }
    }
}