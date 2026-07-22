using System;
using System.Linq;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Расширение метаданных, отвечающее за генерацию и хранение читаемого отображаемого имени (Display Name) для классов в селекторе типов.
    /// Автоматически вычисляет приоритетное имя на основе атрибутов именования, путей категорий или структуры обобщенных типов.
    /// <br/><br/>
    /// A metadata extension responsible for generating and storing a human-readable display name for classes inside the type selector.
    /// Automatically calculates the priority name based on naming attributes, category paths, or generic type structures.
    /// </summary>
    public sealed class DisplayNameMetadataExtension : TypeMetadataExtension
    {
        /// <summary>
        /// Вычисленное отображаемое имя класса для выпадающего списка.
        /// <br/><br/>
        /// The calculated display name of the class for the dropdown menu.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Инициализирует расширение, вычисляя имя класса по цепочке приоритетов: кастомное имя -> имя из пути -> форматированное имя типа (с учетом дженериков).
        /// <br/><br/>
        /// Initializes the extension by calculating the class name through a priority chain: custom name -> name from path -> formatted type name (considering generics).
        /// </summary>
        /// <param name="type">Исследуемый тип конкретной реализации класса. / The specific class implementation type to inspect.</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста. / The base type or interface of the context field.</param>
        public override void Initialize(Type type, Type baseType)
        {
            // Приоритет 1: Ищем кастомное имя через SelectorNameAttribute
            var nameAttr = type.GetCustomAttributes(typeof(SelectorNameAttribute), false).FirstOrDefault() as SelectorNameAttribute;
            if (nameAttr != null)
            {
                DisplayName = nameAttr.Name;
                return;
            }

            // Приоритет 2: Если кастомного имени нет, берем последний элемент из пути SubclassPathAttribute (например, из "AI/Nodes/Idle" возьмет "Idle")
            var pathAttr = type.GetCustomAttributes(typeof(SubclassPathAttribute), false).FirstOrDefault() as SubclassPathAttribute;
            if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Path))
            {
                DisplayName = pathAttr.Path.Split('/').Last();
                return;
            }

            // Приоритет 3: Если атрибутов нет, берем техническое имя класса. 
            // Если контекст является дженериком, красиво форматируем закрывающие аргументы (например, "MyState<EnemyContext>")
            string cleanName = type.Name.Split('`')[0];
            DisplayName = (baseType != null && baseType.IsGenericType)
                ? $"{cleanName}<{string.Join(", ", baseType.GetGenericArguments().Select(t => t.Name))}>"
                : type.Name;
        }
    }
}