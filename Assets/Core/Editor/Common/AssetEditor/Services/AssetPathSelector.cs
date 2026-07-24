using System;
using UnityEditor;

namespace CrystalEditor
{
    public static class AssetPathSelector
    {
        private const string DefaultPath = "Assets";

        public static string GetDefaultPathForAsset(Type assetType) =>
            assetType == null ? DefaultPath : EditorPrefs.GetString(GetPrefsKey(assetType), DefaultPath);

        public static void SetDefaultPathForAsset(Type assetType, string newPath)
        {
            if (assetType != null && !string.IsNullOrEmpty(newPath))
            {
                EditorPrefs.SetString(GetPrefsKey(assetType), newPath);
            }
        }

        private static string GetPrefsKey(Type assetType) => $"CrystalEngine_AssetPath_{assetType.Name}";
    }
}