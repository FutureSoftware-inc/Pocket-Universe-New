using System;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TypeTooltipAttribute : Attribute
    {
        public string Tooltip { get; }

        public TypeTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip ?? string.Empty;
        }
    }
}