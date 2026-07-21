using System;
using UnityEditor;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public static class SelectorCopyPaste
    {
        internal const string COPY_BUFFER_MARKER = "RefSerializer_JSON:";

        private static Type _cachedType;
        private static string _cachedJson;
        private static string _cachedRawBuffer;

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

        private static void ClearCache()
        {
            _cachedType = null;
            _cachedJson = string.Empty;
            _cachedRawBuffer = string.Empty;
        }
    }
}