using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Представляет универсальное числовое значение с явным распределением памяти в стиле Union.
    /// Позволяет хранить и производить операции над любым базовым числовым типом без упаковки (boxing).
    /// <br/><br/>
    /// Represents a universal numeric value with explicit memory layout similar to a C-style Union.
    /// Allows storing and operating on any primitive numeric type without boxing.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct AnyNumber : IComparable
    {
        [FieldOffset(0)][SerializeField] private byte _asByte;
        [FieldOffset(0)][SerializeField] private sbyte _asSByte;
        [FieldOffset(0)][SerializeField] private ushort _asUInt16;
        [FieldOffset(0)][SerializeField] private short _asInt16;
        [FieldOffset(0)][SerializeField] private uint _asUInt32;
        [FieldOffset(0)][SerializeField] private int _asInt32;
        [FieldOffset(0)][SerializeField] private ulong _asUInt64;
        [FieldOffset(0)][SerializeField] private long _asInt64;
        [FieldOffset(0)][SerializeField] private float _asSingle;
        [FieldOffset(0)][SerializeField] private double _asDouble;

        /// <summary>
        /// Текущий сохраненный тип числового значения. Смещен на 8 байт для исключения перекрытия с данными.
        /// <br/><br/>
        /// The current stored type of the numeric value. Offset by 8 bytes to avoid overlapping with data fields.
        /// </summary>
        [FieldOffset(8)][SerializeField] private NumericType _currentType;

        /// <summary>
        /// Инициализирует число со значением типа byte.
        /// <br/><br/>
        /// Initializes the number with a byte value.
        /// </summary>
        public AnyNumber(byte value) : this() { _asByte = value; _currentType = NumericType.Byte; }

        /// <summary>
        /// Инициализирует число со значением типа sbyte.
        /// <br/><br/>
        /// Initializes the number with an sbyte value.
        /// </summary>
        public AnyNumber(sbyte value) : this() { _asSByte = value; _currentType = NumericType.SByte; }

        /// <summary>
        /// Инициализирует число со значением типа ushort.
        /// <br/><br/>
        /// Initializes the number with a ushort value.
        /// </summary>
        public AnyNumber(ushort value) : this() { _asUInt16 = value; _currentType = NumericType.UInt16; }

        /// <summary>
        /// Инициализирует число со значением типа short.
        /// <br/><br/>
        /// Initializes the number with a short value.
        /// </summary>
        public AnyNumber(short value) : this() { _asInt16 = value; _currentType = NumericType.Int16; }

        /// <summary>
        /// Инициализирует число со значением типа uint.
        /// <br/><br/>
        /// Initializes the number with a uint value.
        /// </summary>
        public AnyNumber(uint value) : this() { _asUInt32 = value; _currentType = NumericType.UInt32; }

        /// <summary>
        /// Инициализирует число со значением типа int.
        /// <br/><br/>
        /// Initializes the number with an int value.
        /// </summary>
        public AnyNumber(int value) : this() { _asInt32 = value; _currentType = NumericType.Int32; }

        /// <summary>
        /// Инициализирует число со значением типа ulong.
        /// <br/><br/>
        /// Initializes the number with a ulong value.
        /// </summary>
        public AnyNumber(ulong value) : this() { _asUInt64 = value; _currentType = NumericType.UInt64; }

        /// <summary>
        /// Инициализирует число со значением типа long.
        /// <br/><br/>
        /// Initializes the number with a long value.
        /// </summary>
        public AnyNumber(long value) : this() { _asInt64 = value; _currentType = NumericType.Int64; }

        /// <summary>
        /// Инициализирует число со значением типа float.
        /// <br/><br/>
        /// Initializes the number with a float value.
        /// </summary>
        public AnyNumber(float value) : this() { _asSingle = value; _currentType = NumericType.Single; }

        /// <summary>
        /// Инициализирует число со значением типа double.
        /// <br/><br/>
        /// Initializes the number with a double value.
        /// </summary>
        public AnyNumber(double value) : this() { _asDouble = value; _currentType = NumericType.Double; }

        /// <summary>
        /// Возвращает текущий тип данных, сохраненный в структуре.
        /// <br/><br/>
        /// Returns the current data type stored within the structure.
        /// </summary>
        public NumericType CurrentType => _currentType;

        /// <summary>
        /// Возвращает сохраненное значение в виде объекта (приводит к упаковке).
        /// <br/><br/>
        /// Returns the stored value boxed as an object.
        /// </summary>
        public object Value => _currentType switch
        {
            NumericType.Byte => _asByte,
            NumericType.SByte => _asSByte,
            NumericType.UInt16 => _asUInt16,
            NumericType.Int16 => _asInt16,
            NumericType.UInt32 => _asUInt32,
            NumericType.Int32 => _asInt32,
            NumericType.UInt64 => _asUInt64,
            NumericType.Int64 => _asInt64,
            NumericType.Single => _asSingle,
            NumericType.Double => _asDouble,
            _ => 0f
        };

        /// <summary>
        /// Возвращает системный тип System.Type сохраненного числового значения.
        /// <br/><br/>
        /// Returns the System.Type of the currently stored numeric value.
        /// </summary>
        public Type ValueType => _currentType switch
        {
            NumericType.Byte => typeof(byte),
            NumericType.SByte => typeof(sbyte),
            NumericType.UInt16 => typeof(ushort),
            NumericType.Int16 => typeof(short),
            NumericType.UInt32 => typeof(uint),
            NumericType.Int32 => typeof(int),
            NumericType.UInt64 => typeof(ulong),
            NumericType.Int64 => typeof(long),
            NumericType.Single => typeof(float),
            NumericType.Double => typeof(double),
            _ => typeof(float)
        };

        /// <summary>
        /// Сравнивает текущее числовое значение с другим объектом, приводя оба значения к типу double.
        /// <br/><br/>
        /// Compares the current numeric value with another object by converting both to double.
        /// </summary>
        public int CompareTo(object obj)
        {
            object rawRight = obj is AnyNumber any ? any.Value : obj;
            double left = Convert.ToDouble(Value);
            double right = Convert.ToDouble(rawRight);
            return left.CompareTo(right);
        }

        /// <summary>
        /// Проверяет эквивалентность текущего объекта с переданным объектом.
        /// <br/><br/>
        /// Checks equality between the current object and the specified object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is AnyNumber other && this == other)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает хэш-код текущего значения, вычисленный на основе его представления в формате double.
        /// <br/><br/>
        /// Returns the hash code for the current value, calculated based on its double representation.
        /// </summary>
        public override int GetHashCode()
        {
            return Convert.ToDouble(Value).GetHashCode();
        }

        /// <summary>
        /// Преобразует числовое значение в его строковое представление.
        /// <br/><br/>
        /// Converts the numeric value to its string representation.
        /// </summary>
        public override string ToString()
        {
            return Convert.ToDouble(Value).ToString();
        }

        public static implicit operator AnyNumber(sbyte value) => new AnyNumber(value);
        public static implicit operator AnyNumber(byte value) => new AnyNumber(value);
        public static implicit operator AnyNumber(ushort value) => new AnyNumber(value);
        public static implicit operator AnyNumber(short value) => new AnyNumber(value);
        public static implicit operator AnyNumber(uint value) => new AnyNumber(value);
        public static implicit operator AnyNumber(int value) => new AnyNumber(value);
        public static implicit operator AnyNumber(ulong value) => new AnyNumber(value);
        public static implicit operator AnyNumber(long value) => new AnyNumber(value);
        public static implicit operator AnyNumber(float value) => new AnyNumber(value);
        public static implicit operator AnyNumber(double value) => new AnyNumber(value);

        public static bool operator ==(AnyNumber left, AnyNumber right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(AnyNumber left, AnyNumber right)
        {
            return left.CompareTo(right) != 0;
        }

        public static bool operator <(AnyNumber left, AnyNumber right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(AnyNumber left, AnyNumber right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(AnyNumber left, AnyNumber right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(AnyNumber left, AnyNumber right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static AnyNumber operator +(AnyNumber left, AnyNumber right)
        {
            double result = Convert.ToDouble(left.Value) + Convert.ToDouble(right.Value);
            return CreatePromotedNubmer(left._currentType, right._currentType, result);
        }

        public static AnyNumber operator -(AnyNumber left, AnyNumber right)
        {
            double result = Convert.ToDouble(left.Value) - Convert.ToDouble(right.Value);
            return CreatePromotedNubmer(left._currentType, right._currentType, result);
        }

        public static AnyNumber operator *(AnyNumber left, AnyNumber right)
        {
            double result = Convert.ToDouble(left.Value) * Convert.ToDouble(right.Value);
            return CreatePromotedNubmer(left._currentType, right._currentType, result);
        }

        public static AnyNumber operator /(AnyNumber left, AnyNumber right)
        {
            double result = Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value);
            return CreatePromotedNubmer(left._currentType, right._currentType, result);
        }

        /// <summary>
        /// Создает новый экземпляр AnyNumber с автоматическим повышением результирующего типа данных на основе типов исходных операндов.
        /// Если хотя бы один операнд был Double, результат будет Double. Если Single (float) — результат Single. В остальных случаях — Int64 (long).
        /// <br/><br/>
        /// Creates a new AnyNumber instance with automatic promotion of the resulting data type based on the types of the source operands.
        /// If at least one operand was Double, the result is Double. If Single (float), the result is Single. Otherwise, it is Int64 (long).
        /// </summary>
        private static AnyNumber CreatePromotedNubmer(NumericType type1, NumericType type2, double value)
        {
            if (type1 == NumericType.Double || type2 == NumericType.Double)
            {
                return new AnyNumber(value);
            }
            if (type1 == NumericType.Single || type2 == NumericType.Single)
            {
                return new AnyNumber((float)value);
            }
            return new AnyNumber((long)value);
        }
    }
}