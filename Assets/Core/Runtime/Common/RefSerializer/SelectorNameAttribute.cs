using System;

namespace CrystalEngine
{
    /// <summary>
    /// Атрибут для назначения кастомного (отображаемого) имени классу в выпадающем списке селектора типов.
    /// Заменяет стандартное техническое имя класса на понятное пользователю название в инспекторе.
    /// <br/><br/>
    /// An attribute for assigning a custom (display) name to a class inside the type selector dropdown menu.
    /// Replaces the default technical class name with a user-friendly name in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SelectorNameAttribute : Attribute
    {
        /// <summary>
        /// Кастомное имя класса, отображаемое в выпадающем списке селектора.
        /// <br/><br/>
        /// The custom name of the class displayed in the selector dropdown menu.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Инициализирует новый экземпляр атрибута <see cref="SelectorNameAttribute"/> с указанным кастомным именем.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="SelectorNameAttribute"/> class with the specified custom name.
        /// </summary>
        /// <param name="name">Отображаемое имя класса.<br/><br/>The display name of the class.</param>
        public SelectorNameAttribute(string name)
        {
            Name = name;
        }
    }
}
