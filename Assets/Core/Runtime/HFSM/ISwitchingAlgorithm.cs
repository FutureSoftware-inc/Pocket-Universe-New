namespace Crystal.HFSM
{
    public interface ISwitchingAlgorithm<TContext> where TContext : class
    {
        IState<TContext> Current { get; }
        void Initialize(TContext context, IStateSwitсher<TContext> switсher);
        void Update();
        void FixedUpdate();
        void LateUpdate();
        void ExecuteSwitch<TState>() where TState : IState<TContext>;
    }
}

