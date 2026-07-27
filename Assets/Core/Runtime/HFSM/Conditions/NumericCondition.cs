using System;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Конкретная реализация условия, выполняющая математическое сравнение числовых значений в Blackboard.
    /// Автоматически извлекает и сопоставляет данные нужного примитивного типа, используя универсальный контейнер AnyNumber.
    /// <br/><br/>
    /// A concrete condition implementation that performs mathematical comparison of numeric values within the Blackboard.
    /// Automatically retrieves and matches data of the required primitive type using the universal AnyNumber container.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных, реализующий <see cref="IBlackboardProvider"/>. / The type of the data context class implementing <see cref="IBlackboardProvider"/>.</typeparam>
    [Serializable]
    public sealed class NumericCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        /// <summary>
        /// Числовое значение для сравнения, тип которого определяет формат запрашиваемых данных из Blackboard.
        /// <br/><br/>
        /// The numeric value for comparison, the type of which determines the format of the data requested from the Blackboard.
        /// </summary>
        [SerializeField] private Union _selectionValue;

        /// <summary>
        /// Критерий математического сравнения (например: Равно, Меньше, Больше либо их комбинации).
        /// <br/><br/>
        /// The mathematical comparison criteria (e.g., Equal, Less, Greater, or their combinations).
        /// </summary>
        [SerializeField] private ComparisonType _comparisonType = ComparisonType.None;

        /// <summary>
        /// Извлекает текущее значение из Blackboard на основе ожидаемого типа и выполняет выбранную математическую проверку.
        /// <br/><br/>
        /// Retrieves the current value from the Blackboard based on the expected type and performs the selected mathematical evaluation.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний, содержащий Blackboard. / The state machine data context containing the Blackboard.</param>
        /// <returns>True, если результат сравнения истинен; иначе false. / True if the comparison result evaluates to true; otherwise, false.</returns>
        protected override bool Evaluate(TContext context)
        {
            if (_comparisonType == ComparisonType.None) return false;
            Union currentValue = GetValueFromBlackboard(context.Blackboard);
            return _comparisonType switch
            {
                ComparisonType.Equal => _selectionValue == currentValue,
                ComparisonType.Less => _selectionValue < currentValue,
                ComparisonType.Greater => _selectionValue > currentValue,
                ComparisonType.Less | ComparisonType.Equal => _selectionValue <= currentValue,
                ComparisonType.Greater | ComparisonType.Equal => _selectionValue >= currentValue,
                _ => false
            };
        }

        /// <summary>
        /// Извлекает строго типизированное числовое значение из доски данных, динамически адаптируясь под заданный тип структуры AnyNumber.
        /// <br/><br/>
        /// Retrieves a strongly typed numeric value from the data board, dynamically adapting to the specified type of the AnyNumber structure.
        /// </summary>
        /// <param name="blackboard">Экземпляр доски данных для чтения числового свойства. / The blackboard instance to read the numeric property from.</param>
        /// <returns>Экземпляр универсального числа AnyNumber с прочитанным значением. / An AnyNumber instance holding the retrieved value.</returns>
        private Union GetValueFromBlackboard(Blackboard blackboard)
        {
            return _selectionValue.CurrentType switch
            {
                NumericType.SByte => blackboard.Get<sbyte>(PropertyName),
                NumericType.Byte => blackboard.Get<byte>(PropertyName),
                NumericType.Int16 => blackboard.Get<short>(PropertyName),
                NumericType.UInt16 => blackboard.Get<ushort>(PropertyName),
                NumericType.Int32 => blackboard.Get<int>(PropertyName),
                NumericType.UInt32 => blackboard.Get<uint>(PropertyName),
                NumericType.Int64 => blackboard.Get<long>(PropertyName),
                NumericType.UInt64 => blackboard.Get<ulong>(PropertyName),
                NumericType.Single => blackboard.Get<float>(PropertyName),
                NumericType.Double => blackboard.Get<double>(PropertyName),
                _ => blackboard.Get<float>(PropertyName)
            };
        }
    }
}
