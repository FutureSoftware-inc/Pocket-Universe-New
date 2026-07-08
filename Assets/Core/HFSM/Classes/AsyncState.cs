using System;
using System.Threading;
using System.Threading.Tasks;

namespace Crystal.HFSM
{
    public abstract class AsyncState<TContext> : IAsyncState<TContext> where TContext : class
    {
        public event Action<TContext> OnEntered;
        public event Action<TContext> OnExited;
        public event Action<TContext> OnUpdated;
        public event Action<TContext> OnFixedUpdated;
        public event Action<TContext> OnLateUpdated;
        
        public virtual Task EntryAsync(TContext context, CancellationToken token)
        {
            OnEntered?.Invoke(context);
            return Task.CompletedTask;
        }

        public virtual Task ExitAsync(TContext context, CancellationToken token)
        {
            OnExited?.Invoke(context);
            return Task.CompletedTask;
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