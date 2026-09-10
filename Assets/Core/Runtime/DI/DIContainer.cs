using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CrystalEngine.DI
{
    /// <summary>
    /// Главный управляющий контейнер внедрения зависимостей (DI).
    /// Отвечает за регистрацию связей, управление их жизненным циклом и разрешение графа зависимостей.
    /// <br/><br/>
    /// The main controlling Dependency Injection (DI) container.
    /// Responsible for registering bindings, managing their lifecycles, and resolving the dependency graph.
    /// </summary>
    public sealed class DIContainer
    {
        private readonly Instantiator _instantiator;
        private readonly ConcurrentDictionary<Type, List<Binding>> _bindings = new();
        private readonly DIContainer _parentContainer;
        private readonly ThreadLocal<Stack<Type>> _resolutionStack = new(() => new Stack<Type>());
        private readonly object _lockObject = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DIContainer"/> с необязательным родительским контейнером.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="DIContainer"/> class with an optional parent container.
        /// </summary>
        /// <param name="parentContainer">Родительский контейнер для поиска зависимостей уровнем выше.<br/><br/>The parent container to look up higher-level dependencies.</param>
        public DIContainer(DIContainer parentContainer = null)
        {
            _parentContainer = parentContainer;
            _instantiator = new Instantiator(this);
        }

        /// <summary>
        /// Внедряет зависимости в уже существующий экземпляр объекта (через поля, свойства или методы).
        /// <br/><br/>
        /// Injects dependencies into an already existing object instance (via fields, properties, or methods).
        /// </summary>
        /// <param name="target">Целевой объект для внедрения зависимостей.<br/><br/>The target object to inject dependencies into.</param>
        public void Inject(object target) => _instantiator.InjectObject(target);

        /// <summary>
        /// Создает новый связыватель (Binder) для регистрации зависимости по типу контракта.
        /// <br/><br/>
        /// Creates a new binder for registering a dependency by its contract type.
        /// </summary>
        /// <typeparam name="TContract">Тип интерфейса или базового класса.<br/><br/>The interface or base class type.</typeparam>
        /// <returns>Типизированный экземпляр связывателя.<br/><br/>A typed binder instance.</returns>
        public Binder<TContract> Bind<TContract>() => new(this);

        /// <summary>
        /// Автоматически регистрирует конкретный тип реализации под всеми интерфейсами, которые он реализует.
        /// <br/><br/>
        /// Automatically registers a concrete implementation type under all interfaces it implements.
        /// </summary>
        /// <typeparam name="TConcrete">Тип класса реализации.<br/><br/>The concrete implementation class type.</typeparam>
        /// <returns>Конфигуратор связывания для настройки параметров жизненного цикла.<br/><br/>A binding configurator to set up lifecycle parameters.</returns>
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

        /// <summary>
        /// Регистрирует конкретный тип класса как зависимость самого на себя.
        /// <br/><br/>
        /// Registers a concrete class type as a dependency onto itself.
        /// </summary>
        /// <typeparam name="TConcrete">Тип класса реализации.<br/><br/>The concrete implementation class type.</typeparam>
        /// <returns>Конфигуратор связывания для настройки параметров жизненного цикла.<br/><br/>A binding configurator to set up lifecycle parameters.</returns>
        public IBindingConfigurator BindAsSelf<TConcrete>() where TConcrete : class
        {
            return RegisterBindings(typeof(TConcrete), typeof(TConcrete));
        }

        /// <summary>
        /// Разрешает и возвращает экземпляр объекта по указанному типу контракта.
        /// <br/><br/>
        /// Resolves and returns an object instance by the specified contract type.
        /// </summary>
        /// <typeparam name="TContract">Тип запрашиваемого контракта.<br/><br/>The requested contract type.</typeparam>
        /// <returns>Экземпляр объекта, реализующий данный контракт.<br/><br/>An object instance implementing this contract.</returns>
        public TContract Resolve<TContract>() => (TContract)Resolve(typeof(TContract), null);

        /// <summary>
        /// Возвращает список всех зарегистрированных объектов, соответствующих указанному типу контракта.
        /// Включает зависимости из родительских контейнеров.
        /// <br/><br/>
        /// Returns a list of all registered objects matching the specified contract type.
        /// Includes dependencies from parent containers.
        /// </summary>
        /// <typeparam name="TContract">Тип запрашиваемого контракта.<br/><br/>The requested contract type.</typeparam>
        /// <returns>Только для чтения список найденных объектов.<br/><br/>A read-only list of resolved objects.</returns>
        public IReadOnlyList<TContract> ResolveAll<TContract>()
        {
            List<TContract> result = new List<TContract>();
            ResolveAllInternal(result);
            return result;
        }

        /// <summary>
        /// Создает экземпляр указанного типа, автоматически внедряя все необходимые зависимости в его конструктор.
        /// Объект не регистрируется в контейнере.
        /// <br/><br/>
        /// Instantiates the specified type, automatically injecting all required dependencies into its constructor.
        /// The object is not registered within the container.
        /// </summary>
        /// <typeparam name="TConcrete">Тип создаваемого класса.<br/><br/>The class type to instantiate.</typeparam>
        /// <returns>Новый экземпляр объекта с внедренными зависимостями.<br/><br/>A new object instance with injected dependencies.</returns>
        public TConcrete Instantiate<TConcrete>() where TConcrete : class
        {
            return (TConcrete)_instantiator.Instantiate(typeof(TConcrete));
        }

        /// <summary>
        /// Создает экземпляр Unity-префаба, автоматически внедряя зависимости во все его компоненты MonoBehavior.
        /// <br/><br/>
        /// Instantiates a Unity prefab, automatically injecting dependencies into all its MonoBehaviour components.
        /// </summary>
        /// <param name="prefab">Исходный игровой объект-префаб.<br/><br/>The source prefab GameObject.</param>
        /// <param name="position">Мировая позиция для появления префаба.<br/><br/>The world position for the prefab to spawn.</param>
        /// <param name="rotation">Мировое вращение для префаба.<br/><br/>The world rotation for the prefab.</param>
        /// <param name="parent">Необязательный родительский Трансформ.<br/><br/>An optional parent Transform.</param>
        /// <returns>Созданный экземпляр GameObject на сцене.<br/><br/>The instantiated GameObject instance on the scene.</returns>
        public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return _instantiator.InstantiatePrefab(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Регистрирует связь между типом контракта и типом реализации во внутреннем реестре контейнера.
        /// <br/><br/>
        /// Registers a binding between a contract type and an implementation type in the container's internal registry.
        /// </summary>
        /// <param name="contractType">Тип интерфейса или базового класса (контракт).<br/><br/>The interface or base class type (contract).</param>
        /// <param name="concreteType">Конкретный тип создаваемого класса реализации.<br/><br/>The concrete class type of the implementation to be created.</param>
        /// <returns>Конфигуратор для последующей настройки жизненного цикла связи.<br/><br/>A binding configurator for subsequent connection lifecycle setup.</returns>
        internal IBindingConfigurator RegisterBindings(Type contractType, Type concreteType)
        {
            Binding binding = new Binding(new[] { contractType }, concreteType);
            AppendBinding(contractType, binding);
            return new BindingConfigurator(binding);
        }

        /// <summary>
        /// Разрешает зависимость по указанному типу контракта с учетом контекста целевого типа.
        /// <br/><br/>
        /// Resolves a dependency for the specified contract type, taking into account the target type context.
        /// </summary>
        /// <param name="contractType">Тип запрашиваемого контракта.<br/><br/>The requested contract type.</param>
        /// <param name="targetType">Тип объекта, в который внедряется зависимость (используется для условного внедрения).<br/><br/>The type of the object into which the dependency is injected (used for conditional injection).</param>
        /// <returns>Экземпляр созданного или закешированного объекта зависимости.<br/><br/>The instance of the created or cached dependency object.</returns>
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
                        return _parentContainer.Resolve(contractType, targetType);
                    }
                    throw new Exception(DIErrorFormatter.BuildResolutionTraceError(contractType, stack));
                }
                Binding matchedBinding = null;
                for (int i = bindingList.Count - 1; i >= 0; i--)
                {
                    Binding currentBinding = bindingList[i];
                    if (currentBinding.Condition == null || (targetType != null && currentBinding.Condition(targetType)))
                    {
                        matchedBinding = currentBinding;
                        break;
                    }
                }
                if (matchedBinding == null)
                {
                    throw new Exception($"[DI Error] Для типа {contractType.Name} зарегистрированы только условные биндинги, но ни один не подошел для цели {targetType?.Name ?? "Unknown"}!");
                }
                return ResolveBinding(matchedBinding);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("[DI Trace Error]")) throw;
                throw new Exception(DIErrorFormatter.BuildResolutionTraceError(contractType, stack) + $"\nВнутреннее исключение: {ex.Message}", ex);
            }
            finally
            {
                if (stack.Count > 0)
                {
                    stack.Pop();
                }
            }
        }

        /// <summary>
        /// Напрямую регистрирует готовый дескриптор связи в контейнере, минуя стандартную фабрику типов.
        /// <br/><br/>
        /// Directly registers a ready binding descriptor in the container, bypassing the standard type factory.
        /// </summary>
        /// <param name="contractType">Тип интерфейса или базового класса (контракт).<br/><br/>The interface or base class type (contract).</param>
        /// <param name="binding">Уже сконфигурированный дескриптор связи.<br/><br/>The already configured binding descriptor.</param>
        internal void RegisterBindingDirectly(Type contractType, Binding binding)
        {
            AppendBinding(contractType, binding);
        }

        /// <summary>
        /// Управляет жизненным циклом конкретной связи и возвращает её готовый экземпляр.
        /// <br/><br/>
        /// Manages the lifecycle of a specific binding and returns its ready instance.
        /// </summary>
        /// <param name="binding">Дескриптор связи для разрешения.<br/><br/>The binding descriptor to resolve.</param>
        /// <returns>Готовый объект зависимости (новый или закешированный синглтон).<br/><br/>The ready dependency object (new or cached singleton).</returns>
        private void AppendBinding(Type key, Binding binding)
        {
            List<Binding> bindingList = _bindings.GetOrAdd(key, _ => new List<Binding>());
            lock (bindingList)
            {
                bindingList.Add(binding);
            }
        }

        /// <summary>
        /// Рекурсивно собирает все зарегистрированные зависимости для указанного контракта по всей иерархии контейнеров без аллокаций памяти.
        /// <br/><br/>
        /// Recursively accumulates all registered dependencies for the specified contract across the container hierarchy without memory allocations.
        /// </summary>
        /// <typeparam name="TContract">Тип запрашиваемого контракта.<br/><br/>The requested contract type.</typeparam>
        /// <param name="accumulator">Список-аккумулятор, в который записываются найденные экземпляры.<br/><br/>The accumulator list where the found instances are recorded.</param>
        private object ResolveBinding(Binding binding)
        {
            if (binding.IsPreCreated) return binding.Instance;
            if (binding.Lifecycle == Lifecycle.Singleton)
            {
                if (binding.Instance != null) return binding.Instance;
                lock (_lockObject)
                {
                    if (binding.Instance != null) return binding.Instance;
                    object instance = CreateInstanceFromBinding(binding);
                    binding.SetInstance(instance);
                    return instance;
                }
            }
            return CreateInstanceFromBinding(binding);
        }

        private void ResolveAllInternal<TContract>(List<TContract> accumulator)
        {
            _parentContainer?.ResolveAllInternal(accumulator);
            if (_bindings.TryGetValue(typeof(TContract), out List<Binding> bindingList))
            {
                lock (bindingList)
                {
                    for (int i = 0; i < bindingList.Count; i++)
                    {
                        accumulator.Add((TContract)ResolveBinding(bindingList[i]));
                    }
                }
            }
        }

        private object CreateInstanceFromBinding(Binding binding)
        {
            if (binding.IsFactoryMethod)
            {
                return binding.FactoryMethod(this);
            }
            if (binding.IsPrefabBinding)
            {
                return _instantiator.InstantiatePrefabComponent(binding.Prefab, binding.ConcreteType);
            }
            return _instantiator.Instantiate(binding.ConcreteType);
        }
    }
}