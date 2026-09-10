using System;
using UnityEngine;

namespace CrystalEngine.DI
{
    /// <summary>
    /// Предоставляет типизированный связыватель (Binder) для связывания типа контракта с его реализацией.
    /// <br/><br/>
    /// Provides a typed binder for binding a contract type to its concrete implementation.
    /// </summary>
    /// <typeparam name="TContract">Тип интерфейса или базового класса контракта.<br/><br/>The interface or base class type of the contract.</typeparam>
    public class Binder<TContract>
    {
        private readonly DIContainer _container;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Binder{TContract}"/> для указанного DI-контейнера.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="Binder{TContract}"/> class for the specified DI container.
        /// </summary>
        /// <param name="container">Экземпляр главного DI-контейнера.<br/><br/>The instance of the main DI container.</param>
        public Binder(DIContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Связывает тип контракта с конкретным типом реализации <typeparamref name="TConcrete"/>.
        /// <br/><br/>
        /// Binds the contract type to the concrete implementation type <typeparamref name="TConcrete"/>.
        /// </summary>
        /// <typeparam name="TConcrete">Конкретный тип класса реализации, наследующий или реализующий <typeparamref name="TContract"/>.<br/><br/>The concrete class type of the implementation that inherits or implements <typeparamref name="TContract"/>.</typeparam>
        /// <returns>Конфигуратор для последующей настройки жизненного цикла и условий связи.<br/><br/>A configurator for subsequent lifecycle and condition setup of the binding.</returns>
        public IBindingConfigurator To<TConcrete>() where TConcrete : TContract
        {
            return _container.RegisterBindings(typeof(TContract), typeof(TConcrete));
        }

        /// <summary>
        /// Регистрирует тип контракта на самого себя. Используется, когда контракт является конкретным классом реализации.
        /// <br/><br/>
        /// Registers the contract type onto itself. Used when the contract is a concrete implementation class.
        /// </summary>
        /// <returns>Конфигуратор для последующей настройки жизненного цикла и условий связи.<br/><br/>A configurator for subsequent lifecycle and condition setup of the binding.</returns>
        public IBindingConfigurator AsSelf()
        {
            return _container.RegisterBindings(typeof(TContract), typeof(TContract));
        }

        /// <summary>
        /// Связывает контракт с пользовательским фабричным методом (лямбдой) для кастомного создания объекта.
        /// <br/><br/>
        /// Binds the contract to a custom factory method (lambda) for custom object instantiation.
        /// </summary>
        /// <param name="factoryMethod">Делегат, принимающий контейнер и возвращающий созданный экземпляр объекта.<br/><br/>A delegate that takes the container and returns the created object instance.</param>
        /// <returns>Конфигуратор для последующей настройки жизненного цикла и условий связи.<br/><br/>A configurator for subsequent lifecycle and condition setup of the binding.</returns>
        public IBindingConfigurator ToMethod(Func<DIContainer, TContract> factoryMethod)
        {
            if (factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod), "[DI Error] Фабричный метод не может быть null!");
            }
            Binding binding = new(new[] { typeof(TContract) }, typeof(TContract));
            binding.SetFactoryMethod(ctx => factoryMethod(ctx));
            _container.RegisterBindingDirectly(typeof(TContract), binding);
            return new BindingConfigurator(binding);
        }

        /// <summary>
        /// Связывает контракт с компонентом Unity-префаба. Контейнер автоматически создаст префаб при разрешении зависимости.
        /// <br/><br/>
        /// Binds the contract to a Unity prefab component. The container will automatically instantiate the prefab upon dependency resolution.
        /// </summary>
        /// <param name="prefab">Игровой объект-префаб, содержащий целевой компонент.<br/><br/>The prefab GameObject containing the target component.</param>
        /// <returns>Конфигуратор для последующей настройки жизненного цикла и условий связи.<br/><br/>A configurator for subsequent lifecycle and condition setup of the binding.</returns>
        public IBindingConfigurator ToPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "[DI Error] Переданный префаб не может быть null!");
            }
            Binding binding = new Binding(new[] { typeof(TContract) }, typeof(TContract));
            binding.SetPrefab(prefab);
            _container.RegisterBindingDirectly(typeof(TContract), binding);
            return new BindingConfigurator(binding);
        }
    }
}