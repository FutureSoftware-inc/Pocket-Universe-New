using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Вспомогательный класс для миграции данных между объектами разных типов через рефлексию.
    /// Переносит значения полей с совпадающими именами и совместимыми типами данных в инспекторе.
    /// <br/><br/>
    /// A helper class for migrating data between objects of different types using reflection.
    /// Transfers values of fields with matching names and compatible data types in the Inspector.
    /// </summary>
    public static class DataMigrator
    {
        /// <summary>
        /// Переносит значения сериализуемых полей из исходного объекта в целевой при совпадении их имен и совместимости типов.
        /// <br/><br/>
        /// Transfers values of serializable fields from the source object to the target object when their names match and types are compatible.
        /// </summary>
        /// <param name="source">Исходный объект, откуда копируются данные.<br/><br/>The source object from which data is copied.</param>
        /// <param name="target">Целевой объект, куда записываются данные.<br/><br/>The target object to which data is written.</param>
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

        /// <summary>
        /// Собирает словарь всех сериализуемых полей типа, включая приватные поля базовых классов и исключая автосвойства.
        /// <br/><br/>
        /// Collects a dictionary of all serializable fields of a type, including private fields of base classes and excluding auto-properties.
        /// </summary>
        /// <param name="type">Системный тип исследуемого объекта.<br/><br/>The system type of the object to examine.</param>
        /// <returns>Словарь, где ключ — имя поля, а значение — метаданные FieldInfo.<br/><br/>A dictionary where the key is the field name and the value is FieldInfo metadata.</returns>
        private static Dictionary<string, FieldInfo> GetAllFields(Type type)
        {
            Dictionary<string, FieldInfo> fieldsMap = new Dictionary<string, FieldInfo>();
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                FieldInfo[] fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    // Игнорируем бэкинг-поля автоматических свойств компилятора C#
                    if (field.Name.StartsWith("<")) continue;

                    // Учитываем только публичные поля или приватные поля с атрибутом [SerializeField]
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