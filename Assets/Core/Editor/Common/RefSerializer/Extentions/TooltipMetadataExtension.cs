using System;
using System.Linq;
using CrystalEngine;

namespace CrystalEditor
{
    public sealed class TooltipMetadataExtension : TypeMetadataExtension
    {
        public string Tooltip { get; private set; } = string.Empty;

        public override void Initialize(Type type, Type baseType)
        {
            var attr = type.GetCustomAttributes(typeof(SelectorTooltipAttribute), false).FirstOrDefault() as SelectorTooltipAttribute;
            if (attr != null) Tooltip = attr.Tooltip;
        }
    }
}