using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.DI
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-5000)]
    public class GameObjectContext : Context
    {
        private void Awake()
        {
            DIContainer parentContainer = null;
            SceneContext sceneContext = FindAnyObjectByType<SceneContext>();
            if (sceneContext != null)
            {
                parentContainer = sceneContext.Container;
            }
            else if (ProjectContext.Instance != null)
            {
                parentContainer = ProjectContext.Instance.Container;
            }
            InitializeContext(parentContainer);
            InjectGameObjectTree();
        }

        protected override void FindMonoInstallers()
        {
            if (monoInstallers == null)
            {
                monoInstallers = new List<MonoInstaller>();
            }
            if (monoInstallers.Count == 0)
            {
                MonoInstaller[] foundInstallers = GetComponentsInChildren<MonoInstaller>(true);
                if (foundInstallers != null)
                {
                    monoInstallers.AddRange(foundInstallers);
                }
            }
        }

        private void InjectGameObjectTree()
        {
            MonoBehaviour[] monoBehaviours = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour == null || monoBehaviour == this)
                {
                    continue;
                }
                Container.Inject(monoBehaviour);
            }
            Debug.Log($"[GameObjectContext] Иерархия объекта {gameObject.name} успешно насыщена локальными зависимостями.");
        }
    }

}