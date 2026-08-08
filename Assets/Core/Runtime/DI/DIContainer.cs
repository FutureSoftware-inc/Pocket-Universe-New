using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Build.Content;
using UnityEngine;

namespace CrystalEngine.DI
{
    public sealed class DIContainer
    {
        private readonly Instantiator _instantiator;
        private readonly Dictionary<Type, List<Binding>> _bindings = new();
        private readonly DIContainer _parentContainer;
        private readonly Stack<Type> _resolutionStack = new();

        public DIContainer(DIContainer parentContainer = null)
        {
            _parentContainer = parentContainer;
            _instantiator = new Instantiator(this);
        }

        public void Inject(object target)
        {
            _instantiator.InjectObject(target);
        }

        public Binder<TContract> Bind<TContract>()
        {
            return new Binder<TContract>(this);
        }

        public IBindingConfigurator BindInterfaces<TConcrete>() where TConcrete : class
        {
            Type concreteType = typeof(TConcrete);
            Type[] interfaces = concreteType.GetInterfaces();
            if (interfaces.Length == 0)
            {
                throw new Exception($"[DI Error] У типа {concreteType.Name} нет реализуемых接口ов! Используйте BindAsSelf вместо BindInterfaces.");
            }
            Binding sharedBinding = new Binding(interfaces[0], concreteType);
            BindingConfigurator configurator = new BindingConfigurator(sharedBinding);
            foreach (Type @interface in interfaces)
            {
                AppendBinding(@interface, sharedBinding);
            }
            AppendBinding(concreteType, sharedBinding);
            return configurator;
        }

        public IBindingConfigurator BindAsSelf<TConcrete>() where TConcrete : class
        {
            return RegisterBindings(typeof(TConcrete), typeof(TConcrete));
        }

        public TContract Resolve<TContract>()
        {
            return (TContract)Resolve(typeof(TContract), null);
        }

        public IReadOnlyList<TContract> ResolveAll<TContract>()
        {
            Type contractType = typeof(TContract);
            List<TContract> result = new List<TContract>();
            if (_parentContainer != null)
            {
                result.AddRange(_parentContainer.ResolveAll<TContract>());
            }
            if (_bindings.TryGetValue(contractType, out List<Binding> bindingList))
            {
                foreach (Binding binding in bindingList)
                {
                    result.Add((TContract)ResolveBinding(binding));
                }
            }
            return result;
        }

        public TConcrete Instantiate<TConcrete>() where TConcrete : class
        {
            return (TConcrete)_instantiator.Instantiate(typeof(TConcrete));
        }

        public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefab, position, rotation, parent);
        }

        public GameObject InstantiatePrefab(GameObject prefab, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefab, parent);
        }

        public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefab, position, parent);
        }

        public TContract InstantiatePrefabForComponent<TContract>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where TContract : MonoBehaviour
        {
            GameObject spawnedObject = InstantiatePrefab(prefab, position, rotation, parent);
            TContract component = spawnedObject.GetComponent<TContract>();
            if (component == null)
            {
                throw new Exception($"[DI Error] Префаб {prefab.name} успешно создан, но на нем не найден компонент {typeof(TContract).Name}!");
            }
            return component;
        }

        public TContract InstantiatePrefabForComponent<TContract>(GameObject prefab, Transform parent = null) where TContract : MonoBehaviour
        {
            return InstantiatePrefabForComponent<TContract>(prefab, prefab.transform.position, prefab.transform.rotation, parent);
        }

        public TContract InstantiatePrefabForComponent<TContract>(GameObject prefab, Vector3 position, Transform parent = null) where TContract : MonoBehaviour
        {
            return InstantiatePrefabForComponent<TContract>(prefab, position, prefab.transform.rotation, parent);
        }

        internal IBindingConfigurator RegisterBindings(Type contractType, Type concreteType)
        {
            Binding binding = new Binding(contractType, concreteType);
            AppendBinding(contractType, binding);
            return new BindingConfigurator(binding);
        }

        internal object Resolve(Type contractType, Type targetType = null)
        {
            _resolutionStack.Push(contractType);

            try
            {
                if (!_bindings.TryGetValue(contractType, out List<Binding> bindingList) || bindingList.Count == 0)
                {
                    if (_parentContainer != null)
                    {
                        object parentResult = _parentContainer.Resolve(contractType, targetType);
                        _resolutionStack.Pop();
                        return parentResult;
                    }
                    throw new Exception(BuildResolutionTraceError(contractType));
                }
                Binding matchedBinding = null;
                for (int i = bindingList.Count - 1; i >= 0; i--)
                {
                    Binding currentBinding = bindingList[i];
                    if (currentBinding.Condition == null)
                    {
                        matchedBinding = currentBinding;
                        break;
                    }
                    if (targetType != null && currentBinding.Condition(targetType))
                    {
                        matchedBinding = currentBinding;
                        break;
                    }
                }
                if (matchedBinding == null)
                {
                    throw new Exception($"[DI Error] Для типа {contractType.Name} зарегистрированы только условные биндинги, но ни один не подошел для цели {targetType?.Name ?? "Unknown"}!");
                }
                object result = ResolveBinding(matchedBinding);
                _resolutionStack.Pop();
                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("[DI Trace Error]")) throw;
                throw new Exception(BuildResolutionTraceError(contractType) + $"\nВнутреннее исключение: {ex.Message}", ex);
            }

        }

        private string BuildResolutionTraceError(Type failedType)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<color=red>[DI Trace Error] Не удалось разрешить зависимость для типа: <b>{failedType.Name}</b></color>");
            sb.AppendLine("Цепочка вызовов (Resolution Trace):");
            Type[] trace = _resolutionStack.ToArray();
            Array.Reverse(trace);

            for (int i = 0; i < trace.Length; i++)
            {
                sb.Append($"  {trace[i].Name}");
                if (i < trace.Length - 1)
                {
                    sb.Append(" <b>-></b> ");
                }
            }
            sb.AppendLine(" <b>-></b> <color=red>[ЗДЕСЬ ОШИБКА!]</color>");
            return sb.ToString();
        }

        private void AppendBinding(Type key, Binding binding)
        {
            if (!_bindings.TryGetValue(key, out List<Binding> bindingList))
            {
                bindingList = new List<Binding>();
                _bindings[key] = bindingList;
            }
            bindingList.Add(binding);
        }

        private object ResolveBinding(Binding binding)
        {
            if (binding.IsPreCreated)
            {
                return binding.Instance;
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