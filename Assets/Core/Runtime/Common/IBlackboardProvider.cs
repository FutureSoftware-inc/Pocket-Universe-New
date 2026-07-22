namespace CrystalEngine
{
    /// <summary>
    /// Определяет интерфейс для объектов, предоставляющих доступ к компоненту Blackboard (класс доски данных).
    /// Используется для обмена данными и разделения контекста между различными системами.
    /// <br/><br/>
    /// Defines an interface for objects that provide access to a Blackboard component.
    /// Used for data sharing and context separation between different systems.
    /// </summary>
    public interface IBlackboardProvider
    {
        /// <summary>
        /// Возвращает связанный экземпляр доски данных Blackboard.
        /// <br/><br/>
        /// Gets the associated Blackboard instance.
        /// </summary>
        Blackboard Blackboard { get; }
    }
}