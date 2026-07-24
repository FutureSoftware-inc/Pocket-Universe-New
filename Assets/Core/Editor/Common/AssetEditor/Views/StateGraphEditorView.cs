using CrystalEngine;
using CrystalEngine.HFSM; // Подключаем пространство имен твоего ассета данных
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    /// <summary>
    /// Представление редактора графа состояний (HFSM). 
    /// Координирует работу компонентов интерфейса, холста и файловых операций.
    /// </summary>
    public sealed class StateGraphEditorView : AssetEditorView
    {
        private enum SubViewMode : byte
        {
            Create = 0,
            Library = 1
        }

        private SubViewMode _currentMode = SubViewMode.Create;
        private BehaviourGraphData _currentOpenedAsset;

        // Изолированные компоненты интерфейса
        private TopPanelComponent _topTabs;
        private AssetPathComponent _pathBar;
        private GraphFileOperationsComponent _fileOperations;
        private VisualElement _innerContentArea;

        // Холст графа и его контроллер поиска
        private GridGraphView _graphView;
        private NodeSearchController _searchController;

        protected override void OnInitialize()
        {
            // 1. Декларативно инстанцируем UI-компоненты
            _topTabs = new TopPanelComponent(onModeChanged: (modeId) => SetMode((SubViewMode)modeId));
            _topTabs.Root.style.display = DisplayStyle.Flex;

            _pathBar = new AssetPathComponent(
                getCurrentAssetType: () => typeof(BehaviourGraphData),
                onPathChanged: RefreshContent
            );
            _pathBar.Root.style.display = DisplayStyle.Flex;

            // Создаем кнопки управления файлами и монтируем их прямо в панель путей
            _fileOperations = new GraphFileOperationsComponent(
                onCreateAssetPressed: ExecuteCreateNewAssetFile,
                onSaveGraphPressed: ExecuteSaveCurrentGraph
            );
            _pathBar.Root.Add(_fileOperations.Root);
            _pathBar.Refresh();

            // 2. Инициализируем зону контента и бесконечный холст графа
            _innerContentArea = new VisualElement();
            ConfigureElementFullscreen(_innerContentArea);

            _graphView = new GridGraphView();
            ConfigureElementFullscreen(_graphView);

            // 3. Инициализируем контроллер поиска нод (Передаем холст и метод спавна)
            _searchController = new NodeSearchController(_graphView, HandleTypeSelectedFromMenu);

            // Регистрируем типы, которые будут доступны для создания в меню ПКМ
            _searchController.Initialize(new Type[] { typeof(BehaviourGraphData) });

            // 4. Собираем монолитную структуру внутри контейнера главного окна
            TargetContainer.Add(_topTabs.Root);
            TargetContainer.Add(_pathBar.Root);
            TargetContainer.Add(_innerContentArea);

            // Запускаем первичное отображение
            RefreshContent();
        }

        private void SetMode(SubViewMode mode)
        {
            _currentMode = mode;
            RefreshContent();
        }

        /// <summary>
        /// Логика локальной перерисовки экрана модуля в зависимости от выбранной вкладки.
        /// </summary>
        private void RefreshContent()
        {
            _innerContentArea.Clear();

            if (_currentMode == SubViewMode.Create)
            {
                _innerContentArea.Add(_graphView);

                // Если ассет уже был открыт ранее, сохраняем его на холсте, иначе открываем пустой
                OpenAsset(_currentOpenedAsset);
            }
            else if (_currentMode == SubViewMode.Library)
            {
                _innerContentArea.Add(BuildLibraryGrid());
            }
        }

        /// <summary>
        /// Перехватывает выбранный тип из меню поиска ПКМ и инициирует создание узла.
        /// </summary>
        private void HandleTypeSelectedFromMenu(Type selectedType, Vector2 graphPosition)
        {
            // Архитектурно разделяем создание данных (Фабрика) и создание визуала
            GraphNodeData metadata = NodeFactory.CreateMetadata(selectedType, graphPosition);
            StateNode visualNode = new StateNode(metadata);

            _graphView.AddElement(visualNode);
        }

        public override void OpenAsset(ScriptableObject asset)
        {
            if (_graphView == null) return;

            _graphView.DeleteElements(_graphView.graphElements);
            _currentOpenedAsset = asset as BehaviourGraphData;

            if (_currentOpenedAsset == null) return;

            // Восстанавливаем сохраненные ноды на холсте из файла метаданных
            foreach (GraphNodeData nodeMetadata in _currentOpenedAsset.EditorNodes)
            {
                StateNode visualNode = new StateNode(nodeMetadata);
                _graphView.AddElement(visualNode);
            }
        }

        /// <summary>
        /// Сканирует выбранную папку через нативный кэш Unity и строит сетку библиотеки ассетов.
        /// </summary>
        private VisualElement BuildLibraryGrid()
        {
            ScrollView scrollView = new ScrollView { name = "LibraryGrid", style = { flexGrow = 1, paddingTop = 10, paddingLeft = 10 } };
            VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            scrollView.Add(container);

            string currentRelativeFolder = _pathBar != null ? _pathBar.CurrentPath : "Assets";
            string[] searchFolders = new string[] { currentRelativeFolder };

            // Нативный поиск Unity по типу данных внутри выбранной папки
            string[] guids = AssetDatabase.FindAssets("t:BehaviourGraphData", searchFolders);

            if (guids.Length == 0)
            {
                Label emptyLabel = new Label($"In the folder '{currentRelativeFolder}' no state machine assets found.")
                { style = { color = new Color(0.5f, 0.5f, 0.5f), marginTop = 20, marginLeft = 10 } };
                container.Add(emptyLabel);
                return scrollView;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BehaviourGraphData asset = AssetDatabase.LoadAssetAtPath<BehaviourGraphData>(path);

                if (asset == null) continue;

                Button assetCard = new Button(() =>
                {
                    _currentMode = SubViewMode.Create;
                    RefreshContent();
                    OpenAsset(asset);
                })
                { text = asset.name };

                assetCard.style.width = 100;
                assetCard.style.height = 100;
                assetCard.style.marginRight = 10;
                assetCard.style.marginBottom = 10;
                container.Add(assetCard);
            }

            return scrollView;
        }

        private void ExecuteCreateNewAssetFile()
        {
            string relativeFolderPath = _pathBar.CurrentPath;
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{relativeFolderPath}/NewBehaviourGraph.asset");

            BehaviourGraphData newAsset = ScriptableObject.CreateInstance<BehaviourGraphData>();
            AssetDatabase.CreateAsset(newAsset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _currentOpenedAsset = newAsset;
            SetMode(SubViewMode.Create);
        }

        private void ExecuteSaveCurrentGraph()
        {
            if (_currentOpenedAsset == null)
            {
                EditorUtility.DisplayDialog("Saving the graph", "No asset has been selected to save! Open an asset from the Library or click 'Create Asset'.", "ОК");
                return;
            }

            System.Collections.Generic.List<GraphNodeData> nodesToSave = new();
            System.Collections.Generic.List<IState<IBlackboardProvider>> runtimeStatesToSave = new();

            _graphView.nodes.ForEach(node =>
            {
                if (node is GridNode visualNode)
                {
                    GraphNodeData currentData = new GraphNodeData(
                        visualNode.Guid,
                        visualNode.GetPosition().position, // Снимаем точный Vector2 с холста GraphView
                        visualNode.title
                    );
                    nodesToSave.Add(currentData);
                }
            });

            // Вызываем твой нативный метод внутри ScriptableObject данных!
            _currentOpenedAsset.SaveGraph(nodesToSave, runtimeStatesToSave);

            EditorUtility.SetDirty(_currentOpenedAsset);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CrystalEditor] The graph has been successfully saved in the asset: {_currentOpenedAsset.name} (Saved node: {nodesToSave.Count})");
        }

        private void ConfigureElementFullscreen(VisualElement element)
        {
            element.style.flexGrow = 1;
            element.style.width = Length.Percent(100);
        }

        public override void OnDisable()
        {
            if (TargetContainer != null) TargetContainer.Clear();

            _searchController?.Dispose();
            _searchController = null;

            if (_graphView != null)
            {
                _graphView.RemoveFromHierarchy();
                _graphView = null;
            }
        }
    }
}
