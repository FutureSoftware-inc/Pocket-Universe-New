namespace CrystalEngine.DI
{
    /// <summary>
    /// Реализация конфигуратора связывания, управляющая параметрами регистрации зависимости.
    /// <br/><br/>
    /// Implementation of the binding configurator that manages dependency registration parameters.
    /// </summary>
    internal class BindingConfigurator : IBindingConfigurator
    {
        private readonly Binding _binding;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BindingConfigurator"/> для указанной связи.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="BindingConfigurator"/> class for the specified binding.
        /// </summary>
        internal BindingConfigurator(Binding binding)
        {
            _binding = binding;
        }

        /// <inheritdoc />
        public IBindingConfigurator AsSingle()
        {
            _binding.SetLifecycle(Lifecycle.Singleton);
            return this;
        }

        /// <inheritdoc />
        public IBindingConfigurator AsTransient()
        {
            _binding.SetLifecycle(Lifecycle.Transient);
            return this;
        }

        /// <inheritdoc />
        public IBindingConfigurator FromInstance(object instance)
        {
            _binding.SetPreCreatedInstance(instance);
            _binding.SetLifecycle(Lifecycle.Singleton);
            return this;
        }

        /// <inheritdoc />
        public IBindingConfigurator WhenInjectedInto<TTarget>() where TTarget : class
        {
            _binding.SetCondition(targetType => targetType == typeof(TTarget));
            return this;
        }
    }
}