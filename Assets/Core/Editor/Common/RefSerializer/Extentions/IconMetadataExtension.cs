using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Расширение метаданных, отвечающее за загрузку и хранение текстур иконок для классов в селекторе типов.
    /// Считывает данные из кастомного атрибута <see cref="SelectorIconAttribute"/> и поддерживает как встроенные ресурсы Unity, так и кастомные ассеты.
    /// <br/><br/>
    /// A metadata extension responsible for loading and storing class icon textures inside the type selector.
    /// Reads data from the custom <see cref="SelectorIconAttribute"/> attribute and supports both built-in Unity resources and custom assets.
    /// </summary>
    public sealed class IconMetadataExtension : TypeMetadataExtension
    {
        /// <summary>
        /// Загруженная текстура иконки для отображения в выпадающем списке. Может быть null, если иконка не задана.
        /// <br/><br/>
        /// The loaded icon texture to display in the dropdown menu. Can be null if no icon is specified.
        /// </summary>
        public Texture2D Icon { get; private set; }

        /// <summary>
        /// Инициализирует расширение, извлекая имя или путь к иконке из атрибута, и пытается загрузить её сначала из встроенной базы Unity, а затем по путям проекта.
        /// <br/><br/>
        /// Initializes the extension by extracting the icon name or path from the attribute, attempting to load it first from Unity's built-in database and then from project paths.
        /// </summary>
        /// <param name="type">Исследуемый тип конкретной реализации класса. / The specific class implementation type to inspect.</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста. / The base type or interface of the context field.</param>
        public override void Initialize(Type type, Type baseType)
        {
            var attr = type.GetCustomAttributes(typeof(SelectorIconAttribute), false).FirstOrDefault() as SelectorIconAttribute;
            if (attr == null || string.IsNullOrEmpty(attr.IconName)) return;

            // Сначала пытаемся найти иконку во встроенных ресурсах редактора (например, "d_Prefab Icon"), 
            // если не находим — ищем кастомную текстуру в папке проекта по указанному пути ассета
            Icon = EditorGUIUtility.IconContent(attr.IconName)?.image as Texture2D
                   ?? AssetDatabase.LoadAssetAtPath<Texture2D>(attr.IconName);
        }
    }
}