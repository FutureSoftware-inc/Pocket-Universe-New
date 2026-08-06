using System;
using System.Collections.Generic;
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
            object instance = null;
            if (constructorParameters.Length == 0)
            {
                instance = Activator.CreateInstance(concreteType);
            }
            else
            {
                object[] arguments = new object[constructorParameters.Length];
                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    Type parameterType = constructorParameters[i].ParameterType;
                    arguments[i] = _container.Resolve(parameterType);
                }
                instance = constructor.Invoke(arguments);
            }
            InjectObject(instance);
            return instance;
        }

        internal void InjectObject(object target)
        {
            if (target == null)
            {
                return;
            }
            InjectFields(target);
            InjectMethods(target);
        }

        private void InjectFields(object target)
        {
            Type type = target.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.IsDefined(typeof(InjectAttribute), true))
                {
                    object dependency = _container.Resolve(field.FieldType);
                    field.SetValue(target, dependency);
                }
            }
        }

        private void InjectMethods(object target)
        {
            Type type = target.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MethodInfo method in methods)
            {
                InjectAttribute attribute = method.GetCustomAttribute<InjectAttribute>(true);
                if (attribute != null)
                {
                    ParameterInfo[] methodParameters = method.GetParameters();
                    object[] arguments = new object[methodParameters.Length];
                    for (int i = 0; i < methodParameters.Length; i++)
                    {
                        arguments[i] = _container.Resolve(methodParameters[i].ParameterType);
                    }
                    method.Invoke(target, arguments);
                }
            }
        }
    }
}