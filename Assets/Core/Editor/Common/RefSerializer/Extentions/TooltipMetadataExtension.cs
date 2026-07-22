using System;
using System.Linq;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Расширение метаданных, отвечающее за извлечение и хранение всплывающих подсказок (tooltips) для классов в селекторе типов.
    /// Считывает данные из кастомного атрибута <see cref="SelectorTooltipAttribute"/>.
    /// <br/><br/>
    /// A metadata extension responsible for extracting and storing class tooltips inside the type selector.
    /// Reads data from the custom <see cref="SelectorTooltipAttribute"/> attribute.
    /// </summary>
    public sealed class TooltipMetadataExtension : TypeMetadataExtension
    {
        /// <summary>
        /// Текст всплывающей подсказки для класса. Если атрибут отсутствует, возвращает пустую строку.
        /// <br/><br/>
        /// The tooltip text for the class. Returns an empty string if the attribute is missing.
        /// </summary>
        public string Tooltip { get; private set; } = string.Empty;

        /// <summary>
        /// Инициализирует расширение, пытаясь найти атрибут подсказки на указанном типе через рефлексию.
        /// <br/><br/>
        /// Initializes the extension by attempting to find the tooltip attribute on the specified type using reflection.
        /// </summary>
        /// <param name="type">Исследуемый тип конкретной реализации класса. / The specific class implementation type to inspect.</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста. / The base type or interface of the context field.</param>
        public override void Initialize(Type type, Type baseType)
        {
            var attr = type.GetCustomAttributes(typeof(SelectorTooltipAttribute), false).FirstOrDefault() as SelectorTooltipAttribute;
            if (attr != null) Tooltip = attr.Tooltip;
        }
    }
}