using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    public sealed class TopPanelComponent : VisualComponent<VisualElement>
    {
        private readonly Action<byte> _onModeChanged;

        public TopPanelComponent(Action<byte> onModeChanged)
        {
            _onModeChanged = onModeChanged;
        }

        protected override void SetBaseStyle()
        {
            Root.name = "TopPanel";
            Root.style.height = 40;
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            Root.style.alignItems = Align.Center;
            Root.style.borderBottomWidth = 1;
            Root.style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f);
            Root.style.display = DisplayStyle.None; // Скрыт по дефолту
        }

        protected override void BuildStructure()
        {
            Button createButton = CreateTabButton("+ Create", () => _onModeChanged?.Invoke(0));
            Button libraryButton = CreateTabButton("📚 Library", () => _onModeChanged?.Invoke(1));

            Root.Add(createButton);
            Root.Add(libraryButton);
        }

        private Button CreateTabButton(string text, Action onClick)
        {
            Button btn = new Button(onClick) { text = text };
            btn.style.height = Length.Percent(100);
            btn.style.flexGrow = 1;
            btn.style.marginLeft = 0; btn.style.marginRight = 0;
            btn.style.marginTop = 0; btn.style.marginBottom = 0;
            btn.style.borderTopLeftRadius = 0; btn.style.borderTopRightRadius = 0;
            btn.style.borderBottomLeftRadius = 0; btn.style.borderBottomRightRadius = 0;
            return btn;
        }
    }
}
