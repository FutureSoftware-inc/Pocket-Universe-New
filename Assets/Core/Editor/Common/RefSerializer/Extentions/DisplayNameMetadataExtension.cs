using System;
using System.Linq;

namespace Crystal.Common.Editor
{
    public sealed class DisplayNameMetadataExtension : TypeMetadataExtension
    {
        public string DisplayName { get; private set; }

        public override void Initialize(Type type, Type baseType)
        {
            var nameAttr = type.GetCustomAttributes(typeof(SelectorNameAttribute), false).FirstOrDefault() as SelectorNameAttribute;
            if (nameAttr != null)
            {
                DisplayName = nameAttr.Name;
                return;
            }

            var pathAttr = type.GetCustomAttributes(typeof(SubclassPathAttribute), false).FirstOrDefault() as SubclassPathAttribute;
            if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Path))
            {
                DisplayName = pathAttr.Path.Split('/').Last();
                return;
            }

            string cleanName = type.Name.Split('`')[0];
            DisplayName = (baseType != null && baseType.IsGenericType)
                ? $"{cleanName}<{string.Join(", ", baseType.GetGenericArguments().Select(t => t.Name))}>"
                : type.Name;
        }
    }
}