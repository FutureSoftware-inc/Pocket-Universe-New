using UnityEngine;

namespace CrystalEngine.DI
{
    public abstract class MonoInstaller : MonoBehaviour, IBindingInstaller
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