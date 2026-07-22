using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CrystalEngine.HFSM;

namespace CrystalEditor
{
    public sealed class StateGraphEditorView : AssetEditorView
    {
        private BehaviourGraphData _currentAsset;
        private VisualElement _toolbarContainer;
        private VisualElement _graphCanvasContainer;
        private StateVisualGraphView _graphView;

        protected override void OnInitialize()
        {
            _toolbarContainer = new VisualElement { name = "GraphToolbar" };
            _toolbarContainer.style.height = 30;
            _toolbarContainer.style.flexDirection = FlexDirection.Row;
            _toolbarContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            Root.Add(_toolbarContainer);

            Button btnSave = new Button(SaveGraphData) { text = "💾 Сохранить граф" };
            _toolbarContainer.Add(btnSave);

            _graphCanvasContainer = new VisualElement { name = "GraphCanvas" };
            _graphCanvasContainer.style.flexGrow = 1;
            Root.Add(_graphCanvasContainer);
        }

        public override void OpenAsset(ScriptableObject asset)
        {
            _graphCanvasContainer.Clear();

            if (asset == null)
            {
                // По нажатию кнопки вызываем универсальный базовый метод!
                // Передаем целевой тип ScriptableObject и дефолтное имя файла. Всё!
                Button btnCreateFile = new Button(() => CreateNewAsset<BehaviourGraphData>("BehaviourGraph"))
                {
                    text = "⚙ Создать новый файл Графа Поведения на диске"
                };
                btnCreateFile.style.width = 300;
                btnCreateFile.style.height = 40;
                btnCreateFile.style.alignSelf = Align.Center;
                btnCreateFile.style.marginTop = 100;
                _graphCanvasContainer.Add(btnCreateFile);
                return;
            }

            _currentAsset = asset as BehaviourGraphData;
            if (_currentAsset == null) return;

            _graphView = new StateVisualGraphView();
            _graphView.style.flexGrow = 1;
            _graphCanvasContainer.Add(_graphView);
            _graphView.PopulateFromAsset(_currentAsset);
        }

        private void SaveGraphData()
        {
            if (_currentAsset == null || _graphView == null) return;
            AssetDatabase.SaveAssets();
            Debug.Log($"[HFSM Редактор] Граф {_currentAsset.name} успешно сохранен.");
        }

        public override void OnDisable()
        {
            _graphCanvasContainer?.Clear();
            _graphView = null;
            _currentAsset = null;
        }
    }
}
