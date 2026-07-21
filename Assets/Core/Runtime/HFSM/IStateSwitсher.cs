namespace Crystal.HFSM
{
    public interface IStateSwitсher<TContext> where TContext : class
    {
        void SwitchTo<TState>() where TState : IState<TContext>;
    }
}