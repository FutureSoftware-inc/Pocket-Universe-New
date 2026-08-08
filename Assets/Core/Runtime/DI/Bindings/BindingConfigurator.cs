using UnityEngine;

namespace CrystalEngine.DI
{
    internal class BindingConfigurator : IBindingConfigurator
    {
        private readonly Binding _binding;

        internal BindingConfigurator(Binding binding)
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

        public IBindingConfigurator WhenInjectedInto<TTarget>() where TTarget : class
        {
            _binding.SetCondition(targetType => targetType == typeof(TTarget));
            return this;
        }
    }
}