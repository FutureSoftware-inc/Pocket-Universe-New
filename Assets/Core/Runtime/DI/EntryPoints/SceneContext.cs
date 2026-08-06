using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalEngine.DI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class SceneContext : MonoBehaviour
    {
        [SerializeField] private MonoInstaller[] _monoInstallers;
        [SerializeField] private AssetInstaller[] _assetInstallers;

        private DIContainer _container;

        private void Awake()
        {
            _container = new DIContainer();
            _container.BindAsSelf<DIContainer>().FromInstance(_container);
            if (_monoInstallers == null || _monoInstallers.Length == 0)
            {
                _monoInstallers = FindObjectsByType<MonoInstaller>(FindObjectsInactive.Include);
            }
            if (_assetInstallers == null || _assetInstallers.Length == 0)
            {
                _assetInstallers = new AssetInstaller[0];
            }
            InstallBindings();
            InjectSceneObjects();
        }

        private void InstallBindings()
        {
            foreach (MonoInstaller monoInstaller in _monoInstallers)
            {
                if (monoInstaller != null)
                {
                    monoInstaller.InstallBindings(_container);
                }
            }

            foreach (AssetInstaller assetInstaller in _assetInstallers)
            {
                if (assetInstaller != null)
                {
                    assetInstaller.InstallBindings(_container);
                }
            }
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
                    _container.Inject(monoBehaviour);
                }
            }
            Debug.Log("[SceneContext] Базовые зависимости сцены успешно зарегистрированы.");
        }
    }
}