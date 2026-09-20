using System;
using UnityEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Конкретный класс условия, проверяющий логические значения (bool) в Blackboard.
    /// <br/><br/>
    /// Concrete condition class that evaluates boolean values (bool) within the Blackboard.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных, реализующий интерфейс IBlackboardProvider.<br/><br/>The type of the data context class that implements the IBlackboardProvider interface.</typeparam>
    [Serializable]
    public sealed class BoolCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        /// <summary>
        /// Ожидаемое логическое значение, с которым будет сравниваться текущее значение свойства.
        /// <br/><br/>
        /// The expected boolean value against which the current property value will be compared.
        /// </summary>
        [SerializeField] private bool _expectedValue = false;

        /// <summary>
        /// Вычисляет логическое условие, извлекая значение типа bool из Blackboard по имени свойства.
        /// <br/><br/>
        /// Computes the logical condition by retrieving a bool value from the Blackboard using the property name.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний, предоставляющий доступ к Blackboard.<br/><br/>The data context of the state machine providing access to the Blackboard.</param>
        /// <returns>True, если текущее значение из Blackboard совпадает с ожидаемым; иначе false.<br/><br/>True if the current value from the Blackboard matches the expected value; otherwise, false.</returns>
        protected override bool Evaluate(TContext context)
        {
            bool currentValue = context.Blackboard.Get<bool>(PropertyName);
            return currentValue == _expectedValue;
        }
    }
}