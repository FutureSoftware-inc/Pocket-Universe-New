using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CrystalEngine.DI.Editor
{
    internal static class CrystalDiEditorTools
    {
        [MenuItem("GameObject/CrystalEngine/DI/Create Project Context", false, 9)]
        private static void CreateProjectContextMenu()
        {
            GameObject projectContextGo = new GameObject("[ProjectContext]");
            ProjectContext projectContext = projectContextGo.AddComponent<ProjectContext>();
            Undo.RegisterCreatedObjectUndo(projectContextGo, "Create Project Context");
            GameObject globalInstallerGo = new GameObject("[GlobalInstaller]");
            globalInstallerGo.transform.SetParent(projectContextGo.transform);
            DefaultSceneInstaller defaultInstaller = globalInstallerGo.AddComponent<DefaultSceneInstaller>();
            FieldInfo monoInstallersField = typeof(Context).GetField("monoInstallers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (monoInstallersField != null)
            {
                List<MonoInstaller> installersList = new System.Collections.Generic.List<MonoInstaller> { defaultInstaller };
                monoInstallersField.SetValue(projectContext, installersList);
            }
            FieldInfo assetInstallersField = typeof(Context).GetField("assetInstallers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (assetInstallersField != null)
            {
                List<AssetInstaller> assetList = new System.Collections.Generic.List<AssetInstaller>();
                assetInstallersField.SetValue(projectContext, assetList);
            }
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            Selection.activeGameObject = projectContextGo;
            Debug.Log("<color=orange>[Crystal DI]</color> Глобальный ProjectContext успешно создан! Не забудьте сделать из него префаб или оставить на стартовой сцене.");
        }

        [MenuItem("GameObject/CrystalEngine/DI/Create Scene Context", false, 10)]
        private static void CreateSceneContextMenu()
        {
            GameObject contextGo = new GameObject("[SceneContext]");
            SceneContext sceneContext = contextGo.AddComponent<SceneContext>();
            Undo.RegisterCreatedObjectUndo(contextGo, "Create Scene Context");
            GameObject installerGo = new GameObject("[SceneInstaller]");
            installerGo.transform.SetParent(contextGo.transform);
            DefaultSceneInstaller defaultInstaller = installerGo.AddComponent<DefaultSceneInstaller>();
            FieldInfo monoInstallersField = typeof(Context).GetField("monoInstallers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (monoInstallersField != null)
            {
                List<MonoInstaller> installersList = new List<MonoInstaller> { defaultInstaller };
                monoInstallersField.SetValue(sceneContext, installersList);
            }
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            Selection.activeGameObject = contextGo;
            Debug.Log("<color=green>[Crystal DI]</color> Структура SceneContext успешно создана и связана в 1 клик!");
        }

        [MenuItem("GameObject/CrystalEngine/DI/Create GameObject Context", false, 11)]
        private static void CreateGameObjectContextMenu()
        {
            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo == null)
            {
                EditorUtility.DisplayDialog("Crystal DI Error",
                    "Пожалуйста, выделите Игровой Объект (GameObject) в иерархии, чтобы добавить GameObjectContext!", "ОК");
                return;
            }
            GameObjectContext goContext = selectedGo.AddComponent<GameObjectContext>();
            Undo.AddComponent<GameObjectContext>(selectedGo);
            GameObject localInstallerGo = new GameObject("[LocalInstaller]");
            localInstallerGo.transform.SetParent(selectedGo.transform);
            localInstallerGo.transform.localPosition = Vector3.zero;
            DefaultSceneInstaller defaultInstaller = localInstallerGo.AddComponent<DefaultSceneInstaller>();
            Undo.RegisterCreatedObjectUndo(localInstallerGo, "Create Local Installer");
            FieldInfo monoInstallersField = typeof(Context).GetField("monoInstallers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (monoInstallersField != null)
            {
                List<MonoInstaller> installersList = new List<MonoInstaller> { defaultInstaller };
                monoInstallersField.SetValue(goContext, installersList);
            }
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(selectedGo.scene);
            }
            Debug.Log($"<color=cyan>[Crystal DI]</color> GameObjectContext успешно добавлен на объект {selectedGo.name}!");
        }

        [MenuItem("GameObject/CrystalEngine/DI/Create Project Context", true)]
        private static bool ValidateCreateProjectContext()
        {
            return Object.FindAnyObjectByType<ProjectContext>() == null;
        }

        [MenuItem("GameObject/CrystalEngine/DI/Create Scene Context", true)]
        private static bool ValidateCreateSceneContext()
        {
            return Object.FindAnyObjectByType<SceneContext>() == null;
        }

    }
}
