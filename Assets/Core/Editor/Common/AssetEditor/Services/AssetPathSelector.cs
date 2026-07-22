using System;
using UnityEditor;

namespace CrystalEditor
{
    public static class AssetPathSelector
    {
        // Публичный метод-сервис для получения пути на основе ТИПА АССЕТА
        public static string GetDefaultPathForAsset(Type assetType)
        {
            if (assetType == null) return "Assets";

            // Генерируем уникальный ключ в EditorPrefs строго на основе системного имени ScriptableObject
            string prefsKey = $"UniverseHub_AssetPath_{assetType.Name}";
            return EditorPrefs.GetString(prefsKey, "Assets");
        }

        // Публичный метод-сервис для СОХРАНЕНИЯ пути на основе ТИПА АССЕТА
        public static void SetDefaultPathForAsset(Type assetType, string newPath)
        {
            if (assetType == null || string.IsNullOrEmpty(newPath)) return;

            string prefsKey = $"UniverseHub_AssetPath_{assetType.Name}";
            EditorPrefs.SetString(prefsKey, newPath);
        }
    }
}