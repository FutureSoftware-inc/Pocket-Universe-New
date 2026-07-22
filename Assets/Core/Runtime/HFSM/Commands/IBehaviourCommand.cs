namespace Crystal.HFSM
{
    public interface IBehaviourCommand<TContext> where TContext : class
    {
        void Execute(TContext context);
    }
}