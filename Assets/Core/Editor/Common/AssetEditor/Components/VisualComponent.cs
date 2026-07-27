using System;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    /// <summary>
    /// Абстрактный базовый класс для всех кастомных компонентов интерфейса в CrystalEngine.
    /// Гарантирует единый жизненный цикл верстки, стилизации и обновления состояния.
    /// </summary>
    /// <typeparam name="TElement">Тип корневого контейнера (VisualElement, ScrollView и т.д.)</typeparam>
    public abstract class VisualComponent<TElement> where TElement : VisualElement, new()
    {
        /// <summary>
        /// Корневой визуальный элемент данного компонента.
        /// </summary>
        public TElement Root { get; private set; }

        public VisualComponent()
        {
            // 1. Создаем корневой контейнер нужного типа
            Root = new TElement();

            // 2. Вызываем строго последовательный жизненный цикл сборки
            SetBaseStyle();
            BuildStructure();
        }

        /// <summary>
        /// Этап 1: Настройка внешнего вида, геометрии, отступов и цветов контейнера.
        /// </summary>
        protected abstract void SetBaseStyle();

        /// <summary>
        /// Этап 2: Наполнение контейнера дочерними элементами (кнопками, полями, текстом).
        /// </summary>
        protected abstract void BuildStructure();

        /// <summary>
        /// Этап 3: Реактивное обновление содержимого или данных внутри компонента при изменении внешнего состояния.
        /// </summary>
        public virtual void Refresh() { }
    }
}