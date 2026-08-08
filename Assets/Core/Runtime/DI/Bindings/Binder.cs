namespace CrystalEngine.DI
{
    public class Binder<TContract>
    {
        private readonly DIContainer _container;

        public Binder(DIContainer container)
        {
            _container = container;
        }

        public IBindingConfigurator To<TConcrete>() where TConcrete : TContract
        {
            return _container.RegisterBindings(typeof(TContract), typeof(TConcrete));
        }
    }
}