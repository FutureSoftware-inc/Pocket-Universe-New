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

        private SidebarComponent _sidebar;
        private VisualElement _rightWorkspaceContainer;

        [MenuItem("Tools/Crystal/Universal asset editor")]
        public static void ShowWindow()
        {
            UniversalAssetEditor window = GetWindow<UniversalAssetEditor>();
            window.titleContent = new GUIContent("Universal Hub");
            window.minSize = new Vector2(800f, 500f);
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
            VisualElement root = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

            // 1. Левая панель (Сайдбар) — остается здесь, так как она глобальна для окна
            _sidebar = new SidebarComponent(onModuleSelected: SelectModule, getViewsData: () => _registeredViews);
            root.Add(_sidebar.Root);

            // 2. Правая рабочая область — абсолютно ПУСТАЯ коробка-контейнер
            _rightWorkspaceContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.15f, 0.15f, 0.15f) } };
            root.Add(_rightWorkspaceContainer);

            rootVisualElement.Add(root);

            // Если модуль уже был выбран до перезагрузки, восстанавливаем его
            RefreshWorkspace();
        }

        private void SelectModule(ViewData registeredView)
        {
            if (_selectedModuleData?.ViewType == registeredView.ViewType) return;
            CleanCurrentView();

            _selectedModuleData = registeredView;

            try
            {
                // Инстанцируем View (например, StateGraphEditorView)
                _currentActiveView = (AssetEditorView)Activator.CreateInstance(registeredView.ViewType);

                // Передаем ссылку на окно и на правый пустой контейнер, где View развернет СВОЙ UI
                _currentActiveView.Initialize(this, _rightWorkspaceContainer);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CrystalEditor] Module initialization error: {ex.Message}");
                CleanCurrentView();
            }
        }

        private void RefreshWorkspace()
        {
            if (_selectedModuleData.HasValue)
            {
                SelectModule(_selectedModuleData.Value);
            }
            else
            {
                _rightWorkspaceContainer.Clear();
                Label placeholder = new Label("Select a module in the left panel to get started.")
                { style = { alignSelf = Align.Center, marginTop = 50, color = new Color(0.5f, 0.5f, 0.5f) } };
                _rightWorkspaceContainer.Add(placeholder);
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
    }
}
