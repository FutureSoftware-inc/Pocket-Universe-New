using System;

namespace CrystalEditor
{
    public abstract class TypeMetadataExtension
    {
        public abstract void Initialize(Type type, Type baseType);
    }
}