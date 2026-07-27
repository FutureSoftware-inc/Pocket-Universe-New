using System;
using System.Linq;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Расширение метаданных, отвечающее за извлечение и хранение путей иерархии папок для классов в селекторе типов.
    /// Считывает данные из кастомного атрибута <see cref="SubclassPathAttribute"/>.
    /// <br/><br/>
    /// A metadata extension responsible for extracting and storing class folder hierarchy paths inside the type selector.
    /// Reads data from the custom <see cref="SubclassPathAttribute"/> attribute.
    /// </summary>
    public sealed class PathMetadataExtension : TypeMetadataExtension
    {
        /// <summary>
        /// Строка иерархического пути (например, "AI/Nodes/Conditions"). Если атрибут отсутствует, возвращает пустую строку.
        /// <br/><br/>
        /// The hierarchical path string (e.g., "AI/Nodes/Conditions"). Returns an empty string if the attribute is missing.
        /// </summary>
        public string Path { get; private set; } = string.Empty;

        /// <summary>
        /// Инициализирует расширение, пытаясь найти атрибут пути категории на указанном типе через рефлексию.
        /// <br/><br/>
        /// Initializes the extension by attempting to find the subclass path attribute on the specified type using reflection.
        /// </summary>
        /// <param name="type">Исследуемый тип конкретной реализации класса.<br/><br/>The specific class implementation type to inspect.</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста.<br/><br/>The base type or interface of the context field.</param>
        public override void Initialize(Type type, Type baseType)
        {
            var attr = type.GetCustomAttributes(typeof(SubclassPathAttribute), false).FirstOrDefault() as SubclassPathAttribute;
            if (attr != null) Path = attr.Path;
        }
    }
}