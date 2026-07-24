using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public sealed class AssetPathComponent : VisualComponent<VisualElement>
    {
        private readonly Func<Type> _getCurrentAssetType;

        // ИСПРАВЛЕНИЕ: Добавляем событие уведомления об изменении пути
        private readonly Action _onPathChanged;

        private TextField _pathTextField;

        // Передаем колбэк через конструктор
        public AssetPathComponent(Func<Type> getCurrentAssetType, Action onPathChanged)
        {
            _getCurrentAssetType = getCurrentAssetType;
            _onPathChanged = onPathChanged;
        }

        /// <summary>
        /// Возвращает текущий актуальный относительный путь из текстового поля.
        /// </summary>
        public string CurrentPath => _pathTextField != null ? _pathTextField.value : "Assets";


        protected override void ApplyStyles()
        {
            Root.name = "DynamicPathBar";
            Root.style.height = 30;
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.alignItems = Align.Center;
            Root.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            Root.style.paddingLeft = 10;
            Root.style.paddingRight = 10;
            Root.style.borderBottomWidth = 1;
            Root.style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f);
            Root.style.display = DisplayStyle.None;
        }

        protected override void BuildStructure()
        {
            Root.Add(new Label("Save folder:") { style = { marginRight = 8, fontSize = 11, color = new Color(0.7f, 0.7f, 0.7f) } });

            _pathTextField = new TextField { style = { flexGrow = 1, marginRight = 5 } };
            _pathTextField.isReadOnly = true;
            Root.Add(_pathTextField);

            Button browseButton = new Button(HandleBrowseClick) { text = "...", style = { width = 35, height = 20 } };
            Root.Add(browseButton);
        }

        public override void Refresh()
        {
            Type currentType = _getCurrentAssetType?.Invoke();
            if (currentType != null && _pathTextField != null)
            {
                _pathTextField.value = AssetPathSelector.GetDefaultPathForAsset(currentType);
            }
        }

        private void HandleBrowseClick()
        {
            Type currentType = _getCurrentAssetType?.Invoke();
            if (currentType == null) return;

            string currentFolder = _pathTextField.value.Replace("Assets", Application.dataPath);
            string absolutePath = EditorUtility.OpenFolderPanel("Select the default folder", currentFolder, "");

            if (!string.IsNullOrEmpty(absolutePath) && absolutePath.StartsWith(Application.dataPath))
            {
                string relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                AssetPathSelector.SetDefaultPathForAsset(currentType, relativePath);
                _pathTextField.value = relativePath;

                // ИСПРАВЛЕНИЕ: Дергаем окно, что данные обновились
                _onPathChanged?.Invoke();
            }
        }
    }
}
