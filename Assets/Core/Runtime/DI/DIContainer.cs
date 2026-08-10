using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CrystalEngine.DI
{
    public sealed class DIContainer
    {
        private readonly Instantiator _instantiator;
        private readonly ConcurrentDictionary<Type, List<Binding>> _bindings = new();
        private readonly DIContainer _parentContainer;
        private readonly ThreadLocal<Stack<Type>> _resolutionStack = new(() => new Stack<Type>());
        private readonly object _lockObject = new();

        public DIContainer(DIContainer parentContainer = null)
        {
            _parentContainer = parentContainer;
            _instantiator = new Instantiator(this);
        }

        public void Inject(object target) => _instantiator.InjectObject(target);
        public Binder<TContract> Bind<TContract>() => new Binder<TContract>(this);

        public IBindingConfigurator BindInterfaces<TConcrete>() where TConcrete : class
        {
            Type concreteType = typeof(TConcrete);
            Type[] interfaces = concreteType.GetInterfaces();
            if (interfaces.Length == 0)
            {
                throw new Exception($"[DI Error] У типа {concreteType.Name} нет реализуемых интерфейсов! Используйте BindAsSelf вместо BindInterfaces.");
            }
            Binding sharedBinding = new Binding(interfaces, concreteType);
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

        public TContract Resolve<TContract>() => (TContract)Resolve(typeof(TContract), null);

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
                lock (bindingList)
                {
                    foreach (Binding binding in bindingList)
                    {
                        result.Add((TContract)ResolveBinding(binding));
                    }
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

        internal IBindingConfigurator RegisterBindings(Type contractType, Type concreteType)
        {
            Binding binding = new Binding(new[] { contractType }, concreteType);
            AppendBinding(contractType, binding);
            return new BindingConfigurator(binding);
        }

        internal object Resolve(Type contractType, Type targetType = null)
        {
            Stack<Type> stack = _resolutionStack.Value;
            stack.Push(contractType);
            try
            {
                if (!_bindings.TryGetValue(contractType, out List<Binding> bindingList) || bindingList.Count == 0)
                {
                    if (_parentContainer != null)
                    {
                        object parentResult = _parentContainer.Resolve(contractType, targetType);
                        stack.Pop();
                        return parentResult;
                    }
                    throw new Exception(BuildResolutionTraceError(contractType));
                }
                Binding matchedBinding = null;
                lock (bindingList)
                {
                    for (int i = bindingList.Count - 1; i >= 0; i--)
                    {
                        Binding currentBinding = bindingList[i];
                        if (currentBinding.Condition == null || (targetType != null && currentBinding.Condition(targetType)))
                        {
                            matchedBinding = currentBinding;
                            break;
                        }
                    }
                }
                if (matchedBinding == null)
                {
                    throw new Exception($"[DI Error] Для типа {contractType.Name} зарегистрированы только условные биндинги, но ни один не подошел для цели {targetType?.Name ?? "Unknown"}!");
                }
                object result = ResolveBinding(matchedBinding);
                stack.Pop();
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
            Type[] trace = _resolutionStack.Value.ToArray();
            Array.Reverse(trace);
            for (int i = 0; i < trace.Length; i++)
            {
                sb.Append($"  {trace[i].Name}");
                if (i < trace.Length - 1) sb.Append(" <b>-></b> ");
            }
            sb.AppendLine(" <b>-></b> <color=red>[ЗДЕСЬ ОШИБКА!]</color>");
            return sb.ToString();
        }

        private void AppendBinding(Type key, Binding binding)
        {
            List<Binding> bindingList = _bindings.GetOrAdd(key, _ => new List<Binding>());
            lock (bindingList)
            {
                bindingList.Add(binding);
            }
        }

        private object ResolveBinding(Binding binding)
        {
            if (binding.IsPreCreated) return binding.Instance;
            if (binding.Lifecycle == Lifecycle.Singleton)
            {
                if (binding.Instance != null) return binding.Instance;
                lock (_lockObject)
                {
                    if (binding.Instance != null) return binding.Instance;

                    object instance = _instantiator.Instantiate(binding.ConcreteType);
                    binding.SetInstance(instance);
                    return instance;
                }
            }
            return _instantiator.Instantiate(binding.ConcreteType);
        }
    }
}