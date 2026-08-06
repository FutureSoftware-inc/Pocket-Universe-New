namespace CrystalEngine
{
    /// <summary>
    /// Определяет интерфейс состояния в иерархической машине состояний (HFSM).
    /// Управляет жизненным циклом логики конкретного состояния с доступом к контексту данных.
    /// <br/><br/>
    /// Defines the state interface within the hierarchical finite state machine (HFSM).
    /// Manages the logic lifecycle of a specific state with access to the data context.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public interface ISyncState<TContext> : IState<TContext> where TContext : class
    {
        /// <summary>
        /// Вызывается один раз при переходе в (активации) это состояние.
        /// <br/><br/>
        /// Called once when entering (activating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний.<br/><br/>The data context of the current state machine.</param>
        void Entry(TContext context);

        /// <summary>
        /// Вызывается один раз при выходе из (деактивации) этого состояния.
        /// <br/><br/>
        /// Called once when exiting (deactivating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний.<br/><br/>The data context of the current state machine.</param>
        void Exit(TContext context);
    }
}
