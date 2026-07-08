using UnityEngine;

namespace Crystal.HFSM
{
    public interface IStateSwitсher<TContext> where TContext : class
    {
        void SwichTo<TState>() where TState : IState<TContext>;
    }
}