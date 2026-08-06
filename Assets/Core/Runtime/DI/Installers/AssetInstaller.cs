using UnityEngine;

namespace CrystalEngine.DI
{
    public abstract class AssetInstaller : ScriptableObject, IBindingInstaller
    {
        protected DIContainer Container { get; private set; }

        public void InstallBindings(DIContainer container)
        {
            Container = container;
            InstallBindings();
        }

        protected abstract void InstallBindings();
    }
}