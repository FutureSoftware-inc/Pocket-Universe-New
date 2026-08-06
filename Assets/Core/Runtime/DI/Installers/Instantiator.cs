using System;
using System.Reflection;
using UnityEngine;

namespace CrystalEngine.DI
{
    public class Instantiator
    {
        private readonly DIContainer _container;

        public Instantiator(DIContainer container)
        {
            _container = container;
        }

        public object Instantiate(Type concreteType)
        {
            ConstructorInfo[] constructors = concreteType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
            {
                throw new Exception($"[DI Error] У типа {concreteType.Name} нет публичных конструкторов!");
            }
            ConstructorInfo constructor = constructors[0];
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
            {
                return Activator.CreateInstance(concreteType);
            }
            object[] arguments = new object[constructorParameters.Length];
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                Type parameterType = constructorParameters[i].ParameterType;
                arguments[i] = _container.Resolve(parameterType);
            }
            return constructor.Invoke(arguments);
        }
    }
}