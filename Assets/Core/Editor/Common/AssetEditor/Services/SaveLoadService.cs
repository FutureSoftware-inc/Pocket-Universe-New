using System;
using UnityEditor;
using UnityEngine;

namespace CrystalEditor
{
    public static class SaveLoadService
    {
        public static void Save(ScriptableObject targetAsset, string undoMessage = "CrystalEngine: Modify Asset")
        {
            if (targetAsset == null) return;
            string assetPath = AssetDatabase.GetAssetPath(targetAsset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.MakeEditable(assetPath);
            }
            Undo.RecordObject(targetAsset, undoMessage);
            EditorUtility.SetDirty(targetAsset);
            AssetDatabase.SaveAssetIfDirty(targetAsset);
        }

        public static void ExecuteBulkOperation(Action bulkAction, string progressTitle = "Processing assets...")
        {
            if (bulkAction == null) return;
            int progressId = Progress.Start(progressTitle, null, Progress.Options.Indefinite);
            try
            {
                AssetDatabase.StartAssetEditing();
                bulkAction.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CrystalEngine] Critical error during bulk I/O operation: {ex.Message}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Progress.Remove(progressId);
            }
        }

        public static bool SavePrefab(GameObject instanceRoot, string undoMessage = "CrystalEngine: Save Prefab")
        {
            if (instanceRoot == null) return false;
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instanceRoot);
            if (prefabAsset == null)
            {
                Debug.LogError($"[CrystalEngine] GameObject '{instanceRoot.name}' is not a valid Prefab Instance! Cannot save changes.");
                return false;
            }
            string assetPath = AssetDatabase.GetAssetPath(prefabAsset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.MakeEditable(assetPath);
            }
            Undo.RegisterFullObjectHierarchyUndo(prefabAsset, undoMessage);
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath, out bool success);
            if (success)
            {
                EditorUtility.SetDirty(prefabAsset);
                AssetDatabase.SaveAssetIfDirty(prefabAsset);
            }
            return success;
        }

        public static T CreateNewAssetFile<T>(string folderPath, string defaultName) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(folderPath)) folderPath = "Assets";
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{defaultName}.asset");
            T newAsset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newAsset, fullPath);
            Undo.RegisterCreatedObjectUndo(newAsset, $"CrystalEngine: Create {typeof(T).Name}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return newAsset;
        }

        public static GameObject CreateNewPrefabAsset(string folderPath, string prefabName, GameObject sourceGameObject)
        {
            if (sourceGameObject == null) return null;
            if (string.IsNullOrEmpty(folderPath)) folderPath = "Assets";
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{prefabName}.prefab");
            GameObject newPrefabAsset = PrefabUtility.SaveAsPrefabAsset(sourceGameObject, fullPath, out bool success);
            if (success)
            {
                Undo.RegisterCreatedObjectUndo(newPrefabAsset, "CrystalEngine: Create Prefab Asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return newPrefabAsset;
            }
            Debug.LogError($"[CrystalEngine] Failed to create prefab asset at path: {fullPath}");
            return null;
        }

        public static T Load<T>(string assetPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("[CrystalEngine] Attempted to load asset with an empty or null path.");
                return null;
            }
            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(assetGuid))
            {
                Debug.LogError($"[CrystalEngine] Failed to load asset. File not found at path: '{assetPath}'");
                return null;
            }
            try
            {
                UnityEngine.Object loadedObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (loadedObject == null)
                {
                    Debug.LogError($"[CrystalEngine] Asset database returned null for file at path: '{assetPath}'");
                    return null;
                }
                if (loadedObject is T typedAsset)
                {
                    return typedAsset;
                }
                Debug.LogError($"[CrystalEngine] Type mismatch! Asset at '{assetPath}' is of type '{loadedObject.GetType().Name}', but requested type was '{typeof(T).Name}'.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CrystalEngine] Critical exception thrown while loading asset at '{assetPath}': {ex.Message}");
                return null;
            }
        }
    }
}