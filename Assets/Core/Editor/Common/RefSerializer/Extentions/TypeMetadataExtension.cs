using System;

namespace CrystalEditor
{
    /// <summary>
    /// Абстрактный базовый класс для расширений метаданных, используемых в селекторе типов.
    /// Позволяет извлекать, обрабатывать и кэшировать специфичную информацию о классах (пути, иконки, подсказки) при индексации.
    /// <br/><br/>
    /// Abstract base class for metadata extensions used within the type selector.
    /// Allows extracting, processing, and caching specific information about classes (paths, icons, tooltips) during indexing.
    /// </summary>
    public abstract class TypeMetadataExtension
    {
        /// <summary>
        /// Инициализирует расширение метаданных для конкретного типа реализации относительно его базового типа.
        /// <br/><br/>
        /// Initializes the metadata extension for a specific implementation type relative to its base type.
        /// </summary>
        /// <param name="type">Исследуемый тип конкретной реализации класса. / The specific class implementation type to inspect.</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста. / The base type or interface of the context field.</param>
        public abstract void Initialize(Type type, Type baseType);
    }
}
