using System;
using UnityEngine;

namespace CrystalEngine.DI
{
    /// <summary>
    /// Представляет внутренний контейнер данных (дескриптор) для зарегистрированной зависимости.
    /// Хранит информацию о контрактах, реализации, жизненном цикле и условиях внедрения.
    /// <br/><br/>
    /// Represents an internal data container (descriptor) for a registered dependency.
    /// Stores information about contracts, implementation, lifecycle, and injection conditions.
    /// </summary>
    internal sealed class Binding
    {
        private volatile object _instance;

        /// <summary>
        /// Возвращает массивов типов-интерфейсов (контрактов), к которым привязана данная зависимость.
        /// <br/><br/>
        /// Returns an array of contract/interface types to which this dependency is bound.
        /// </summary>
        internal Type[] ContractTypes { get; }

        /// <summary>
        /// Возвращает конкретный тип реализации зависимости.
        /// <br/><br/>
        /// Returns the concrete implementation type of the dependency.
        /// </summary>
        internal Type ConcreteType { get; }

        /// <summary>
        /// Возвращает текущий жизненный цикл зависимости.
        /// <br/><br/>
        /// Returns the current lifecycle of the dependency.
        /// </summary>
        internal Lifecycle Lifecycle { get; private set; }

        /// <summary>
        /// Возвращает уже созданный или закешированный экземпляр объекта (для Singleton).
        /// <br/><br/>
        /// Returns an already created or cached object instance (for Singleton).
        /// </summary>
        internal object Instance => _instance;

        /// <summary>
        /// Возвращает зарегистрированный пользовательский фабричный метод для создания зависимости.
        /// <br/><br/>
        /// Returns the registered custom factory method for dependency instantiation.
        /// </summary>
        internal Func<DIContainer, object> FactoryMethod { get; private set; }

        /// <summary>
        /// Возвращает Unity-префаб, связанный с текущей зависимостью.
        /// <br/><br/>
        /// Returns the Unity prefab associated with the current dependency.
        /// </summary>
        internal GameObject Prefab { get; private set; }

        /// <summary>
        /// Возвращает флаг, указывающий, создается ли данная зависимость через фабричный метод.
        /// <br/><br/>
        /// Returns a flag indicating whether this dependency is created via a factory method.
        /// </summary>
        internal bool IsFactoryMethod => FactoryMethod != null;

        /// <summary>
        /// Возвращает флаг, указывающий, привязана ли данная зависимость к Unity-префабу.
        /// <br/><br/>
        /// Returns a flag indicating whether this dependency is bound to a Unity prefab.
        /// </summary>
        internal bool IsPrefabBinding => Prefab != null;

        /// <summary>
        /// Возвращает флаг, указывающий, был ли экземпляр предоставлен извне при регистрации.
        /// <br/><br/>
        /// Returns a flag indicating whether the instance was pre-created and provided during registration.
        /// </summary>
        internal bool IsPreCreated { get; private set; }

        /// <summary>
        /// Возвращает предикат условного внедрения зависимости (условие контекста).
        /// <br/><br/>
        /// Returns the conditional dependency injection predicate (context condition).
        /// </summary>
        internal Func<Type, bool> Condition { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Binding"/> с указанными типами контрактов и типом реализации.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="Binding"/> class with the specified contract types and concrete implementation type.
        /// </summary>
        /// <param name="contractTypes">Массив интерфейсов или базовых классов, представляющих контракты зависимости.<br/><br/>An array of interfaces or base classes representing dependency contracts.</param>
        /// <param name="concreteType">Конкретный тип класса реализации зависимости.<br/><br/>The concrete class type of the dependency implementation.</param>
        internal Binding(Type[] contractTypes, Type concreteType)
        {
            ContractTypes = contractTypes;
            ConcreteType = concreteType;
            Lifecycle = Lifecycle.Transient;
            Condition = null;
        }

        /// <summary>
        /// Изменяет жизненный цикл текущей связи на указанный.
        /// <br/><br/>
        /// Changes the lifecycle of the current binding to the specified one.
        /// </summary>
        /// <param name="lifecycle">Целевой жизненный цикл для применения к связи.<br/><br/>The target lifecycle to apply to the binding.</param>
        internal void SetLifecycle(Lifecycle lifecycle)
        {
            Lifecycle = lifecycle;
        }

        /// <summary>
        /// Принудительно устанавливает созданный в процессе работы экземпляр объекта (кеш для Singleton).
        /// <br/><br/>
        /// Forcibly sets the object instance created during runtime (cache for Singleton).
        /// </summary>
        /// <param name="instance">Созданный экземпляр объекта зависимости.<br/><br/>The created dependency object instance.</param>
        internal void SetInstance(object instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Устанавливает готовый внешний экземпляр объекта и переводит связь в статус предопределенной.
        /// <br/><br/>
        /// Sets a pre-created external object instance and marks the binding as pre-defined.
        /// </summary>
        /// <param name="instance">Готовый внешний экземпляр объекта.<br/><br/>The pre-created external object instance.</param>
        internal void SetPreCreatedInstance(object instance)
        {
            _instance = instance;
            IsPreCreated = true;
        }

        /// <summary>
        /// Устанавливает предикат условия для контекстного (условного) внедрения зависимости.
        /// <br/><br/>
        /// Sets the condition predicate for contextual (conditional) dependency injection.
        /// </summary>
        /// <param name="condition">Функция-предикат, принимающая тип цели и возвращающая истину, если внедрение разрешено.<br/><br/>
        /// A predicate function that takes the target type and returns true if injection is allowed.</param>
        internal void SetCondition(Func<Type, bool> condition)
        {
            Condition = condition;
        }

        /// <summary>
        /// Устанавливает пользовательский фабричный метод для кастомного создания объекта.
        /// <br/><br/>
        /// Sets a custom factory method for bespoke object instantiation.
        /// </summary>
        /// <param name="factoryMethod">Функция-делегат, принимающая контейнер и возвращающая экземпляр объекта.<br/><br/>
        /// A delegate function that takes the container and returns the object instance.</param>
        internal void SetFactoryMethod(Func<DIContainer, object> factoryMethod)
        {
            FactoryMethod = factoryMethod;
        }

        /// <summary>
        /// Устанавливает Unity-префаб в качестве источника зависимости и переводит её в жизненный цикл Singleton.
        /// <br/><br/>
        /// Sets a Unity prefab as the dependency source and switches its lifecycle to Singleton.
        /// </summary>
        /// <param name="prefab">Игровой объект-префаб, содержащий целевой компонент.<br/><br/>The prefab GameObject containing the target component.</param>
        internal void SetPrefab(UnityEngine.GameObject prefab)
        {
            Prefab = prefab;
            Lifecycle = Lifecycle.Singleton;
        }
    }
}