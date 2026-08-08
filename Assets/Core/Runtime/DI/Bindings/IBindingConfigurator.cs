using UnityEngine;

namespace CrystalEngine.DI
{
    public interface IBindingConfigurator
    {
        IBindingConfigurator AsSingle();
        IBindingConfigurator AsTransient();
        void FromInstance(object instance);
        IBindingConfigurator WhenInjectedInto<TTarget>() where TTarget : class;

    }
}