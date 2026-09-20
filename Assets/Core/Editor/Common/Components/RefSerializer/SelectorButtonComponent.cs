using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    /// <summary>
    /// Кастомный UI-компонент кнопки-селектора для отображения и выбора полиморфного типа.
    /// </summary>
    public sealed class SelectorButtonComponent : VisualComponent<Button>
    {
        private readonly Action _onClicked;

        /// <summary>
        /// Инициализирует компонент кнопки-селектора.
        /// </summary>
        /// <param name="onClicked">Обратный вызов для открытия выпадающего меню (AdvancedDropdown).</param>
        public SelectorButtonComponent(Action onClicked) : base()
        {
            _onClicked = onClicked ?? throw new ArgumentNullException(nameof(onClicked));

            // Безопасная подписка на клик, исключающая утечки памяти в инспекторе Unity
            Root.clickable.clicked += _onClicked;
        }

        /// <summary>
        /// Этап 1: Настройка геометрии кнопки-селектора.
        /// </summary>
        protected override void SetBaseStyle()
        {
            Root.style.flexGrow = 1f;
            Root.style.marginLeft = 4f;
            Root.style.unityTextAlign = TextAnchor.MiddleLeft; // Выравнивание текста по левому краю для аккуратного вида
        }

        /// <summary>
        /// Этап 2: У кнопки нет вложенной структуры UI элементов.
        /// </summary>
        protected override void BuildStructure() { }

        /// <summary>
        /// Этап 3: Декларативное обновление текста кнопки на основе текущего типа и контекста.
        /// </summary>
        /// <param name="currentValue">Текущий тип объекта (может быть null).</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста.</param>
        public void Refresh(Type currentValue, Type baseType)
        {
            Root.text = GetCleanTypeName(currentValue, baseType);
        }

        /// <summary>
        /// Внутренний хелпер для красивого форматирования имени типа (скрывает технические символы `1 и подставляет аргументы).
        /// </summary>
        private string GetCleanTypeName(Type type, Type baseType)
        {
            if (type == null)
                return "Select Type...";

            if (!type.IsGenericType)
                return type.Name;

            string cleanName = type.Name.Split('`')[0];
            if (baseType != null && baseType.IsGenericType)
            {
                string contextArgs = string.Join(", ", baseType.GetGenericArguments().Select(t => t.Name));
                return $"{cleanName}<{contextArgs}>";
            }

            return $"{cleanName}<>";
        }
    }
}
