using System;

namespace Crystal.Common
{
    [Flags]
    public enum ComparisonType : byte
    {
        None = 0,
        Less = 1 << 0,
        Equal = 1 << 1,
        Greater = 1 << 2,

        LessOrEqual = Less | Equal,
        GreaterOrEqual = Greater | Equal,
        NotEqual = Less | Greater,

        Any = Less | Equal | Greater
    }
}