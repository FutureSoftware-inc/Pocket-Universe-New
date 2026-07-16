using System;

namespace Crystal.HFSM
{
    public abstract class State<TContext> : IState<TContext> where TContext : class
    {
        public event Action<TContext> OnEntered;
        public event Action<TContext> OnExited;
        public event Action<TContext> OnUpdated;
        public event Action<TContext> OnFixedUpdated;
        public event Action<TContext> OnLateUpdated;

        public virtual void Entry(TContext context)
        {
            OnEntered?.Invoke(context);
        }

        public virtual void Exit(TContext context)
        {
            OnExited?.Invoke(context);
        }

        public virtual void Update(TContext context)
        {
            OnUpdated?.Invoke(context);
        }

        public virtual void FixedUpdate(TContext context)
        {
            OnFixedUpdated?.Invoke(context);
        }

        public virtual void LateUpdate(TContext context)
        {
            OnLateUpdated?.Invoke(context);
        }
    }
}