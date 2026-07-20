using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Crystal.Common.Editor
{
    public static class TypeRegistry
    {
        private static readonly Dictionary<Type, List<Type>> _typeCache = new();

        [InitializeOnLoadMethod]
        public static void ClearCache()
        {
            _typeCache.Clear();
        }

        public static IReadOnlyList<Type> GetImplementations(Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }
            if (_typeCache.TryGetValue(baseType, out List<Type> cachedTypes))
            {
                return cachedTypes;
            }
            List<Type> derivedTypes = TypeCache.GetTypesDerivedFrom(baseType) //
                .Where(IsValidImplementation)
                .ToList();

            _typeCache[baseType] = derivedTypes;
            return derivedTypes;
        }

        private static bool IsValidImplementation(Type type)
        {
            return !type.IsInterface && !type.IsAbstract && !type.ContainsGenericParameters;
        }
    }
}
