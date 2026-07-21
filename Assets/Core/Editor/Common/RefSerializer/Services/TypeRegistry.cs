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
            Type searchType = baseType.IsGenericType && !baseType.IsGenericTypeDefinition ? baseType.GetGenericTypeDefinition() : baseType;
            if (_typeCache.TryGetValue(searchType, out List<Type> cachedTypes))
            {
                return cachedTypes;
            }
            List<Type> derivedTypes = TypeCache.GetTypesDerivedFrom(searchType).Where(IsValidImplementation).ToList();
            if (IsValidImplementation(searchType) && !derivedTypes.Contains(searchType))
            {
                derivedTypes.Insert(0, searchType);
            }
            _typeCache[searchType] = derivedTypes;
            return derivedTypes;
        }

        private static bool IsValidImplementation(Type type)
        {
            return !type.IsInterface && !type.IsAbstract;
        }
    }
}
