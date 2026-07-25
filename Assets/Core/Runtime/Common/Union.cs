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
    public struct Union : IComparable, IComparable<Union>, IEquatable<Union>, IConvertible
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
        public Union(byte value) : this() { _asByte = value; _currentType = NumericType.Byte; }

        /// <summary>
        /// Инициализирует число со значением типа sbyte.
        /// <br/><br/>
        /// Initializes the number with an sbyte value.
        /// </summary>
        public Union(sbyte value) : this() { _asSByte = value; _currentType = NumericType.SByte; }

        /// <summary>
        /// Инициализирует число со значением типа ushort.
        /// <br/><br/>
        /// Initializes the number with a ushort value.
        /// </summary>
        public Union(ushort value) : this() { _asUInt16 = value; _currentType = NumericType.UInt16; }

        /// <summary>
        /// Инициализирует число со значением типа short.
        /// <br/><br/>
        /// Initializes the number with a short value.
        /// </summary>
        public Union(short value) : this() { _asInt16 = value; _currentType = NumericType.Int16; }

        /// <summary>
        /// Инициализирует число со значением типа uint.
        /// <br/><br/>
        /// Initializes the number with a uint value.
        /// </summary>
        public Union(uint value) : this() { _asUInt32 = value; _currentType = NumericType.UInt32; }

        /// <summary>
        /// Инициализирует число со значением типа int.
        /// <br/><br/>
        /// Initializes the number with an int value.
        /// </summary>
        public Union(int value) : this() { _asInt32 = value; _currentType = NumericType.Int32; }

        /// <summary>
        /// Инициализирует число со значением типа ulong.
        /// <br/><br/>
        /// Initializes the number with a ulong value.
        /// </summary>
        public Union(ulong value) : this() { _asUInt64 = value; _currentType = NumericType.UInt64; }

        /// <summary>
        /// Инициализирует число со значением типа long.
        /// <br/><br/>
        /// Initializes the number with a long value.
        /// </summary>
        public Union(long value) : this() { _asInt64 = value; _currentType = NumericType.Int64; }

        /// <summary>
        /// Инициализирует число со значением типа float.
        /// <br/><br/>
        /// Initializes the number with a float value.
        /// </summary>
        public Union(float value) : this() { _asSingle = value; _currentType = NumericType.Single; }

        /// <summary>
        /// Инициализирует число со значением типа double.
        /// <br/><br/>
        /// Initializes the number with a double value.
        /// </summary>
        public Union(double value) : this() { _asDouble = value; _currentType = NumericType.Double; }

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
        /// <summary>
        /// Сравнивает текущее числовое значение с другим объектом. 
        /// Исключает упаковку, если передан AnyNumber, и сохраняет точность для целых чисел.
        /// </summary>
        public int CompareTo(object obj)
        {
            // По спецификации .NET любое значение больше null
            if (obj == null) return 1;

            // Если передан AnyNumber, вызываем быстрый метод без упаковки и потери точности
            if (obj is Union other)
            {
                return CompareTo(other);
            }

            // Если передан примитивный тип, пробуем сравнить его без конвертации в double,
            // чтобы не потерять точность больших целых чисел (long/ulong).
            try
            {
                // Преобразуем примитив в AnyNumber (неявные операторы это позволяют)
                // и вызываем безопасное сравнение
                switch (obj)
                {
                    case int i: return CompareTo((Union)i);
                    case long l: return CompareTo((Union)l);
                    case float f: return CompareTo((Union)f);
                    case double d: return CompareTo((Union)d);
                    case byte b: return CompareTo((Union)b);
                    case sbyte sb: return CompareTo((Union)sb);
                    case short s: return CompareTo((Union)s);
                    case ushort us: return CompareTo((Union)us);
                    case uint ui: return CompareTo((Union)ui);
                    case ulong ul: return CompareTo((Union)ul);
                }

                // Для редких типов (например, decimal или кастомных IConvertible) откатываемся на double
                double left = AsDouble();
                double right = Convert.ToDouble(obj);
                return left.CompareTo(right);
            }
            catch (Exception)
            {
                throw new ArgumentException($"[AnyNumber] Cannot compare AnyNumber with type {obj.GetType().Name}");
            }
        }

        public int CompareTo(Union other)
        {
            bool thisIsInt = IsIntegralType(_currentType);
            bool otherIsInt = IsIntegralType(other._currentType);
            if (thisIsInt && otherIsInt)
            {
                return CompareIntegrals(this, other);
            }
            double left = AsDouble();
            double right = other.AsDouble();
            return left.CompareTo(right);
        }

        /// <summary>
        /// Проверяет эквивалентность текущего объекта с переданным объектом.
        /// <br/><br/>
        /// Checks equality between the current object and the specified object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Union other)
            {
                return Equals(other);
            }
            switch (obj)
            {
                case int i: return Equals((Union)i);
                case long l: return Equals((Union)l);
                case float f: return Equals((Union)f);
                case double d: return Equals((Union)d);
                case byte b: return Equals((Union)b);
                case sbyte sb: return Equals((Union)sb);
                case short s: return Equals((Union)s);
                case ushort us: return Equals((Union)us);
                case uint ui: return Equals((Union)ui);
                case ulong ul: return Equals((Union)ul);
            }
            return false;
        }

        public bool Equals(Union other)
        {
            if (_currentType != other._currentType)
            {
                return CompareTo(other) == 0;
            }
            return _currentType switch
            {
                NumericType.Byte => _asByte == other._asByte,
                NumericType.SByte => _asSByte == other._asSByte,
                NumericType.UInt16 => _asUInt16 == other._asUInt16,
                NumericType.Int16 => _asInt16 == other._asInt16,
                NumericType.UInt32 => _asUInt32 == other._asUInt32,
                NumericType.Int32 => _asInt32 == other._asInt32,
                NumericType.UInt64 => _asUInt64 == other._asUInt64,
                NumericType.Int64 => _asInt64 == other._asInt64,
                NumericType.Single => _asSingle.Equals(other._asSingle),
                NumericType.Double => _asDouble.Equals(other._asDouble),
                _ => false
            };
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

        public static implicit operator Union(sbyte value) => new Union(value);
        public static implicit operator Union(byte value) => new Union(value);
        public static implicit operator Union(ushort value) => new Union(value);
        public static implicit operator Union(short value) => new Union(value);
        public static implicit operator Union(uint value) => new Union(value);
        public static implicit operator Union(int value) => new Union(value);
        public static implicit operator Union(ulong value) => new Union(value);
        public static implicit operator Union(long value) => new Union(value);
        public static implicit operator Union(float value) => new Union(value);
        public static implicit operator Union(double value) => new Union(value);

        public static explicit operator sbyte(Union number)
        {
            return number._currentType switch
            {
                NumericType.SByte => number._asSByte,
                NumericType.Byte => checked((sbyte)number._asByte),
                NumericType.Int16 => checked((sbyte)number._asInt16),
                NumericType.UInt16 => checked((sbyte)number._asUInt16),
                NumericType.Int32 => checked((sbyte)number._asInt32),
                NumericType.UInt32 => checked((sbyte)number._asUInt32),
                NumericType.Int64 => checked((sbyte)number._asInt64),
                NumericType.UInt64 => checked((sbyte)number._asUInt64),
                NumericType.Single => checked((sbyte)number._asSingle),
                NumericType.Double => checked((sbyte)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator byte(Union number)
        {
            return number._currentType switch
            {
                NumericType.Byte => number._asByte,
                NumericType.SByte => checked((byte)number._asSByte),
                NumericType.Int16 => checked((byte)number._asInt16),
                NumericType.UInt16 => checked((byte)number._asUInt16),
                NumericType.Int32 => checked((byte)number._asInt32),
                NumericType.UInt32 => checked((byte)number._asUInt32),
                NumericType.Int64 => checked((byte)number._asInt64),
                NumericType.UInt64 => checked((byte)number._asUInt64),
                NumericType.Single => checked((byte)number._asSingle),
                NumericType.Double => checked((byte)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator short(Union number)
        {
            return number._currentType switch
            {
                NumericType.Int16 => number._asInt16,
                NumericType.SByte => number._asSByte,
                NumericType.Byte => number._asByte,
                NumericType.UInt16 => checked((short)number._asUInt16),
                NumericType.Int32 => checked((short)number._asInt32),
                NumericType.UInt32 => checked((short)number._asUInt32),
                NumericType.Int64 => checked((short)number._asInt64),
                NumericType.UInt64 => checked((short)number._asUInt64),
                NumericType.Single => checked((short)number._asSingle),
                NumericType.Double => checked((short)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator ushort(Union number)
        {
            return number._currentType switch
            {
                NumericType.UInt16 => number._asUInt16,
                NumericType.Byte => number._asByte,
                NumericType.SByte => checked((ushort)number._asSByte),
                NumericType.Int16 => checked((ushort)number._asInt16),
                NumericType.Int32 => checked((ushort)number._asInt32),
                NumericType.UInt32 => checked((ushort)number._asUInt32),
                NumericType.Int64 => checked((ushort)number._asInt64),
                NumericType.UInt64 => checked((ushort)number._asUInt64),
                NumericType.Single => checked((ushort)number._asSingle),
                NumericType.Double => checked((ushort)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator int(Union number)
        {
            return number._currentType switch
            {
                NumericType.Int32 => number._asInt32,
                NumericType.SByte => number._asSByte,
                NumericType.Byte => number._asByte,
                NumericType.Int16 => number._asInt16,
                NumericType.UInt16 => number._asUInt16,
                NumericType.UInt32 => checked((int)number._asUInt32),
                NumericType.Int64 => checked((int)number._asInt64),
                NumericType.UInt64 => checked((int)number._asUInt64),
                NumericType.Single => checked((int)number._asSingle),
                NumericType.Double => checked((int)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator uint(Union number)
        {
            return number._currentType switch
            {
                NumericType.UInt32 => number._asUInt32,
                NumericType.Byte => number._asByte,
                NumericType.UInt16 => number._asUInt16,
                NumericType.SByte => checked((uint)number._asSByte),
                NumericType.Int16 => checked((uint)number._asInt16),
                NumericType.Int32 => checked((uint)number._asInt32),
                NumericType.Int64 => checked((uint)number._asInt64),
                NumericType.UInt64 => checked((uint)number._asUInt64),
                NumericType.Single => checked((uint)number._asSingle),
                NumericType.Double => checked((uint)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator long(Union number)
        {
            return number._currentType switch
            {
                NumericType.Int64 => number._asInt64,
                NumericType.SByte => number._asSByte,
                NumericType.Byte => number._asByte,
                NumericType.Int16 => number._asInt16,
                NumericType.UInt16 => number._asUInt16,
                NumericType.Int32 => number._asInt32,
                NumericType.UInt32 => number._asUInt32,
                NumericType.UInt64 => checked((long)number._asUInt64),
                NumericType.Single => checked((long)number._asSingle),
                NumericType.Double => checked((long)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator ulong(Union number)
        {
            return number._currentType switch
            {
                NumericType.UInt64 => number._asUInt64,
                NumericType.Byte => number._asByte,
                NumericType.UInt16 => number._asUInt16,
                NumericType.UInt32 => number._asUInt32,
                NumericType.SByte => checked((ulong)number._asSByte),
                NumericType.Int16 => checked((ulong)number._asInt16),
                NumericType.Int32 => checked((ulong)number._asInt32),
                NumericType.Int64 => checked((ulong)number._asInt64),
                NumericType.Single => checked((ulong)number._asSingle),
                NumericType.Double => checked((ulong)number._asDouble),
                _ => 0
            };
        }

        public static explicit operator float(Union number)
        {
            return number._currentType switch
            {
                NumericType.Single => number._asSingle,
                NumericType.SByte => number._asSByte,
                NumericType.Byte => number._asByte,
                NumericType.Int16 => number._asInt16,
                NumericType.UInt16 => number._asUInt16,
                NumericType.Int32 => number._asInt32,
                NumericType.UInt32 => number._asUInt32,
                NumericType.Int64 => number._asInt64,
                NumericType.UInt64 => number._asUInt64,
                NumericType.Double => (float)number._asDouble,
                _ => 0f
            };
        }

        public static explicit operator double(Union number) => number.AsDouble();

        public static bool operator ==(Union left, Union right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(Union left, Union right)
        {
            return left.CompareTo(right) != 0;
        }

        public static bool operator <(Union left, Union right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Union left, Union right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Union left, Union right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Union left, Union right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Union operator +(Union left, Union right)
        {
            bool leftIsInt = IsIntegralType(left._currentType);
            bool rightIsInt = IsIntegralType(right._currentType);
            if (leftIsInt && rightIsInt)
            {
                bool leftSigned = IsSigned(left._currentType);
                bool rightSigned = IsSigned(right._currentType);
                if (!leftSigned && !rightSigned)
                {
                    return new Union(left.AsUInt64() + right.AsUInt64());
                }
                if (leftSigned && rightSigned)
                {
                    return new Union(left.AsInt64() + right.AsInt64());
                }
                long l = left.AsInt64();
                long r = right.AsInt64();
                return new Union(l + r);
            }
            if (left._currentType == NumericType.Double || right._currentType == NumericType.Double)
            {
                return new Union(left.AsDouble() + right.AsDouble());
            }
            return new Union((float)left.AsDouble() + (float)right.AsDouble());
        }

        public static Union operator -(Union left, Union right)
        {
            bool leftIsInt = IsIntegralType(left._currentType);
            bool rightIsInt = IsIntegralType(right._currentType);
            if (leftIsInt && rightIsInt)
            {
                bool leftSigned = IsSigned(left._currentType);
                bool rightSigned = IsSigned(right._currentType);

                if (!leftSigned && !rightSigned)
                {
                    ulong l = left.AsUInt64();
                    ulong r = right.AsUInt64();
                    if (l >= r) return new Union(l - r);
                    return new Union((long)l - (long)r); // Результат стал отрицательным
                }
                return new Union(left.AsInt64() - right.AsInt64());
            }
            if (left._currentType == NumericType.Double || right._currentType == NumericType.Double)
            {
                return new Union(left.AsDouble() - right.AsDouble());
            }
            return new Union((float)left.AsDouble() - (float)right.AsDouble());
        }

        public static Union operator *(Union left, Union right)
        {
            bool leftIsInt = IsIntegralType(left._currentType);
            bool rightIsInt = IsIntegralType(right._currentType);
            if (leftIsInt && rightIsInt)
            {
                bool leftSigned = IsSigned(left._currentType);
                bool rightSigned = IsSigned(right._currentType);
                if (!leftSigned && !rightSigned)
                {
                    return new Union(left.AsUInt64() * right.AsUInt64());
                }
                return new Union(left.AsInt64() * right.AsInt64());
            }
            if (left._currentType == NumericType.Double || right._currentType == NumericType.Double)
            {
                return new Union(left.AsDouble() * right.AsDouble());
            }
            return new Union((float)left.AsDouble() * (float)right.AsDouble());
        }

        public static Union operator /(Union left, Union right)
        {
            bool leftIsInt = IsIntegralType(left._currentType);
            bool rightIsInt = IsIntegralType(right._currentType);
            if (leftIsInt && rightIsInt)
            {
                bool leftSigned = IsSigned(left._currentType);
                bool rightSigned = IsSigned(right._currentType);
                if (!leftSigned && !rightSigned)
                {
                    ulong r = right.AsUInt64();
                    if (r == 0) throw new DivideByZeroException();
                    return new Union(left.AsUInt64() / r);
                }
                long rs = right.AsInt64();
                if (rs == 0) throw new DivideByZeroException();
                return new Union(left.AsInt64() / rs);
            }
            if (left._currentType == NumericType.Double || right._currentType == NumericType.Double)
            {
                return new Union(left.AsDouble() / right.AsDouble());
            }
            return new Union((float)left.AsDouble() / (float)right.AsDouble());
        }

        #region IConvertible Implementation

        #region IConvertible Explicit Implementation

        TypeCode IConvertible.GetTypeCode()
        {
            return _currentType switch
            {
                NumericType.Byte => TypeCode.Byte,
                NumericType.SByte => TypeCode.SByte,
                NumericType.Int16 => TypeCode.Int16,
                NumericType.UInt16 => TypeCode.UInt16,
                NumericType.Int32 => TypeCode.Int32,
                NumericType.UInt32 => TypeCode.UInt32,
                NumericType.Int64 => TypeCode.Int64,
                NumericType.UInt64 => TypeCode.UInt64,
                NumericType.Single => TypeCode.Single,
                NumericType.Double => TypeCode.Double,
                _ => TypeCode.Object
            };
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return _currentType switch
            {
                NumericType.Byte => _asByte != 0,
                NumericType.SByte => _asSByte != 0,
                NumericType.Int16 => _asInt16 != 0,
                NumericType.UInt16 => _asUInt16 != 0,
                NumericType.Int32 => _asInt32 != 0,
                NumericType.UInt32 => _asUInt32 != 0,
                NumericType.Int64 => _asInt64 != 0,
                NumericType.UInt64 => _asUInt64 != 0,
                NumericType.Single => _asSingle != 0f,
                NumericType.Double => _asDouble != 0.0,
                _ => false
            };
        }

        byte IConvertible.ToByte(IFormatProvider provider) => (byte)this;

        sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte)this;

        short IConvertible.ToInt16(IFormatProvider provider) => (short)this;

        ushort IConvertible.ToUInt16(IFormatProvider provider) => (ushort)this;

        int IConvertible.ToInt32(IFormatProvider provider) => (int)this;

        uint IConvertible.ToUInt32(IFormatProvider provider) => (uint)this;

        long IConvertible.ToInt64(IFormatProvider provider) => (long)this;

        ulong IConvertible.ToUInt64(IFormatProvider provider) => (ulong)this;

        float IConvertible.ToSingle(IFormatProvider provider) => (float)this;

        double IConvertible.ToDouble(IFormatProvider provider) => (double)this;

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return _currentType switch
            {
                NumericType.Byte => _asByte,
                NumericType.SByte => _asSByte,
                NumericType.Int16 => _asInt16,
                NumericType.UInt16 => _asUInt16,
                NumericType.Int32 => _asInt32,
                NumericType.UInt32 => _asUInt32,
                NumericType.Int64 => _asInt64,
                NumericType.UInt64 => _asUInt64,
                NumericType.Single => (decimal)_asSingle,
                NumericType.Double => (decimal)_asDouble,
                _ => 0m
            };
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return checked((char)((IConvertible)this).ToInt32(provider));
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("[AnyNumber] Invalid cast from AnyNumber to DateTime");
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return _currentType switch
            {
                NumericType.Byte => _asByte.ToString(provider),
                NumericType.SByte => _asSByte.ToString(provider),
                NumericType.Int16 => _asInt16.ToString(provider),
                NumericType.UInt16 => _asUInt16.ToString(provider),
                NumericType.Int32 => _asInt32.ToString(provider),
                NumericType.UInt32 => _asUInt32.ToString(provider),
                NumericType.Int64 => _asInt64.ToString(provider),
                NumericType.UInt64 => _asUInt64.ToString(provider),
                NumericType.Single => _asSingle.ToString(provider),
                NumericType.Double => _asDouble.ToString(provider),
                _ => string.Empty
            };
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(Union)) return this;
            return Convert.ChangeType(Value, conversionType, provider);
        }

        #endregion


        #endregion

        private static bool IsIntegralType(NumericType type)
        {
            return type != NumericType.Single && type != NumericType.Double;
        }

        private double AsDouble()
        {
            return _currentType switch
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
                _ => 0.0
            };
        }

        private static int CompareIntegrals(Union left, Union right)
        {
            bool leftSigned = IsSigned(left._currentType);
            bool rightSigned = IsSigned(right._currentType);
            if (leftSigned && rightSigned)
            {
                long l = left.AsInt64();
                long r = right.AsInt64();
                return l.CompareTo(r);
            }
            if (!leftSigned && !rightSigned)
            {
                ulong l = left.AsUInt64();
                ulong r = right.AsUInt64();
                return l.CompareTo(r);
            }
            if (leftSigned)
            {
                long l = left.AsInt64();
                if (l < 0) return -1;
                return ((ulong)l).CompareTo(right.AsUInt64());
            }
            else
            {
                long r = right.AsInt64();
                if (r < 0) return 1;
                return left.AsUInt64().CompareTo((ulong)r);
            }
        }

        private static bool IsSigned(NumericType type)
        {
            return type == NumericType.SByte || type == NumericType.Int16 ||
                   type == NumericType.Int32 || type == NumericType.Int64;
        }

        private long AsInt64()
        {
            return _currentType switch
            {
                NumericType.SByte => _asSByte,
                NumericType.Int16 => _asInt16,
                NumericType.Int32 => _asInt32,
                NumericType.Int64 => _asInt64,
                NumericType.Byte => _asByte,
                NumericType.UInt16 => _asUInt16,
                NumericType.UInt32 => _asUInt32,
                _ => 0
            };
        }

        private ulong AsUInt64()
        {
            return _currentType switch
            {
                NumericType.Byte => _asByte,
                NumericType.UInt16 => _asUInt16,
                NumericType.UInt32 => _asUInt32,
                NumericType.UInt64 => _asUInt64,
                _ => 0
            };
        }
    }
}