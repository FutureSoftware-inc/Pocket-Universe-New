using Crystal.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Crystal.HFSM
{
    [Serializable]
    public abstract class AsyncState<TContext> : IAsyncState<TContext> where TContext : class
    {
        [SerializeReferenceSelector]
        [SerializeField] private List<Transition<TContext>> _transitions = new();

        public IReadOnlyList<Transition<TContext>> Transitions => _transitions;

        public event Action<IAsyncState<TContext>, TContext> OnEntered;
        public event Action<IAsyncState<TContext>, TContext> OnExited;
        public event Action<IAsyncState<TContext>, TContext> OnUpdated;
        public event Action<IAsyncState<TContext>, TContext> OnFixedUpdated;
        public event Action<IAsyncState<TContext>, TContext> OnLateUpdated;

        public virtual Task EntryAsync(TContext context, CancellationToken token)
        {
            OnEntered?.Invoke(this, context);
            return Task.CompletedTask;
        }

        public virtual Task ExitAsync(TContext context, CancellationToken token)
        {
            OnExited?.Invoke(this, context);
            return Task.CompletedTask;
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