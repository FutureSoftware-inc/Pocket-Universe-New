using System;

namespace CrystalEngine
{
    /// <summary>
    /// Атрибут для определения пути в иерархии папок выпадающего меню селектора типов.
    /// Позволяет группировать подклассы по категориям (например, "AI/Nodes/Conditions"), аналогично меню добавления компонентов в Unity.
    /// <br/><br/>
    /// An attribute for defining a path in the type selector dropdown menu hierarchy.
    /// Allows grouping subclasses into categories (e.g., "AI/Nodes/Conditions"), mimicking Unity's component selection menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SubclassPathAttribute : Attribute
    {
        /// <summary>
        /// Путь к категории в меню селектора, разделенный символом косой черты (слешем).
        /// <br/><br/>
        /// The category path in the selector menu, separated by a forward slash.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Инициализирует новый экземпляр атрибута <see cref="SubclassPathAttribute"/> с указанием иерархического пути.
        /// <br/><br/>
        /// Initializes a new instance of the <see cref="SubclassPathAttribute"/> class with the specified hierarchical path.
        /// </summary>
        /// <param name="path">Строка пути с категориями для группировки в меню. / The path string with categories for menu grouping.</param>
        public SubclassPathAttribute(string path)
        {
            Path = path;
        }
    }
}
