using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Класс, представляющий переход между состояниями в машине состояний (HFSM).
    /// Содержит список условий, выполнение которых необходимо для осуществления перехода.
    /// <br/><br/>
    /// A class representing a transition between states within the state machine (HFSM).
    /// Contains a list of conditions that must be met to trigger the transition.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний. / The type of the data context class for the states.</typeparam>
    [Serializable]
    public class Transition<TContext> where TContext : class
    {
        /// <summary>
        /// Имя целевого состояния, в которое ведет этот переход. Используется для настройки в инспекторе.
        /// <br/><br/>
        /// The name of the target state this transition leads to. Used for configuration in the Inspector.
        /// </summary>
        [SerializeField] private string _targetStateName;

        /// <summary>
        /// Список условий (логических критериев), которые должны одновременно выполниться для срабатывания перехода.
        /// <br/><br/>
        /// The list of conditions (logical criteria) that must simultaneously evaluate to true to trigger the transition.
        /// </summary>
        [SerializeReferenceSelector]
        [SerializeField] private List<Condition<TContext>> _conditions = new();

        /// <summary>
        /// Прямая ссылка на объект целевого состояния. Настраивается при инициализации графа.
        /// <br/><br/>
        /// A direct reference to the target state object. Resolved during graph initialization.
        /// </summary>
        [SerializeReferenceSelector]
        private IState<TContext> _targetState;

        /// <summary>
        /// Возвращает имя целевого состояния.
        /// <br/><br/>
        /// Gets the name of the target state.
        /// </summary>
        public string TargetStateName => _targetStateName;

        /// <summary>
        /// Возвращает прямую ссылку на целевое состояние.
        /// <br/><br/>
        /// Gets the direct reference to the target state.
        /// </summary>
        public IState<TContext> TargetState => _targetState;

        /// <summary>
        /// Инициализирует переход, связывая его с конкретным объектом целевого состояния.
        /// <br/><br/>
        /// Initializes the transition by linking it to a specific target state object.
        /// </summary>
        /// <param name="targetState">Объект целевого состояния. Не может быть null. / The target state object. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если переданный <paramref name="targetState"/> равен null. / Thrown when the specified <paramref name="targetState"/> is null.</exception>
        public void Initialize(IState<TContext> targetState)
        {
            _targetState = targetState ?? throw new ArgumentNullException(nameof(targetState));
        }

        /// <summary>
        /// Проверяет, готовы ли все условия для осуществления перехода в целевое состояние.
        /// <br/><br/>
        /// Checks whether all conditions are met to execute the transition to the target state.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний. / The data context of the state machine.</param>
        /// <returns>True, если условий нет или все они выполнились успешно; иначе false. / True if there are no conditions or all of them evaluated to true; otherwise, false.</returns>
        public bool ToTargetState(TContext context)
        {
            if (_conditions == null || _conditions.Count == 0) return true;

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (!_conditions[i].Check(context)) return false;
            }
            return true;
        }
    }
}
