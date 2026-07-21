using System;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SelectorTooltipAttribute : Attribute
    {
        public string Tooltip { get; }

        public SelectorTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}
