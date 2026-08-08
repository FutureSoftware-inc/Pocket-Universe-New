using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalEngine.DI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class SceneContext : Context
    {
        private void Awake()
        {
            DIContainer parentContainer = null;
            if (ProjectContext.Instance != null)
            {
                parentContainer = ProjectContext.Instance.Container;
            }
            InitializeContext(parentContainer);
            InjectSceneObjects();
        }

        private void InjectSceneObjects()
        {
            Scene currentScene = gameObject.scene;
            GameObject[] rootGameObjects = currentScene.GetRootGameObjects();
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                MonoBehaviour[] monoBehaviours = rootGameObject.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour == null || monoBehaviour == this)
                    {
                        continue;
                    }
                    Container.Inject(monoBehaviour);
                }
            }
            Debug.Log("[SceneContext] Базовые зависимости сцены успешно зарегистрированы.");
        }
    }
}