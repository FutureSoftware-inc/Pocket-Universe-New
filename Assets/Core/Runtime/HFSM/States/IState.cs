namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Базовый интерфейс-маркер для всех типов состояний в машине поведений.
    /// <br/><br/>
    /// Base marker interface for all state types within the behavior machine.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public interface IState<TContext> where TContext : class
    {
        void Update(TContext context);
        void FixedUpdate(TContext context);
        void LateUpdate(TContext context);
    }
}
