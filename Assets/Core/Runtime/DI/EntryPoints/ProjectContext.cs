using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.DI
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-20000)]
    public sealed class ProjectContext : Context
    {
        public static ProjectContext Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeContext();
            Debug.Log("[ProjectContext] Глобальный контейнер приложения успешно собран.");
        }

        protected override void FindMonoInstallers()
        {
            if (monoInstallers == null)
            {
                monoInstallers = new List<MonoInstaller>();
            }
            if (monoInstallers.Count == 0)
            {
                MonoInstaller[] foundInstallers = GetComponents<MonoInstaller>();
                if (foundInstallers != null)
                {
                    monoInstallers.AddRange(foundInstallers);
                }
            }
        }
    }
}