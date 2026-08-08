using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.DI
{
    public abstract class Context : MonoBehaviour
    {
        [SerializeField] protected List<MonoInstaller> monoInstallers;
        [SerializeField] protected List<AssetInstaller> assetInstallers;

        public DIContainer Container { get; private set; }
        
        protected void InitializeContext(DIContainer parentContainer = null)
        {
            Container = new DIContainer(parentContainer);
            Container.BindAsSelf<DIContainer>().FromInstance(Container);
            ValidateAndFindInstallers();
            InstallBindings();
        }

        private void ValidateAndFindInstallers()
        {
            ValidateAssetInstallers();
            FindMonoInstallers();
        }

        private void ValidateAssetInstallers()
        {
            if (assetInstallers == null)
            {
                assetInstallers = new List<AssetInstaller>();
            }
        }

        protected virtual void FindMonoInstallers()
        {
            if (monoInstallers == null || monoInstallers.Count == 0)
            {
                MonoInstaller[] foundInstallers = FindObjectsByType<MonoInstaller>(FindObjectsInactive.Include);
                if (foundInstallers != null)
                {
                    monoInstallers.AddRange(foundInstallers);
                }
            }
        }

        private void InstallBindings()
        {
            foreach (MonoInstaller monoInstaller in monoInstallers)
            {
                if (monoInstaller != null)
                {
                    monoInstaller.InstallBindings(Container);
                }
            }
            foreach (AssetInstaller assetInstaller in assetInstallers)
            {
                if (assetInstaller != null)
                {
                    assetInstaller.InstallBindings(Container);
                }
            }
        }
    }
}