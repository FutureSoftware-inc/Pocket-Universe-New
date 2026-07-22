namespace CrystalEngine
{
    /// <summary>
    /// Перечисление поддерживаемых встроенных числовых типов данных.
    /// <br/><br/>
    /// Enumeration of supported built-in numeric data types.
    /// </summary>
    public enum NumericType : int
    {
        /// <summary>
        /// Беззнаковое 8-битное целое число (byte).
        /// <br/><br/>
        /// Unsigned 8-bit integer (byte).
        /// </summary>
        Byte = 0,

        /// <summary>
        /// Знаковое 8-битное целое число (sbyte).
        /// <br/><br/>
        /// Signed 8-bit integer (sbyte).
        /// </summary>
        SByte = 1,

        /// <summary>
        /// Беззнаковое 16-битное целое число (ushort).
        /// <br/><br/>
        /// Unsigned 16-bit integer (ushort).
        /// </summary>
        UInt16 = 2,

        /// <summary>
        /// Знаковое 16-битное целое число (short).
        /// <br/><br/>
        /// Signed 16-bit integer (short).
        /// </summary>
        Int16 = 3,

        /// <summary>
        /// Беззнаковое 32-битное целое число (uint).
        /// <br/><br/>
        /// Unsigned 32-bit integer (uint).
        /// </summary>
        UInt32 = 4,

        /// <summary>
        /// Знаковое 32-битное целое число (int).
        /// <br/><br/>
        /// Signed 32-bit integer (int).
        /// </summary>
        Int32 = 5,

        /// <summary>
        /// Беззнаковое 64-битное целое число (ulong).
        /// <br/><br/>
        /// Unsigned 64-bit integer (ulong).
        /// </summary>
        UInt64 = 6,

        /// <summary>
        /// Знаковое 64-битное целое число (long).
        /// <br/><br/>
        /// Signed 64-bit integer (long).
        /// </summary>
        Int64 = 7,

        /// <summary>
        /// Число с плавающей запятой одинарной точности (float).
        /// <br/><br/>
        /// Single-precision floating-point number (float).
        /// </summary>
        Single = 8,

        /// <summary>
        /// Число с плавающей запятой двойной точности (double).
        /// <br/><br/>
        /// Double-precision floating-point number (double).
        /// </summary>
        Double = 9,
    }
}
