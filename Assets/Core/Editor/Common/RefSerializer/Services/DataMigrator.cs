using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public static class DataMigrator
    {
        public static void MigrateData(object source, object target)
        {
            if (source == null || target == null)
            {
                return;
            }
            Dictionary<string, FieldInfo> sourceFields = GetAllFields(source.GetType());
            Dictionary<string, FieldInfo> targetFields = GetAllFields(target.GetType());
            foreach (KeyValuePair<string, FieldInfo> oldField in sourceFields)
            {
                if (!targetFields.TryGetValue(oldField.Key, out FieldInfo newField))
                {
                    continue;
                }
                if (newField.FieldType.IsAssignableFrom(oldField.Value.FieldType))
                {
                    try
                    {
                        object value = oldField.Value.GetValue(source);
                        newField.SetValue(target, value);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"[DataMigrator] Не удалось перенести поле {oldField.Key}. Ошибка: {exception.Message}");
                    }
                }
            }
        }

        private static Dictionary<string, FieldInfo> GetAllFields(Type type)
        {
            Dictionary<string, FieldInfo> fieldsMap = new Dictionary<string, FieldInfo>();
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                FieldInfo[] fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (field.Name.StartsWith("<")) continue;
                    if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                    {
                        if (!fieldsMap.ContainsKey(field.Name))
                        {
                            fieldsMap[field.Name] = field;
                        }
                    }
                }
                currentType = currentType.BaseType;
            }
            return fieldsMap;
        }
    }
}