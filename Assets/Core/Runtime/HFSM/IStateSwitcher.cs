namespace Crystal.HFSM
{
    public interface IStateSwitcher<TContext> where TContext : class
    {
        void SwitchTo<TState>() where TState : IState<TContext>;
    }
}