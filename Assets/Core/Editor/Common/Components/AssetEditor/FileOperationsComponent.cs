using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    /// <summary>
    /// Изолированный UI-компонент кнопок создания и сохранения ассета для панели инструментов.
    /// </summary>
    public sealed class FileOperationsComponent : VisualComponent<VisualElement>
    {
        private readonly Action _onCreateAssetPressed;
        private readonly Action _onSaveGraphPressed;

        public FileOperationsComponent(Action onCreateAssetPressed, Action onSaveGraphPressed)
        {
            _onCreateAssetPressed = onCreateAssetPressed;
            _onSaveGraphPressed = onSaveGraphPressed;
        }

        protected override void SetBaseStyle()
        {
            Root.name = "FileOperationsBar";
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.alignItems = Align.Center;
            Root.style.marginLeft = 10; // Небольшой отступ от текстового поля пути
        }

        protected override void BuildStructure()
        {
            // Кнопка создания нового файла ассета на диске
            Button createAssetBtn = new Button(_onCreateAssetPressed)
            {
                text = "✨ Create asset",
                style = { height = 22, marginRight = 5, backgroundColor = new Color(0.25f, 0.4f, 0.25f) } // Зеленоватый оттенок
            };

            // Кнопка сохранения текущего состояния холста в ассет
            Button saveGraphBtn = new Button(_onSaveGraphPressed)
            {
                text = "💾 Save the graph",
                style = { height = 22, backgroundColor = new Color(0.25f, 0.3f, 0.4f) } // Синеватый оттенок
            };

            Root.Add(createAssetBtn);
            Root.Add(saveGraphBtn);
        }
    }
}