using System;
using System.Linq;
using CrystalEngine;

namespace CrystalEditor
{
    public sealed class PathMetadataExtension : TypeMetadataExtension
    {
        public string Path { get; private set; } = string.Empty;

        public override void Initialize(Type type, Type baseType)
        {
            var attr = type.GetCustomAttributes(typeof(SubclassPathAttribute), false).FirstOrDefault() as SubclassPathAttribute;
            if (attr != null) Path = attr.Path;
        }
    }
}