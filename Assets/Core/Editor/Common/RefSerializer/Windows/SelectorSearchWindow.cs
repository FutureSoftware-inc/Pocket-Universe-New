using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public sealed class SelectorSearchWindow : EditorWindow
    {
        private List<YamlValidator.BrokenAssetResult> _brokenAssets = new();

        private ListView _listView;
        private TextField _oldClassField;
        private TextField _newClassField;
        private Button _fixButton;

        private YamlValidator.BrokenAssetResult? _selectedResult;

        [MenuItem("Tools/Crystal/SerializeReference Validator")]
        public static void ShowWindow()
        {
            SelectorSearchWindow window = GetWindow<SelectorSearchWindow>();
            window.titleContent = new GUIContent("Ref Search & Validator");
            window.minSize = new Vector2(500f, 400f);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 8f;
            root.style.paddingRight = 8f;
            root.style.paddingTop = 8f;
            root.style.paddingBottom = 8f;

            VisualElement topPanel = new VisualElement();
            topPanel.style.flexDirection = FlexDirection.Row;
            topPanel.style.marginBottom = 6f;
            root.Add(topPanel);

            Button scanButton = new Button { text = "Scan Project For Broken Types" };
            scanButton.style.flexGrow = 1f;
            scanButton.style.height = 24f;
            scanButton.clicked += OnScanPressed;
            topPanel.Add(scanButton);

            Label listLabel = new Label("Broken Assets List:");
            listLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            listLabel.style.marginBottom = 2f;
            root.Add(listLabel);

            _listView = new ListView
            {
                makeItem = () => new Label(),
                bindItem = (element, index) =>
                {
                    var result = _brokenAssets[index];
                    string fileName = System.IO.Path.GetFileName(result.AssetPath);
                    ((Label)element).text = $"[{fileName}] -> Missing: {result.MissingClassName}";
                },
                itemsSource = _brokenAssets,
                selectionType = SelectionType.Single
            };
            _listView.style.flexGrow = 1f;
            _listView.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            _listView.style.borderBottomWidth = 1f;
            _listView.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
            _listView.style.marginBottom = 8f;

            _listView.selectionChanged += OnSelectionChanged;
            root.Add(_listView);

            VisualElement fixPanel = new VisualElement();
            fixPanel.style.borderBottomWidth = 1f;
            fixPanel.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
            fixPanel.style.paddingLeft = 4f;
            fixPanel.style.paddingRight = 4f;
            fixPanel.style.paddingTop = 4f;
            fixPanel.style.paddingBottom = 4f;
            root.Add(fixPanel);

            _oldClassField = new TextField("Missing Class Name:");
            _oldClassField.SetEnabled(false);
            fixPanel.Add(_oldClassField);

            _newClassField = new TextField("Target New Class Name:")
            {
                tooltip = "Enter full type name (e.g. MyNewCondition)"
            };
            fixPanel.Add(_newClassField);


            _fixButton = new Button { text = "Fix Selected Asset" };
            _fixButton.style.height = 22f;
            _fixButton.style.marginTop = 4f;
            _fixButton.clicked += OnFixPressed;
            _fixButton.SetEnabled(false);
            fixPanel.Add(_fixButton);
        }

        private void OnScanPressed()
        {
            _brokenAssets = YamlValidator.ScanProjectForBrokenReferences();
            _listView.itemsSource = _brokenAssets;
            _listView.Rebuild();
            _selectedResult = null;
            _oldClassField.value = string.Empty;
            _fixButton.SetEnabled(false);
            if (_brokenAssets.Count == 0)
            {
                EditorUtility.DisplayDialog("Scan Completed", "No broken SerializeReference types found! Everything is clean.", "OK");
            }
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = _listView.selectedItem;
            if (selected is YamlValidator.BrokenAssetResult result)
            {
                _selectedResult = result;
                _oldClassField.value = result.MissingClassName;
                _fixButton.SetEnabled(true);
            }
        }

        private void OnFixPressed()
        {
            if (_selectedResult == null) return;

            string targetNewClass = _newClassField.value.Trim();
            if (string.IsNullOrEmpty(targetNewClass))
            {
                EditorUtility.DisplayDialog("Validation Error", "Please enter a valid target class name to replace the missing one.", "OK");
                return;
            }

            var asset = _selectedResult.Value;
            bool success = YamlValidator.FixBrokenReference(asset.AssetPath, asset.MissingClassName, targetNewClass);

            if (success)
            {
                EditorUtility.DisplayDialog("Success", $"Asset text-data repaired successfully!\nRefreshed: {System.IO.Path.GetFileName(asset.AssetPath)}", "OK");
                _brokenAssets.Remove(asset);
                _listView.Rebuild();
                _listView.ClearSelection();

                _selectedResult = null;
                _oldClassField.value = string.Empty;
                _fixButton.SetEnabled(false);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to fix reference. Please check console for details.", "OK");
            }
        }
    }
}
