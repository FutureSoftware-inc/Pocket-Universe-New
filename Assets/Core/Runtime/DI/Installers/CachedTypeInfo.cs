using System;
using System.Reflection;
using System.Collections.Generic;

namespace CrystalEngine.DI
{
    internal readonly struct CachedMethodInfo
    {
        public MethodInfo Method { get; }
        public Type[] ParameterTypes { get; }

        public CachedMethodInfo(MethodInfo method, Type[] parameterTypes)
        {
            Method = method;
            ParameterTypes = parameterTypes;
        }
    }

    internal sealed class CachedTypeInfo
    {
        public ConstructorInfo Constructor { get; }
        public Type[] ConstructorParametersType { get; }
        public IReadOnlyList<FieldInfo> InjectedFields { get; }

        public IReadOnlyList<CachedMethodInfo> InjectedMethods { get; }

        public CachedTypeInfo(
            ConstructorInfo constructor,
            Type[] constructorParametersType,
            IReadOnlyList<FieldInfo> injectedFields,
            IReadOnlyList<CachedMethodInfo> injectedMethods)
        {
            Constructor = constructor;
            ConstructorParametersType = constructorParametersType;
            InjectedFields = injectedFields;
            InjectedMethods = injectedMethods;
        }
    }
}