using System;
using UnityEditor;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Вспомогательный класс для копирования и вставки полиморфных объектов через системный буфер обмена Unity.
    /// Сериализует объекты в JSON формат с сохранением метаданных типов для безопасного воссоздания и валидации совместимости.
    /// <br/><br/>
    /// A helper class for copying and pasting polymorphic objects via the Unity system copy buffer.
    /// Serializes objects into JSON format while preserving type metadata for safe recreation and compatibility validation.
    /// </summary>
    public static class SelectorCopyPaste
    {
        internal const string COPY_BUFFER_MARKER = "RefSerializer_JSON:";

        private static Type _cachedType;
        private static string _cachedJson;
        private static string _cachedRawBuffer;

        /// <summary>
        /// Сериализует переданный объект в строку с префиксом-маркером и типом, после чего помещает её в системный буфер обмена.
        /// <br/><br/>
        /// Serializes the specified object into a string with a prefix marker and type, then places it into the system copy buffer.
        /// </summary>
        /// <param name="target">Объект для копирования. / The object to copy.</param>
        public static void Copy(object target)
        {
            if (target == null) return;
            try
            {
                string typeName = target.GetType().AssemblyQualifiedName;
                string json = EditorJsonUtility.ToJson(target);
                ClearCache();
                EditorGUIUtility.systemCopyBuffer = $"{COPY_BUFFER_MARKER}{typeName}|{json}";
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CopyPaste] Не удалось скопировать объект. Ошибка: {exception.Message}");
            }
        }

        /// <summary>
        /// Создает новый экземпляр кэшированного типа и восстанавливает в него данные из JSON строки буфера обмена.
        /// <br/><br/>
        /// Creates a new instance of the cached type and restores data into it from the copy buffer's JSON string.
        /// </summary>
        /// <returns>Новый воссозданный объект или null, если операция не удалась. / A new recreated object, or null if the operation fails.</returns>
        public static object Paste()
        {
            try
            {
                if (_cachedType == null || string.IsNullOrEmpty(_cachedJson))
                {
                    return null;
                }
                object newInstance = ReferenceFactory.CreateInstance(_cachedType);
                if (newInstance == null) return null;
                EditorJsonUtility.FromJsonOverwrite(_cachedJson, newInstance);
                return newInstance;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CopyPaste] Не удалось вставить данные из буфера обмена. Ошибка: {exception.Message}");
                return null;
            }
        }

        /// <summary>
        /// Проверяет системный буфер обмена на наличие валидных данных CrystalEngine и оценивает совместимость скопированного типа с базовым типом целевого поля.
        /// <br/><br/>
        /// Validates the system copy buffer for valid CrystalEngine data and evaluates the compatibility of the copied type against the target field's base type.
        /// </summary>
        /// <param name="baseType">Базовый тип или интерфейс целевого поля. / The base type or interface of the target field.</param>
        /// <returns>True, если данные в буфере валидны и типы совместимы; иначе false. / True if the buffer data is valid and types are compatible; otherwise, false.</returns>
        public static bool CanPaste(Type baseType)
        {
            string currentBuffer = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(currentBuffer) || !currentBuffer.StartsWith(COPY_BUFFER_MARKER))
            {
                ClearCache();
                return false;
            }
            if (currentBuffer == _cachedRawBuffer && _cachedType != null)
            {
                return IsTypeCompatible(baseType, _cachedType);
            }
            try
            {
                string cleanBuffer = currentBuffer.Substring(COPY_BUFFER_MARKER.Length);
                string[] parts = cleanBuffer.Split('|', 2);
                if (parts.Length < 2) return false;
                string typeName = parts[0];
                Type copiedType = Type.GetType(typeName);
                if (copiedType == null) return false;
                _cachedRawBuffer = currentBuffer;
                _cachedType = copiedType;
                _cachedJson = parts[1];
                return IsTypeCompatible(baseType, _cachedType);
            }
            catch
            {
                ClearCache();
                return false;
            }
        }

        /// <summary>
        /// Проверяет логическую совместимость типов, включая поддержку открытых и закрытых обобщенных (generic) интерфейсов и классов.
        /// <br/><br/>
        /// Evaluates logical type compatibility, including support for open and closed generic interfaces and classes.
        /// </summary>
        /// <param name="baseType">Базовый ожидаемый тип. / The expected base type.</param>
        /// <param name="copiedType">Тип копируемого объекта. / The type of the copied object.</param>
        /// <returns>True, если скопированный тип может быть приведен к базовому; иначе false. / True if the copied type can be assigned to the base type; otherwise, false.</returns>
        private static bool IsTypeCompatible(Type baseType, Type copiedType)
        {
            if (!baseType.IsGenericType && !copiedType.IsGenericType)
            {
                return baseType.IsAssignableFrom(copiedType);
            }
            Type openCopiedType = copiedType.IsGenericType ? copiedType.GetGenericTypeDefinition() : copiedType;
            Type openBaseType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
            if (openBaseType.IsAssignableFrom(openCopiedType)) return true;
            if (openBaseType.IsInterface)
            {
                foreach (Type interfaceType in openCopiedType.GetInterfaces())
                {
                    Type checkInterface = interfaceType.IsGenericType ? interfaceType.GetGenericTypeDefinition() : interfaceType;
                    if (checkInterface == openBaseType) return true;
                }
            }
            Type currentBase = openCopiedType.BaseType;
            while (currentBase != null && currentBase != typeof(object))
            {
                Type checkBase = currentBase.IsGenericType ? currentBase.GetGenericTypeDefinition() : currentBase;
                if (checkBase == openBaseType) return true;
                currentBase = currentBase.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Полностью сбрасывает и очищает внутренние кэшированные текстовые буферы и данные типов.
        /// <br/><br/>
        /// Completely resets and clears internal cached text buffers and type data.
        /// </summary>
        private static void ClearCache()
        {
            _cachedType = null;
            _cachedJson = string.Empty;
            _cachedRawBuffer = string.Empty;
        }
    }
}