namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Определяет интерфейс состояния в иерархической машине состояний (HFSM).
    /// Управляет жизненным циклом логики конкретного состояния с доступом к контексту данных.
    /// <br/><br/>
    /// Defines the state interface within the hierarchical finite state machine (HFSM).
    /// Manages the logic lifecycle of a specific state with access to the data context.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний. / The type of the data context class for the states.</typeparam>
    public interface IState<TContext> where TContext : class
    {
        /// <summary>
        /// Вызывается один раз при переходе в (активации) это состояние.
        /// <br/><br/>
        /// Called once when entering (activating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void Entry(TContext context);

        /// <summary>
        /// Вызывается один раз при выходе из (деактивации) этого состояния.
        /// <br/><br/>
        /// Called once when exiting (deactivating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void Exit(TContext context);

        /// <summary>
        /// Вызывается каждый кадр игрового цикла (Update) пока состояние активно.
        /// <br/><br/>
        /// Called every frame of the game loop (Update) while the state is active.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void Update(TContext context);

        /// <summary>
        /// Вызывается каждый кадр физического цикла (FixedUpdate) пока состояние активно.
        /// <br/><br/>
        /// Called every physics frame (FixedUpdate) while the state is active.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void FixedUpdate(TContext context);

        /// <summary>
        /// Вызывается в конце кадра игрового цикла (LateUpdate) пока состояние активно.
        /// <br/><br/>
        /// Called at the end of every frame loop (LateUpdate) while the state is active.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void LateUpdate(TContext context);
    }
}
