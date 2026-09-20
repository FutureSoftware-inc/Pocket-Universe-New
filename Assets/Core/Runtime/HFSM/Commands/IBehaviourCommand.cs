namespace CrystalEngine
{
    /// <summary>
    /// Определяет интерфейс команды для выполнения атомарных игровых действий или операций над контекстом.
    /// Используется для вынесения конкретного поведения в изолированные переиспользуемые классы (паттерн Команда).
    /// <br/><br/>
    /// Defines a command interface for executing atomic game actions or operations on a context.
    /// Used to extract specific behaviour into isolated, reusable classes (Command pattern).
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public interface IBehaviourCommand<TContext> where TContext : class
    {
        /// <summary>
        /// Выполняет заложенное логическое действие, используя предоставленный контекст данных.
        /// <br/><br/>
        /// Executes the encapsulated logical action using the provided data context.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        void Execute(TContext context);
    }
}