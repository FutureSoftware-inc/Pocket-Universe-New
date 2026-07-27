namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Определяет интерфейс переключателя состояний в иерархической машине состояний (HFSM).
    /// Предоставляет контракт для выполнения прямых переходов между конкретными типами состояний.
    /// <br/><br/>
    /// Defines an interface for a state switcher within the hierarchical finite state machine (HFSM).
    /// Provides a contract for executing direct transitions between specific state types.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public interface IStateSwitcher<TContext> where TContext : class
    {
        /// <summary>
        /// Выполняет прямой переход машины состояний на указанный тип состояния <typeparamref name="TState"/>.
        /// <br/><br/>
        /// Executes a direct transition of the state machine to the specified state type <typeparamref name="TState"/>.
        /// </summary>
        /// <typeparam name="TState">Целевой тип состояния, реализующий <see cref="ISyncState{TContext}"/>.<br/><br/>The target state type implementing <see cref="ISyncState{TContext}"/>.</typeparam>
        void SwitchTo<TState>() where TState : ISyncState<TContext>;
    }
}
