using System;

namespace Crystal.Common.Editor
{
    public abstract class TypeMetadataExtension
    {
        public abstract void Initialize(Type type, Type baseType);
    }
}