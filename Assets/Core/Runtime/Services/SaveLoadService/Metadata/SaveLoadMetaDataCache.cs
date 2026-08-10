using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrystalEngine.Services
{
    internal sealed class SaveLoadMetaDataCache
    {
        private readonly Dictionary<Type, IReadOnlyList<FieldInfo>> _cachedFields = new();
        private readonly Dictionary<Type, IReadOnlyList<PropertyInfo>> _cachedProperties = new();

        internal IReadOnlyList<FieldInfo> GetSerializableFields(Type type)
        {
            if (_cachedFields.TryGetValue(type, out var fields))
            {
                return fields;
            }
            FieldInfo[] allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<FieldInfo> result = FilterElementsWithAttribute(allFields, null);
            _cachedFields[type] = result;
            return result;
        }

        internal IReadOnlyList<PropertyInfo> GetSerializableProperties(Type type)
        {
            if (_cachedProperties.TryGetValue(type, out var properties))
            {
                return properties;
            }
            PropertyInfo[] allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<PropertyInfo> result = FilterElementsWithAttribute(allProperties, prop => prop.CanWrite);
            _cachedProperties[type] = result;
            return result;
        }

        private List<TInfo> FilterElementsWithAttribute<TInfo>(TInfo[] elements, Predicate<TInfo> additionalValidation)
            where TInfo : MemberInfo
        {
            List<TInfo> saveableElements = new List<TInfo>();
            for (int i = 0; i < elements.Length; i++)
            {
                TInfo element = elements[i];
                if (element.GetCustomAttribute<SaveDataAttribute>() == null)
                    continue;
                if (additionalValidation != null && !additionalValidation(element))
                    continue;
                saveableElements.Add(element);
            }
            return saveableElements;
        }
    }
}