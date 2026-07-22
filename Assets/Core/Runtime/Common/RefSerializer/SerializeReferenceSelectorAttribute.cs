using System;
using UnityEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Универсальный атрибут для любых сериализуемых полей, активирующий расширенный селектор типов в инспекторе Unity.
    /// Упрощает выбор и настройку полиморфных полей: интерфейсов, абстрактных и обобщенных классов, а также стандартных типов данных.
    /// <br/><br/>
    /// A universal attribute for any serializable fields that enables an advanced type selector in the Unity Inspector.
    /// Simplifies selecting and configuring polymorphic fields: interfaces, abstract and generic classes, as well as standard data types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SerializeReferenceSelectorAttribute : PropertyAttribute
    {
        /// <summary>
        /// Определяет, должен ли селектор типов отображаться в виде выпадающего списка (Dropdown).
        /// По умолчанию имеет значение true.
        /// <br/><br/>
        /// Determines whether the type selector should be displayed as a dropdown menu.
        /// Defaults to true.
        /// </summary>
        public bool DisplayAsDropdown { get; set; } = true;

        /// <summary>
        /// Кастомный заголовок для поля или выпадающего списка в инспекторе.
        /// <br/><br/>
        /// A custom title for the field or dropdown menu in the Inspector.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Инициализирует новый экземпляр атрибута <see cref="SerializeReferenceSelectorAttribute"/> без кастомного заголовка.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="SerializeReferenceSelectorAttribute"/> class without a custom title.
        /// </summary>
        public SerializeReferenceSelectorAttribute()
        {
            Title = null;
        }

        /// <summary>
        /// Инициализирует новый экземпляр атрибута <see cref="SerializeReferenceSelectorAttribute"/> с указанным кастомным заголовком.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="SerializeReferenceSelectorAttribute"/> class with the specified custom title.
        /// </summary>
        /// <param name="title">Отображаемый заголовок для поля. / The display title for the field.</param>
        public SerializeReferenceSelectorAttribute(string title)
        {
            Title = title;
        }
    }
}
