using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CrystalEngine
{
    /// <summary>
    /// Абстрактный базовый класс для условий переходов в машине состояний.
    /// Позволяет проверять логические критерии с поддержкой инвертирования результата.
    /// <br/><br/>
    /// Abstract base class for transition conditions within the state machine.
    /// Allows evaluating logical criteria with support for result inversion.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    [Serializable]
    public abstract class Condition<TContext> where TContext : class
    {
        /// <summary>
        /// Имя проверяемого свойства или ключа (например, в Blackboard).
        /// <br/><br/>
        /// The name of the property or key to be evaluated (e.g., in a Blackboard).
        /// </summary>
        [FormerlySerializedAs("_propertyName")]
        [InspectorName("Property Name")]
        [SerializeField] private string _propertyName = "Property name";

        /// <summary>
        /// Флаг инверсии результата. Если true, условие вернет противоположное значение.
        /// <br/><br/>
        /// Result inversion flag. If true, the condition returns the opposite value.
        /// </summary>
        [SerializeField] private bool _invert = false;

        /// <summary>
        /// Возвращает имя проверяемого свойства.
        /// <br/><br/>
        /// Gets the name of the property being evaluated.
        /// </summary>
        public string PropertyName => _propertyName;

        /// <summary>
        /// Внутренний метод вычисления логического условия, реализуемый в конкретных подклассах.
        /// <br/><br/>
        /// Internal method for computing the logical condition, implemented by specific subclasses.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <returns>True, если условие выполнено; иначе false.<br/><br/>True if the condition is met; otherwise, false.</returns>
        protected abstract bool Evaluate(TContext context);

        /// <summary>
        /// Проверяет выполнение условия, автоматически применяя инверсию, если она включена.
        /// <br/><br/>
        /// Checks the evaluation of the condition, automatically applying inversion if it is enabled.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <returns>Результирующее логическое значение с учетом флага инверсии.<br/><br/>The resulting boolean value considering the inversion flag.</returns>
        public bool Check(TContext context)
        {
            return _invert ? !Evaluate(context) : Evaluate(context);
        }
    }
}