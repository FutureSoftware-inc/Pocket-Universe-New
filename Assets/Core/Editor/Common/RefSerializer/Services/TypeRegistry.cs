using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using CrystalEngine;

namespace CrystalEngineEditor
{
    public static class TypeRegistry
    {
        private static readonly Dictionary<Type, List<TypeMetadata>> _registryCache = new();
        private static readonly Dictionary<Type, TypeMetadata> _metadataCache = new();

        private static readonly Type[] _supportedAttributes = new[]
        {
            typeof(SelectorNameAttribute),
            typeof(SelectorIconAttribute),
            typeof(SelectorTooltipAttribute),
            typeof(SubclassPathAttribute)
        };

        [InitializeOnLoadMethod]
        public static void ClearCache()
        {
            _registryCache.Clear();
            _metadataCache.Clear();
        }

        public static IReadOnlyList<TypeMetadata> GetImplementations(Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }
            Type searchType = baseType.IsGenericType && !baseType.IsGenericTypeDefinition ? baseType.GetGenericTypeDefinition() : baseType;
            if (_registryCache.TryGetValue(searchType, out List<TypeMetadata> cachedList))
            {
                return cachedList;
            }
            List<TypeMetadata> resultList = new();
            TypeCache.TypeCollection deviredTypes = TypeCache.GetTypesDerivedFrom(searchType);
            for (int i = 0; i < deviredTypes.Count; i++)
            {
                Type type = deviredTypes[i];
                if (IsValidImplementation(type))
                {
                    TypeMetadata metadata = GetOrCreateMetadata(type);
                    resultList.Add(metadata);
                }
            }
            if (IsValidImplementation(searchType))
            {
                TypeMetadata metadata = GetOrCreateMetadata(searchType);
                if (!resultList.Contains(metadata))
                {
                    resultList.Insert(0, metadata);
                }
            }
            _registryCache[searchType] = resultList;
            return resultList;
        }

        private static bool IsValidImplementation(Type type)
        {
            if (type.IsInterface)
            {
                return false;
            }
            if (type.IsAbstract)
            {
                return false;
            }
            return true;
        }

        private static TypeMetadata GetOrCreateMetadata(Type type)
        {
            if (_metadataCache.TryGetValue(type, out TypeMetadata cachedMetadata))
            {
                return cachedMetadata;
            }
            List<Attribute> foundAttributes = new();
            for (int i = 0; i < _supportedAttributes.Length; i++)
            {
                Attribute attribute = type.GetCustomAttribute(_supportedAttributes[i]);
                if (attribute != null)
                {
                    foundAttributes.Add(attribute);
                }
            }
            string displayName = type.GetGenericName();
            SelectorNameAttribute nameAttribute = type.GetCustomAttribute<SelectorNameAttribute>();
            if (nameAttribute != null)
            {
                displayName = nameAttribute.Name;
            }
            TypeMetadata metadata = new TypeMetadata(type, displayName, foundAttributes);
            _metadataCache[type] = metadata;
            return metadata;
        }
    }
}