using System;

namespace CrystalEngine
{
    /// <summary>
    /// Битовая маска типов сравнения числовых значений.
    /// Позволяет гибко комбинировать базовые условия проверки (меньше, равно, больше).
    /// <br/><br/>
    /// A bitmask of numeric comparison types.
    /// Allows flexible combination of basic evaluation conditions (less, equal, greater).
    /// </summary>
    [Flags]
    public enum ComparisonType : byte
    {
        /// <summary>
        /// Условие сравнения отсутствует.
        /// <br/><br/>
        /// No comparison condition specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Проверка на "меньше".
        /// <br/><br/>
        /// "Less than" evaluation.
        /// </summary>
        Less = 1 << 0,

        /// <summary>
        /// Проверка на "равно".
        /// <br/><br/>
        /// "Equal to" evaluation.
        /// </summary>
        Equal = 1 << 1,

        /// <summary>
        /// Проверка на "больше".
        /// <br/><br/>
        /// "Greater than" evaluation.
        /// </summary>
        Greater = 1 << 2,

        /// <summary>
        /// Проверка на "меньше или равно". Комбинация флагов Less и Equal.
        /// <br/><br/>
        /// "Less than or equal to" evaluation. A combination of Less and Equal flags.
        /// </summary>
        LessOrEqual = Less | Equal,

        /// <summary>
        /// Проверка на "больше или равно". Комбинация флагов Greater и Equal.
        /// <br/><br/>
        /// "Greater than or equal to" evaluation. A combination of Greater and Equal flags.
        /// </summary>
        GreaterOrEqual = Greater | Equal,

        /// <summary>
        /// Проверка на "не равно". Комбинация флагов Less и Greater.
        /// <br/><br/>
        /// "Not equal to" evaluation. A combination of Less and Greater flags.
        /// </summary>
        NotEqual = Less | Greater,

        /// <summary>
        /// Любое соответствие (меньше, равно или больше). Истинный результат при любом сравнении.
        /// <br/><br/>
        /// Any match (less, equal, or greater). Evaluates to true for any valid comparison.
        /// </summary>
        Any = Less | Equal | Greater
    }
}
