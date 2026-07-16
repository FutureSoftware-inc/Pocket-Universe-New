using UnityEngine;

namespace Crystal.HFSM
{
    public class StateMachine<TContext, TSource> 
        where TContext : class
        where TSource : IStateSwitсher<TContext>
    {
        private IState<TContext> _current;


    }
}