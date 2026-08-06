using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    /// <summary>
    /// Кастомный UI-компонент кнопки сброса (полиморфного поля в null) для CrystalEngine.
    /// </summary>
    public sealed class ResetButtonComponent : VisualComponent<Button>
    {
        private readonly Action _onClicked;

        /// <summary>
        /// Инициализирует компонент кнопки сброса.
        /// </summary>
        /// <param name="onClicked">Обратный вызов при клике на кнопку.</param>
        public ResetButtonComponent(Action onClicked) : base()
        {
            _onClicked = onClicked ?? throw new ArgumentNullException(nameof(onClicked));

            // Безопасно привязываем клик через встроенный механизм Clickable, избегая утечек памяти в Drawer
            Root.clickable.clicked += _onClicked;
        }

        /// <summary>
        /// Этап 1: Стилизация кнопки сброса (красный крестик, фиксированные размеры).
        /// </summary>
        protected override void SetBaseStyle()
        {
            Root.text = "✕";
            Root.style.width = 20f;
            Root.style.height = 18f;
            Root.style.marginLeft = 2f;
            Root.style.paddingLeft = 0f;
            Root.style.paddingRight = 0f;
            Root.style.color = new Color(0.7f, 0.3f, 0.3f);
            Root.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        /// <summary>
        /// Этап 2: У кнопки сброса нет внутренних дочерних элементов, оставляем пустым.
        /// </summary>
        protected override void BuildStructure() { }

        /// <summary>
        /// Этап 3: Обновление видимости кнопки в зависимости от того, выбран ли какой-то тип.
        /// </summary>
        /// <param name="hasValue">True, если в поле сейчас находится объект (не null).</param>
        public void Refresh(bool hasValue)
        {
            Root.style.display = hasValue ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
