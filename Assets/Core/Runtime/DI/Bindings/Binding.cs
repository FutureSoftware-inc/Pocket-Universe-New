using System;

namespace CrystalEngine.DI
{
    internal sealed class Binding
    {
        internal Type ContractType { get; }
        internal Type ConcreteType { get; }
        internal Lifecycle Lifecycle { get; private set; }
        internal object Instance { get; private set; }
        internal bool IsPreCreated { get; private set; }
        internal Func<Type, bool> Condition { get; private set; }

        internal Binding(Type contractType, Type concreteType)
        {
            ContractType = contractType;
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
            Instance = instance;
        }

        internal void SetPreCreatedInstance(object instance)
        {
            Instance = instance;
            IsPreCreated = true;
        }

        internal void SetCondition(Func<Type, bool> condition)
        {
            Condition = condition;
        }
    }
}