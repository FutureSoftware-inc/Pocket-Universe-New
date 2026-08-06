using UnityEngine;

namespace CrystalEngine.DI
{
    public class BindingConfigurator : IBindingConfigurator
    {
        private readonly Binding _binding;

        public BindingConfigurator(Binding binding)
        {
            _binding = binding;
        }

        public IBindingConfigurator AsSingle()
        {
            _binding.SetLifecycle(Lifecycle.Singleton);
            return this;
        }

        public IBindingConfigurator AsTransient()
        {
            _binding.SetLifecycle(Lifecycle.Transient);
            return this;
        }

        public void FromInstance(object instance)
        {
            _binding.SetPreCreatedInstance(instance);
            _binding.SetLifecycle(Lifecycle.Singleton);
        }
    }
}