using System;
using UnityEditor;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public static class SelectorCopyPaste
    {
        private const string CopyBufferMarker = "RefSerializer_JSON:";

        public static void Copy(object target)
        {
            if (target == null) return;
            try
            {
                string typeName = target.GetType().AssemblyQualifiedName;
                string json = EditorJsonUtility.ToJson(target);
                EditorGUIUtility.systemCopyBuffer = $"{CopyBufferMarker}{typeName}|{json}";
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CopyPaste] Не удалось скопировать объект. Ошибка: {exception.Message}");
            }
        }

        public static object Paste()
        {
            string buffer = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(buffer) || !buffer.StartsWith(CopyBufferMarker))
            {
                return null;
            }
            try
            {
                string cleanBuffer = buffer.Substring(CopyBufferMarker.Length);
                string[] parts = cleanBuffer.Split('|', 2);
                if (parts.Length < 2) return null;
                string typeName = parts[0];
                string json = parts[1];
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    Debug.LogWarning($"[CopyPaste] Не удалось распознать тип {typeName} при вставке из буфера обмена.");
                    return null;
                }
                object newInstance = ReferenceFactory.CreateInstance(type);
                if (newInstance == null) return null;
                EditorJsonUtility.FromJsonOverwrite(json, newInstance);
                return newInstance;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CopyPaste] Не удалось вставить данные из буфера обмена. Ошибка: {exception.Message}");
                return null;
            }
        }

        public static bool CanPaste()
        {
            string buffer = EditorGUIUtility.systemCopyBuffer;
            return !string.IsNullOrEmpty(buffer) && buffer.StartsWith(CopyBufferMarker);
        }
    }
}
