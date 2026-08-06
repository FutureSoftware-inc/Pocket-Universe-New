using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.DI
{
    public sealed class DIContainer
    {
        private readonly Instantiator _instantiator;
        private readonly Dictionary<Type, Binding> _bindings = new();

        public DIContainer()
        {
            _instantiator = new Instantiator(this);
        }

        public Binder<TContract> Bind<TContract>()
        {
            return new Binder<TContract>(this);
        }

        public IBindingConfigurator BindAsSelf<TConcrete>() where TConcrete : class
        {
            return RegisterBindings(typeof(TConcrete), typeof(TConcrete));
        }

        public TContract Resolve<TContract>()
        {
            return (TContract)Resolve(typeof(TContract));
        }

        internal IBindingConfigurator RegisterBindings(Type contractType, Type concreteType)
        {
            Binding binding = new Binding(contractType, concreteType);
            _bindings[contractType] = binding;
            return new BindingConfigurator(binding);
        }

        internal object Resolve(Type contractType)
        {
            if (!_bindings.TryGetValue(contractType, out Binding binding))
            {
                throw new Exception($"[DI Error] Зависимость для типа {contractType.Name} не зарегистрирована!");
            }
            if (binding.Lifecycle == Lifecycle.Singleton && binding.Instance != null)
            {
                return binding.Instance;
            }
            object instance = _instantiator.Instantiate(binding.ConcreteType);
            if (binding.Lifecycle == Lifecycle.Singleton)
            {
                binding.SetInstance(instance);
            }
            return instance;
        }
    }
}