namespace CrystalEngine.DI
{
    /// <summary>
    /// Предоставляет контракт для настройки параметров связывания зависимости в DI-контейнере через Fluent API.
    /// <br/><br/>
    /// Provides a contract for configuring dependency binding parameters within the DI container using a Fluent API.
    /// </summary>
    public interface IBindingConfigurator
    {
        /// <summary>
        /// Устанавливает жизненный цикл зависимости как Singleton.
        /// <br/><br/>
        /// Sets the dependency lifecycle to Singleton.
        /// </summary>
        IBindingConfigurator AsSingle();

        /// <summary>
        /// Устанавливает жизненный цикл зависимости как Transient.
        /// <br/><br/>
        /// Sets the dependency lifecycle to Transient.
        /// </summary>
        IBindingConfigurator AsTransient();

        /// <summary>
        /// Привязывает зависимость к конкретному уже существующему экземпляру объекта.
        /// <br/><br/>
        /// Binds the dependency to a specific, already existing object instance.
        /// </summary>
        IBindingConfigurator FromInstance(object instance);

        /// <summary>
        /// Определяет условие, при котором зависимость внедряется только в указанный целевой тип <typeparamref name="TTarget"/>.
        /// <br/><br/>
        /// Defines a condition where the dependency is injected only into the specified target type <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TTarget">Тип компонента, в который внедряется зависимость.<br/><br/>The type of the component into which the dependency is injected.</typeparam>
        IBindingConfigurator WhenInjectedInto<TTarget>() where TTarget : class;
    }
}
