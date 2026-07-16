using System;

namespace Crystal.Common
{
    [Flags]
    public enum RenderingFlags : byte
    {
        None = 0,
        NotNull = 1 << 0,
        HideLabel = 1 << 1,
        GroupByType = 1 << 2
    }
}