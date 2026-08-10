using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CrystalEngine.DI
{
    internal class Instantiator
    {
        private readonly DIContainer _container;

        // Потокобезопасный кэш типов
        private readonly ConcurrentDictionary<Type, CachedTypeInfo> _typeCache = new();

        internal Instantiator(DIContainer container)
        {
            _container = container;
        }

        internal object Instantiate(Type concreteType)
        {
            CachedTypeInfo typeInfo = GetOrCreateCache(concreteType);
            object instance;

            if (typeInfo.ConstructorParametersType.Length == 0)
            {
                instance = Activator.CreateInstance(concreteType);
            }
            else
            {
                object[] arguments = ResolveDependencies(typeInfo.ConstructorParametersType, concreteType);
                instance = typeInfo.Constructor.Invoke(arguments);
            }

            InjectObject(instance);
            return instance;
        }

        // Этот метод по-прежнему должен вызываться ТОЛЬКО в главном потоке Unity,
        // так как внутри используется нативный Object.Instantiate
        internal GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "[DI Error] Попытка спавна пустого префаба!");
            }

            // Защита уровня Unity 6: спавн префабов разрешен только из Main Thread
            if (PlayerLoopHelper.IsMainThread == false)
            {
                // Если у тебя импортирован UniTask, он предоставляет PlayerLoopHelper.IsMainThread
                // Если компилятор ругается и на него, можно просто оставить проверку на совести Unity:
                // нативный Object.Instantiate сам выбросит корректное исключение при вызове из фонового потока.
            }

            GameObject spawnedObject = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            MonoBehaviour[] components = spawnedObject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour component in components)
            {
                if (component != null)
                {
                    InjectObject(component);
                }
            }
            return spawnedObject;
        }


        internal GameObject InstantiatePrefab(GameObject prefab, Transform parent = null) =>
            InstantiatePrefab(prefab, prefab.transform.position, prefab.transform.rotation, parent);

        internal GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Transform parent = null) =>
            InstantiatePrefab(prefab, position, prefab.transform.rotation, parent);

        internal void InjectObject(object target)
        {
            if (target == null) return;
            InjectFields(target);
            InjectMethods(target);
        }

        private CachedTypeInfo GetOrCreateCache(Type type)
        {
            // Метод GetOrAdd в ConcurrentDictionary атомарен и безопасен
            return _typeCache.GetOrAdd(type, t =>
            {
                ConstructorInfo bestConstructor = FindBestConstructor(t, out Type[] parameterTypes);
                List<FieldInfo> injectFields = FindInjectFields(t);
                List<CachedMethodInfo> injectMethods = FindInjectMethods(t);
                return new CachedTypeInfo(bestConstructor, parameterTypes, injectFields, injectMethods);
            });
        }

        private ConstructorInfo FindBestConstructor(Type type, out Type[] parameterTypes)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
            {
                throw new Exception($"[DI Error] У типа {type.Name} нет публичных конструкторов!");
            }
            ConstructorInfo bestConstructor = null;
            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.GetCustomAttribute<InjectAttribute>(true) != null)
                {
                    bestConstructor = constructor;
                    break;
                }
            }
            if (bestConstructor == null)
            {
                bestConstructor = constructors[0];
                int maxParameters = bestConstructor.GetParameters().Length;
                for (int i = 1; i < constructors.Length; i++)
                {
                    int parametersCount = constructors[i].GetParameters().Length;
                    if (parametersCount > maxParameters)
                    {
                        maxParameters = parametersCount;
                        bestConstructor = constructors[i];
                    }
                }
            }
            ParameterInfo[] parameters = bestConstructor.GetParameters();
            parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }
            return bestConstructor;
        }


        private List<FieldInfo> FindInjectFields(Type type)
        {
            List<FieldInfo> injectFields = new List<FieldInfo>();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<InjectAttribute>(true) != null)
                {
                    injectFields.Add(field);
                }
            }
            return injectFields;
        }

        private List<CachedMethodInfo> FindInjectMethods(Type type)
        {
            List<CachedMethodInfo> injectMethods = new List<CachedMethodInfo>();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>(true) != null)
                {
                    ParameterInfo[] methodParams = method.GetParameters();
                    Type[] methodParamTypes = new Type[methodParams.Length];
                    for (int i = 0; i < methodParams.Length; i++)
                    {
                        methodParamTypes[i] = methodParams[i].ParameterType;
                    }
                    injectMethods.Add(new CachedMethodInfo(method, methodParamTypes));
                }
            }
            return injectMethods;
        }

        private void InjectFields(object target)
        {
            Type type = target.GetType();
            CachedTypeInfo typeInfo = GetOrCreateCache(type);
            foreach (FieldInfo field in typeInfo.InjectedFields)
            {
                object dependency = _container.Resolve(field.FieldType, type);
                field.SetValue(target, dependency);
            }
        }

        private void InjectMethods(object target)
        {
            Type type = target.GetType();
            CachedTypeInfo typeInfo = GetOrCreateCache(type);
            foreach (CachedMethodInfo methodInfo in typeInfo.InjectedMethods)
            {
                object[] arguments = ResolveDependencies(methodInfo.ParameterTypes, type);
                methodInfo.Method.Invoke(target, arguments);
            }
        }

        private object[] ResolveDependencies(Type[] parameterTypes, Type targetType)
        {
            object[] arguments = new object[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                arguments[i] = _container.Resolve(parameterTypes[i], targetType);
            }
            return arguments;
        }
    }
}