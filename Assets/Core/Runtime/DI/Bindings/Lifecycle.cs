namespace CrystalEngine.DI
{
    /// <summary>
    /// Определяет жизненный цикл зависимостей, регистрируемых в DI-контейнере.
    /// <br/><br/>
    /// Defines the lifecycle of dependencies registered within the DI container.
    /// </summary>
    public enum Lifecycle : byte
    {
        /// <summary>
        /// Объект создается один раз при первой инициализации и используется повторно при всех последующих запросах.
        /// <br/><br/>
        /// The object is created once upon initial request and reused for all subsequent requests.
        /// </summary>
        Singleton = 0,

        /// <summary>
        /// Новый экземпляр объекта создается при каждом запросе зависимости.
        /// <br/><br/>
        /// A new instance of the object is created upon every dependency request.
        /// </summary>
        Transient = 1
    }
}
