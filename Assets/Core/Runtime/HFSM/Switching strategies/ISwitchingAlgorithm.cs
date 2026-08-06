namespace CrystalEngine
{
    /// <summary>
    /// Определяет интерфейс алгоритма (стратегии) переключения состояний в машине состояний (HFSM).
    /// Отвечает за логику выбора, смены и обновления текущего активного состояния.
    /// <br/><br/>
    /// Defines an interface for a state switching algorithm (strategy) within the state machine (HFSM).
    /// Responsible for the logic of selecting, changing, and updating the currently active state.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public interface ISwitchingAlgorithm<TContext> where TContext : class
    {
        /// <summary>
        /// Возвращает текущее активное состояние, управляемое данным алгоритмом.
        /// <br/><br/>
        /// Gets the current active state managed by this algorithm.
        /// </summary>
        ISyncState<TContext> Current { get; }

        /// <summary>
        /// Инициализирует алгоритм переключения, связывая его с контекстом данных и компонентом переключения состояний.
        /// <br/><br/>
        /// Initializes the switching algorithm, linking it with the data context and the state switcher component.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <param name="switсher">Компонент, осуществляющий непосредственную смену состояний.<br/><br/>The component that performs the actual state switching.</param>
        void Initialize(TContext context, IStateSwitcher<TContext> switсher);

        /// <summary>
        /// Обновляет логику алгоритма переключения каждый кадр (Update).
        /// <br/><br/>
        /// Updates the switching algorithm logic every frame (Update).
        /// </summary>
        void Update();

        /// <summary>
        /// Обновляет логику алгоритма переключения каждый физический кадр (FixedUpdate).
        /// <br/><br/>
        /// Updates the switching algorithm logic every physics frame (FixedUpdate).
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// Обновляет логику алгоритма переключения в конце кадра (LateUpdate).
        /// <br/><br/>
        /// Updates the switching algorithm logic at the end of the frame (LateUpdate).
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// Выполняет непосредственную смену текущего состояния на состояние типа <typeparamref name="TState"/>.
        /// <br/><br/>
        /// Executes the actual transition from the current state to a state of type <typeparamref name="TState"/>.
        /// </summary>
        /// <typeparam name="TState">Целевой тип состояния, реализующий <see cref="ISyncState{TContext}"/>.<br/><br/>The target state type implementing <see cref="ISyncState{TContext}"/>.</typeparam>
        void ExecuteSwitch<TState>() where TState : ISyncState<TContext>;
    }
}
