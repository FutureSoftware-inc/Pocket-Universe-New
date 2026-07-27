using System;

namespace CrystalEngine
{
    /// <summary>
    /// Атрибут для назначения всплывающей подсказки классу в кастомном селекторе типов.
    /// Позволяет выводить описание назначения класса при его выборе в инспекторе.
    /// <br/><br/>
    /// An attribute for assigning a tooltip to a class inside the custom type selector.
    /// Allows displaying a description of the class's purpose when selecting it in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SelectorTooltipAttribute : Attribute
    {
        /// <summary>
        /// Текст всплывающей подсказки, описывающий класс в выпадающем списке селектора.
        /// <br/><br/>
        /// The tooltip text describing the class in the selector dropdown menu.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// Инициализирует новый экземпляр атрибута <see cref="SelectorTooltipAttribute"/> с указанным текстом подсказки.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="SelectorTooltipAttribute"/> class with the specified tooltip text.
        /// </summary>
        /// <param name="tooltip">Текст отображаемой подсказки.<br/><br/>The text of the tooltip to display.</param>
        public SelectorTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}
