using CrystalEngine;
using CrystalEngine.HFSM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public sealed class StateGraphEditorView : AssetEditorView,
        ISavableModule,
        ICreatableModule,
        ISubViewSupport
    {
        private enum SubViewMode : byte { Create = 0, Library = 1 }

        private SubViewMode _currentMode = SubViewMode.Create;
        private BehaviourGraphData _currentOpenedAsset;
        private LibraryGridComponent<BehaviourGraphData> _libraryGrid;

        private GridGraphView _graphView;
        private NodeSearchController _searchController;

        // Модуль четко декларирует, с каким типом данных он работает
        public override Type TargetAssetType => typeof(BehaviourGraphData);

        protected override void OnInitialize()
        {
            _graphView = new GridGraphView();
            ConfigureElementFullscreen(_graphView);

            _searchController = new NodeSearchController(_graphView, HandleTypeSelectedFromMenu);

            // ИСПОЛЬЗУЕМ ТВОЙ СЕРВИС: Запрашиваем из TypeRegistry все типы в проекте, 
            // у которых общим предком является корневой интерфейс IStateBase
            Type[] availableStateTypes = TypeRegistry.GetImplementations(typeof(IState<IBlackboardProvider>)).ToArray();
            _searchController.Initialize(availableStateTypes);

            // Твой неизмененный код инициализации библиотеки и монтирования элементов
            _libraryGrid = new LibraryGridComponent<BehaviourGraphData>(
                getSearchPath: () => AssetPathSelector.GetDefaultPathForAsset(TargetAssetType),
                onAssetSelected: (asset) =>
                {
                    _currentMode = SubViewMode.Create;
                    string assetPath = AssetDatabase.GetAssetPath(asset);
                    OpenTarget(new UnityObjectEditorTarget(asset, assetPath));
                }
            );

            RefreshContent();
        }

        public void SetSubViewMode(byte modeId)
        {
            _currentMode = (SubViewMode)modeId;
            RefreshContent();
        }

        public void NotifyPathChanged() => RefreshContent();

        private void RefreshContent()
        {
            TargetContainer.Clear();

            if (_currentMode == SubViewMode.Create)
            {
                TargetContainer.Add(_graphView);
                SyncVisualNodes();
            }
            else if (_currentMode == SubViewMode.Library)
            {
                _libraryGrid.Refresh();
                TargetContainer.Add(_libraryGrid.Root);
            }
        }

        private void HandleTypeSelectedFromMenu(Type selectedType, Vector2 graphPosition)
        {
            GraphNodeData metadata = NodeFactory.CreateMetadata(selectedType, graphPosition);
            StateNode visualNode = new StateNode(metadata);
            _graphView.AddElement(visualNode);
        }

        public override void OpenTarget(IEditorTarget target)
        {
            if (_graphView == null) return;

            _currentOpenedAsset = target?.GetAs<BehaviourGraphData>();
            if (_currentMode == SubViewMode.Create)
            {
                SyncVisualNodes();
            }
        }

        private void SyncVisualNodes()
        {
            if (_graphView == null) return;
            _graphView.DeleteElements(_graphView.graphElements);

            if (_currentOpenedAsset == null) return;

            foreach (GraphNodeData nodeMetadata in _currentOpenedAsset.EditorNodes)
            {
                StateNode visualNode = new StateNode(nodeMetadata);
                _graphView.AddElement(visualNode);
            }
        }

        void ICreatableModule.ExecuteCreate()
        {
            string relativeFolderPath = AssetPathSelector.GetDefaultPathForAsset(TargetAssetType);
            BehaviourGraphData newAsset = SaveLoadService.CreateNewAssetFile<BehaviourGraphData>(relativeFolderPath, "NewBehaviourGraph");

            if (newAsset == null) return;

            _currentOpenedAsset = newAsset;
            _currentMode = SubViewMode.Create;
            RefreshContent();
        }

        void ISavableModule.ExecuteSave()
        {
            if (_currentOpenedAsset == null)
            {
                EditorUtility.DisplayDialog("Saving the graph", "No active asset opened to save!", "OK");
                return;
            }

            List<GraphNodeData> nodesToSave = new();
            List<IState<IBlackboardProvider>> runtimeStatesToSave = new();

            _graphView.nodes.ForEach(node =>
            {
                if (node is GridNode visualNode)
                {
                    var currentData = new GraphNodeData(visualNode.Guid, visualNode.GetPosition().position, visualNode.title);
                    nodesToSave.Add(currentData);
                }
            });

            _currentOpenedAsset.SaveGraph(nodesToSave, runtimeStatesToSave);
            SaveLoadService.Save(_currentOpenedAsset, "CrystalEngine: Save HFSM State Graph");
            Debug.Log($"[CrystalEditor] Graph successfully synchronized: {_currentOpenedAsset.name}");
        }

        private void ConfigureElementFullscreen(VisualElement element)
        {
            element.style.flexGrow = 1;
            element.style.width = Length.Percent(100);
            element.style.height = Length.Percent(100);
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
