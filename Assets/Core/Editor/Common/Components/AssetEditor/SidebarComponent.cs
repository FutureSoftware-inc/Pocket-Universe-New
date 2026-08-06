using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    // Меняем контейнер на VisualElement, чтобы жестко зафиксировать кнопку внизу
    public sealed class SidebarComponent : VisualComponent<VisualElement>
    {
        // Единственный источник правды для размеров левой панели
        public const float DefaultWidth = 200f;
        public const float MinimumWidth = 150f;

        private readonly Action<ViewData> _onModuleSelected;
        private readonly Func<IReadOnlyList<ViewData>> _getViewsData;
        private readonly Action _onResetLayoutPressed; // Колбэк для сброса

        private ScrollView _modulesScrollView;

        public SidebarComponent(Action<ViewData> onModuleSelected, Func<IReadOnlyList<ViewData>> getViewsData, Action onResetLayoutPressed)
        {
            _onModuleSelected = onModuleSelected;
            _getViewsData = getViewsData;
            _onResetLayoutPressed = onResetLayoutPressed;

            // ИСПРАВЛЕНИЕ: Поля инициализированы! Теперь безопасно находим кнопку в Root и подписываем её
            Button resetBtn = Root.Q<Button>("ResetLayoutButton");
            if (resetBtn != null)
            {
                resetBtn.clicked += () => _onResetLayoutPressed?.Invoke();
            }

            Refresh();
        }


        protected override void SetBaseStyle()
        {
            Root.name = "Sidebar";
            Root.style.flexGrow = 1;
            Root.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            Root.style.borderRightWidth = 1;
            Root.style.borderRightColor = new Color(0.12f, 0.12f, 0.12f);
        }

        protected override void BuildStructure()
        {
            _modulesScrollView = new ScrollView();
            _modulesScrollView.style.flexGrow = 1;
            Root.Add(_modulesScrollView);

            // Создаем "пустую" кнопку без колбэка на этапе верстки. 
            // Даем ей имя (name), чтобы легко найти её позже.
            Button resetBtn = new Button()
            {
                name = "ResetLayoutButton",
                text = "⚙️ Reset Layout",
                style = {
            height = 25,
            marginTop = 5,
            marginBottom = 5,
            marginLeft = 5,
            marginRight = 5,
            backgroundColor = new Color(0.25f, 0.25f, 0.25f)
        }
            };
            Root.Add(resetBtn);
        }

        public override void Refresh()
        {
            if (_modulesScrollView == null) return;
            _modulesScrollView.Clear();

            IReadOnlyList<ViewData> views = _getViewsData?.Invoke();
            if (views == null) return;

            foreach (ViewData viewData in views)
            {
                Button viewButton = new Button(() => _onModuleSelected?.Invoke(viewData))
                {
                    text = viewData.DisplayName
                };
                viewButton.style.height = 30;
                viewButton.style.marginBottom = 2;

                _modulesScrollView.Add(viewButton);
            }
        }
    }
}
