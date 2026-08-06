using UnityEngine;

namespace CrystalEngine.DI
{
    public interface IBindingConfigurator
    {
        IBindingConfigurator AsSingle();
        IBindingConfigurator AsTransient();
    }
}