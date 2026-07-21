using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Crystal.Common
{
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

        [FieldOffset(8)][SerializeField] private NumericType _currentType;

        public AnyNumber(byte value) : this() { _asByte = value; _currentType = NumericType.Byte; }
        public AnyNumber(sbyte value) : this() { _asSByte = value; _currentType = NumericType.SByte; }
        public AnyNumber(ushort value) : this() { _asUInt16 = value; _currentType = NumericType.UInt16; }
        public AnyNumber(short value) : this() { _asInt16 = value; _currentType = NumericType.Int16; }
        public AnyNumber(uint value) : this() { _asUInt32 = value; _currentType = NumericType.UInt32; }
        public AnyNumber(int value) : this() { _asInt32 = value; _currentType = NumericType.Int32; }
        public AnyNumber(ulong value) : this() { _asUInt64 = value; _currentType = NumericType.UInt64; }
        public AnyNumber(long value) : this() { _asInt64 = value; _currentType = NumericType.Int64; }
        public AnyNumber(float value) : this() { _asSingle = value; _currentType = NumericType.Single; }
        public AnyNumber(double value) : this() { _asDouble = value; _currentType = NumericType.Double; }

        public NumericType CurrentType => _currentType;

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

        public int CompareTo(object obj)
        {
            object rawRight = obj is AnyNumber any ? any.Value : obj;
            double left = Convert.ToDouble(Value);
            double right = Convert.ToDouble(rawRight);
            return left.CompareTo(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is AnyNumber other && this == other)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Convert.ToDouble(Value).GetHashCode();
        }

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