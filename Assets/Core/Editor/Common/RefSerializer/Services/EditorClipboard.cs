using System;
using UnityEditor;
using UnityEngine;

namespace CrystalEngineEditor
{
    public static class EditorClipboard
    {
        internal const string LOG_PREFIX = "[CrystalEngine:Clipboard] ";
        internal const string DEFAULT_ASSEMBLY_NAME = "_assemblyQualifiedName";
        internal const string DEFAULT_JSON_DATA_NAME = "_jsonData";

        public static void Copy(SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            object target = property.managedReferenceValue;
            if (target == null) return;
            try
            {
                GUIUtility.systemCopyBuffer = CreatePayload(target);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LOG_PREFIX}Failed to copy object. Exception: {exception.Message}");
            }
        }

        public static void Cut(SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (property.managedReferenceValue == null) return;
            Copy(property);
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static bool CanPaste(SerializedProperty property)
        {
            if (property == null) return false;
            string buffer = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(buffer)) return false;
            if (!buffer.Contains(DEFAULT_ASSEMBLY_NAME) || !buffer.Contains(DEFAULT_JSON_DATA_NAME)) return false;
            try
            {
                ClipboardPayload payload = JsonUtility.FromJson<ClipboardPayload>(buffer);
                if (payload == null || string.IsNullOrEmpty(payload.AssemblyQualifiedName)) return false;
                Type copiedType = Type.GetType(payload.AssemblyQualifiedName);
                if (copiedType == null) return false;
                Type fieldType = property.GetFieldType();
                return fieldType.IsAssignableFrom(copiedType);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Paste(SerializedProperty property)
        {
            if (!CanPaste(property)) return;
            try
            {
                ClipboardPayload payload = JsonUtility.FromJson<ClipboardPayload>(GUIUtility.systemCopyBuffer);
                property.managedReferenceValue = CloneFromPayload(payload);
                property.serializedObject.ApplyModifiedProperties();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX}Failed to paste object. Exception: {ex.Message}");
            }
        }

        public static void Duplicate(SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            object target = property.managedReferenceValue;
            if (target == null) return;
            try
            {
                string rawPayload = CreatePayload(target);
                ClipboardPayload payload = JsonUtility.FromJson<ClipboardPayload>(rawPayload);
                property.managedReferenceValue = CloneFromPayload(payload);
                property.serializedObject.ApplyModifiedProperties();
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LOG_PREFIX}Failed to duplicate object. Exception: {exception.Message}");
            }
        }

        private static string CreatePayload(object target)
        {
            string typeName = target.GetType().AssemblyQualifiedName;
            string json = EditorJsonUtility.ToJson(target);
            ClipboardPayload payload = new ClipboardPayload(typeName, json);
            return JsonUtility.ToJson(payload);
        }

        private static object CloneFromPayload(ClipboardPayload payload)
        {
            Type type = Type.GetType(payload.AssemblyQualifiedName);
            object instance = Activator.CreateInstance(type);
            EditorJsonUtility.FromJsonOverwrite(payload.JsonData, instance);
            return instance;
        }        
    }
}