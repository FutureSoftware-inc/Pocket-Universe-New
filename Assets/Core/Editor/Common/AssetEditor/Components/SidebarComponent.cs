using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public sealed class SidebarComponent : VisualComponent<ScrollView>
    {
        private readonly Action<ViewData> _onModuleSelected;
        private readonly Func<IReadOnlyList<ViewData>> _getViewsData;

        public SidebarComponent(Action<ViewData> onModuleSelected, Func<IReadOnlyList<ViewData>> getViewsData)
        {
            _onModuleSelected = onModuleSelected;
            _getViewsData = getViewsData;

            // Запускаем первичное наполнение данными
            Refresh();
        }

        protected override void ApplyStyles()
        {
            Root.name = "Sidebar";
            Root.style.width = 200;
            Root.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            Root.style.borderRightWidth = 1;
            Root.style.borderRightColor = new Color(0.12f, 0.12f, 0.12f);
        }

        protected override void BuildStructure()
        {
            // Корневая структура статична, динамика идет в Refresh
        }

        public override void Refresh()
        {
            Root.Clear();
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

                Root.Add(viewButton);
            }
        }
    }
}
