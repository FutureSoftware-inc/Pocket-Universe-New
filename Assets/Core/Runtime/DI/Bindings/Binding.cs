using System;
using UnityEngine;

namespace CrystalEngine.DI
{
    public sealed class Binding
    {
        public Type ContractType { get; }
        public Type ConcreteType { get; }
        public Lifecycle Lifecycle { get; private set; }
        public object Instance { get; private set; }
        public bool IsPreCreated { get; private set; }

        public Binding(Type contractType, Type concreteType)
        {
            ContractType = contractType;
            ConcreteType = concreteType;
            Lifecycle = Lifecycle.Transient;
        }

        public void SetLifecycle(Lifecycle lifecycle)
        {
            Lifecycle = lifecycle;
        }

        public void SetInstance(object instance)
        {
            Instance = instance;
        }

        public void SetPreCreatedInstance(object instance)
        {
            Instance = instance;
            IsPreCreated = true;
        }
    }
}