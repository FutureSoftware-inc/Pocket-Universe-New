using System;

namespace CrystalEngine
{
    /// <summary>
    /// Атрибут для назначения кастомной иконки классу в выпадающем списке селектора типов.
    /// Позволяет визуально выделить тип в инспекторе, связав его с текстурой или встроенным графическим ресурсом Unity.
    /// <br/><br/>
    /// An attribute for assigning a custom icon to a class inside the type selector dropdown menu.
    /// Allows visually distinguishing a type in the Inspector by associating it with a texture or built-in Unity GUI asset.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SelectorIconAttribute : Attribute
    {
        /// <summary>
        /// Имя или путь к графическому ресурсу иконки, которая будет отображаться рядом с элементом.
        /// <br/><br/>
        /// The name or path of the icon graphic resource to be displayed next to the element.
        /// </summary>
        public string IconName { get; }

        /// <summary>
        /// Инициализирует новый экземпляр атрибута <see cref="SelectorIconAttribute"/> с указанием имени или пути к иконке.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="SelectorIconAttribute"/> class with the specified icon name or path.
        /// </summary>
        /// <param name="iconName">Имя или идентификатор иконки для загрузки.<br/><br/>The name or identifier of the icon to load.</param>
        public SelectorIconAttribute(string iconName)
        {
            IconName = iconName;
        }
    }
}
