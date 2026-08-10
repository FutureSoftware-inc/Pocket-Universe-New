using System;

namespace CrystalEngine.DI
{
    internal sealed class Binding
    {
        private volatile object _instance;

        internal Type[] ContractTypes { get; }
        internal Type ConcreteType { get; }
        internal Lifecycle Lifecycle { get; private set; }

        internal object Instance => _instance;
        internal bool IsPreCreated { get; private set; }
        internal Func<Type, bool> Condition { get; private set; }

        internal Binding(Type[] contractTypes, Type concreteType)
        {
            ContractTypes = contractTypes;
            ConcreteType = concreteType;
            Lifecycle = Lifecycle.Transient;
            Condition = null;
        }

        internal void SetLifecycle(Lifecycle lifecycle)
        {
            Lifecycle = lifecycle;
        }

        internal void SetInstance(object instance)
        {
            _instance = instance;
        }

        internal void SetPreCreatedInstance(object instance)
        {
            _instance = instance;
            IsPreCreated = true;
        }

        internal void SetCondition(Func<Type, bool> condition)
        {
            Condition = condition;
        }
    }
}