using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CrystalEditor
{
    public static class TypeRegistry
    {
        private static readonly Dictionary<Type, List<Type>> _typeCache = new();
        private static readonly Dictionary<Type, Dictionary<Type, TypeMetadataExtension>> _metadataCache = new();

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

        public static T GetExtension<T>(Type type, Type baseType) where T : TypeMetadataExtension
        {
            if (!_metadataCache.TryGetValue(type, out var extensions))
            {
                extensions = new Dictionary<Type, TypeMetadataExtension>();

                foreach (var factory in _extensionFactories)
                {
                    var extension = factory();
                    extension.Initialize(type, baseType);
                    extensions[extension.GetType()] = extension;
                }

                _metadataCache[type] = extensions;
            }

            return extensions.TryGetValue(typeof(T), out var result) ? (T)result : null;
        }

        private static readonly List<Func<TypeMetadataExtension>> _extensionFactories = new()
        {
            () => new PathMetadataExtension(),
            () => new DisplayNameMetadataExtension(),
            () => new TooltipMetadataExtension(),
            () => new IconMetadataExtension()
        };

        private static bool IsValidImplementation(Type type)
        {
            return !type.IsInterface && !type.IsAbstract;
        }
    }
}
