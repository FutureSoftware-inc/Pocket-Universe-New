using System;

namespace CrystalEngine
{
    /// <summary>
    /// Центральный управляющий класс иерархической машины состояний (HFSM).
    /// Делегирует управление жизненным циклом и логику переходов выбранному алгоритму переключения.
    /// <br/><br/>
    /// The central controlling class of the hierarchical finite state machine (HFSM).
    /// Delegates lifecycle management and transition logic to the selected switching algorithm.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public sealed class StateMachine<TContext> : IStateSwitcher<TContext> where TContext : class
    {
        private readonly TContext _context;
        private readonly ISwitchingAlgorithm<TContext> _algorithm;

        /// <summary>
        /// Возвращает текущее активное состояние машины, запрашивая его у алгоритма переключения.
        /// <br/><br/>
        /// Gets the current active state of the machine by querying the switching algorithm.
        /// </summary>
        public ISyncState<TContext> ActiveState => _algorithm.Current;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StateMachine{TContext}"/> с заданным контекстом и стратегией переключения.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="StateMachine{TContext}"/> class with the specified context and switching strategy.
        /// </summary>
        /// <param name="context">Контекст данных для работы состояний. Не может быть null.<br/><br/>The data context for state operations. Cannot be null.</param>
        /// <param name="algorithm">Алгоритм, управляющий сменой состояний. Не может быть null.<br/><br/>The algorithm controlling state transitions. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если один из параметров равен null.<br/><br/>Thrown when one of the specified parameters is null.</exception>
        public StateMachine(TContext context, ISwitchingAlgorithm<TContext> algorithm)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _algorithm.Initialize(_context, this);
        }

        /// <summary>
        /// Запускает обновление логики текущего активного состояния через алгоритм переключения в цикле Update.
        /// <br/><br/>
        /// Triggers the logic update of the current active state through the switching algorithm during the Update cycle.
        /// </summary>
        public void Update() => _algorithm.Update();

        /// <summary>
        /// Запускает физическое обновление текущего активного состояния через алгоритм переключения в цикле FixedUpdate.
        /// <br/><br/>
        /// Triggers the physics update of the current active state through the switching algorithm during the FixedUpdate cycle.
        /// </summary>
        public void FixedUpdate() => _algorithm.FixedUpdate();

        /// <summary>
        /// Запускает позднее обновление текущего активного состояния через алгоритм переключения в цикле LateUpdate.
        /// <br/><br/>
        /// Triggers the late update of the current active state through the switching algorithm during the LateUpdate cycle.
        /// </summary>
        public void LateUpdate() => _algorithm.LateUpdate();

        /// <summary>
        /// Запрашивает у алгоритма принудительное переключение на указанный тип состояния <typeparamref name="TState"/>.
        /// <br/><br/>
        /// Requests the algorithm to forcefully switch to the specified state type <typeparamref name="TState"/>.
        /// </summary>
        /// <typeparam name="TState">Целевой тип состояния для перехода.<br/><br/>The target state type to transition into.</typeparam>
        public void SwitchTo<TState>() where TState : ISyncState<TContext>
        {
            _algorithm.ExecuteSwitch<TState>();
        }
    }
}