using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public sealed class UniversalAssetEditor : EditorWindow
    {
        private IReadOnlyList<ViewData> _registeredViews;
        private ViewData? _selectedModuleData;
        private AssetEditorView _currentActiveView;

        // Системный каркас интерфейса (Shell)
        private SidebarComponent _sidebar;
        private TopPanelComponent _topTabs;
        private AssetPathComponent _pathBar;
        private FileOperationsComponent _fileOperations;

        private TwoPaneSplitView _splitView;

        private VisualElement _rightWorkspaceContainer;
        private VisualElement _moduleContentContainer;
        private byte _currentSubModeId = 0;

        [MenuItem("Tools/Crystal/Universal asset editor")]
        public static void ShowWindow()
        {
            UniversalAssetEditor window = GetWindow<UniversalAssetEditor>();
            window.titleContent = new GUIContent("Universal Hub");
            window.minSize = new Vector2(900f, 600f);
            window.Show();
        }

        private void OnEnable()
        {
            _registeredViews = EditorViewRegistry.GetViews();
            CreateGUI();
        }

        private void OnDisable() => CleanCurrentView();

        public void CreateGUI()
        {
            rootVisualElement.Clear();

            // Читаем дефолтную ширину из самого компонента сайдбара
            _splitView = new TwoPaneSplitView(0, SidebarComponent.DefaultWidth, TwoPaneSplitViewOrientation.Horizontal);
            _splitView.style.flexGrow = 1;

            _sidebar = new SidebarComponent(
                onModuleSelected: SelectModule,
                getViewsData: () => _registeredViews,
                onResetLayoutPressed: ResetSplitterToDefault
            );

            // Читаем минимальную ширину оттуда же
            _sidebar.Root.style.minWidth = SidebarComponent.MinimumWidth;
            _sidebar.Root.style.width = StyleKeyword.Null;

            _rightWorkspaceContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.15f, 0.15f, 0.15f) } };
            _rightWorkspaceContainer.style.minWidth = 400;

            // (Тут остается твой неизмененный код инициализации _topTabs, _pathBar, _fileOperations и т.д.)
            _topTabs = new TopPanelComponent(HandleSubModeChanged);
            _pathBar = new AssetPathComponent(getCurrentAssetType: () => _currentActiveView?.TargetAssetType, onPathChanged: ForwardPathChangedNotification);
            _fileOperations = new FileOperationsComponent(onCreateAssetPressed: HandleCreatePressed, onSaveGraphPressed: HandleSavePressed);
            _pathBar.Root.Add(_fileOperations.Root);
            _moduleContentContainer = new VisualElement { style = { flexGrow = 1 } };
            _rightWorkspaceContainer.Add(_topTabs.Root);
            _rightWorkspaceContainer.Add(_pathBar.Root);
            _rightWorkspaceContainer.Add(_moduleContentContainer);

            // Добавляем элементы в сохраненный _splitView
            _splitView.Add(_sidebar.Root);
            _splitView.Add(_rightWorkspaceContainer);

            rootVisualElement.Add(_splitView);

            RefreshWorkspace();
        }

        private void SelectModule(ViewData registeredView)
        {
            if (_selectedModuleData?.ViewType == registeredView.ViewType) return;

            CleanCurrentView();
            _selectedModuleData = registeredView;

            try
            {
                _currentActiveView = (AssetEditorView)Activator.CreateInstance(registeredView.ViewType);

                // Передаем модулю ТОЛЬКО его контентную зону, а не всю правую панель!
                _currentActiveView.Initialize(this, _moduleContentContainer);

                // Настраиваем видимость элементов каркаса на основе возможностей модуля
                _topTabs.Root.style.display = DisplayStyle.Flex;
                _pathBar.Root.style.display = DisplayStyle.Flex;
                _pathBar.Refresh();

                UpdateCapabilitiesUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CrystalEditor] Module initialization error: {ex.Message}");
                CleanCurrentView();
            }
        }

        private void UpdateCapabilitiesUI()
        {
            if (_fileOperations == null) return;

            // Динамически управляем доступностью кнопок на основе интерфейсов-маркеров
            _fileOperations.Root.Q<Button>("CreateAssetButton")?.SetEnabled(_currentActiveView is ICreatableModule);
            _fileOperations.Root.Q<Button>("SaveGraphButton")?.SetEnabled(_currentActiveView is ISavableModule);
        }

        private void HandleSubModeChanged(byte modeId)
        {
            _currentSubModeId = modeId;
            // Перенаправляем событие переключения вложений внутрь инкапсулированного метода окна, 
            // если модуль поддерживает внутренние подпредставления (например, библиотеку)
            if (_currentActiveView is ISubViewSupport subViewSupport)
            {
                subViewSupport.SetSubViewMode(modeId);
            }
        }

        private void ForwardPathChangedNotification()
        {
            if (_currentActiveView is ISubViewSupport subViewSupport)
            {
                subViewSupport.NotifyPathChanged();
            }
        }

        private void HandleCreatePressed()
        {
            if (_currentActiveView is ICreatableModule creatable) creatable.ExecuteCreate();
        }

        private void HandleSavePressed()
        {
            if (_currentActiveView is ISavableModule savable) savable.ExecuteSave();
        }


        private void RefreshWorkspace()
        {
            if (_selectedModuleData.HasValue)
            {
                SelectModule(_selectedModuleData.Value);
            }
            else
            {
                _moduleContentContainer.Clear();
                _topTabs.Root.style.display = DisplayStyle.None;
                _pathBar.Root.style.display = DisplayStyle.None;

                Label placeholder = new Label("Select a module in the left panel to get started.")
                { style = { alignSelf = Align.Center, marginTop = 50, color = new Color(0.5f, 0.5f, 0.5f) } };
                _moduleContentContainer.Add(placeholder);
            }
        }

        private void CleanCurrentView()
        {
            if (_currentActiveView != null)
            {
                _currentActiveView.OnDisable();
                _currentActiveView = null;
            }
            _selectedModuleData = null;
        }

        private void ResetSplitterToDefault()
        {
            if (_splitView == null) return;

            // 1. Принудительно перезаписываем дефолтное измерение
            _splitView.fixedPaneInitialDimension = SidebarComponent.DefaultWidth;

            // 2. Официальный нативный трюк сброса кэша геометрии:
            // Схлопываем левую панель (индекс 0) и мгновенно её разворачиваем.
            // Это заставляет сплиттер нативно очистить инлайн-стили мыши и применить 200px.
            _splitView.CollapseChild(0);
            _splitView.UnCollapse();

            // 3. На всякий случай просим окно обновиться
            _splitView.MarkDirtyRepaint();

            Debug.Log("[CrystalEditor] Разметка сброшена через нативные Collapse/UnCollapse.");
        }

    }
}