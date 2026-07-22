namespace Crystal.HFSM
{
    public interface IState<TContext> where TContext : class
    {
        void Entry(TContext context);      
        void Exit(TContext context);
        void Update(TContext context);
        void FixedUpdate(TContext context);
        void LateUpdate(TContext context);
    }
}