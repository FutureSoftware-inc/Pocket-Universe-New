using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    public class UniversalAssetEditor : EditorWindow
    {
        private List<EditorViewRegistry.ViewData> _registeredViews;
        private EditorViewRegistry.ViewData _selectedModuleData;

        private ScrollView _sidebarContainer;
        private VisualElement _moduleContentContainer;
        private VisualElement _modeButtonsContainer;

        private AssetEditorView _currentActiveView;
        private SubViewMode _currentMode = SubViewMode.Library;

        // ПОЛЯ ДЛЯ СЕРВИСА ПУТЕЙ: Объявлены строго на уровне класса
        private TextField _globalPathTextField;

        private enum SubViewMode
        {
            Library,
            CreateNew
        }

        [MenuItem("Universe/Hub")]
        public static void ShowWindow()
        {
            UniversalAssetEditor window = GetWindow<UniversalAssetEditor>();
            window.titleContent = new GUIContent("Universe Hub");
            window.minSize = new Vector2(800, 500);
        }

        private void OnEnable()
        {
            _registeredViews = EditorViewRegistry.GetViews();
            InitializeUI();
        }

        private void OnDisable()
        {
            _currentActiveView?.OnDisable();
        }

        private void InitializeUI()
        {
            rootVisualElement.Clear();

            VisualElement mainContainer = new VisualElement { name = "MainContainer" };
            mainContainer.style.flexDirection = FlexDirection.Row;
            mainContainer.style.flexGrow = 1;

            _sidebarContainer = new ScrollView { name = "Sidebar" };
            _sidebarContainer.style.width = 200;
            _sidebarContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            _sidebarContainer.style.borderRightWidth = 1;
            _sidebarContainer.style.borderRightColor = new Color(0.12f, 0.12f, 0.12f);

            VisualElement workspace = new VisualElement { name = "Workspace" };
            workspace.style.flexGrow = 1;
            workspace.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

            _modeButtonsContainer = new VisualElement { name = "ModeButtons" };
            _modeButtonsContainer.style.height = 35;
            _modeButtonsContainer.style.flexDirection = FlexDirection.Row;
            _modeButtonsContainer.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            _modeButtonsContainer.style.borderBottomWidth = 1;
            _modeButtonsContainer.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
            _modeButtonsContainer.style.display = DisplayStyle.None;

            _moduleContentContainer = new VisualElement { name = "ModuleContent" };
            _moduleContentContainer.style.flexGrow = 1;

            workspace.Add(_modeButtonsContainer);
            workspace.Add(_moduleContentContainer);

            mainContainer.Add(_sidebarContainer);
            mainContainer.Add(workspace);
            rootVisualElement.Add(mainContainer);

            BuildSidebar();
            BuildModeButtons();
        }

        private void BuildSidebar()
        {
            _sidebarContainer.Clear();

            foreach (var viewData in _registeredViews)
            {
                Button btn = new Button(() => SelectModule(viewData)) { text = viewData.DisplayName };
                btn.style.height = 30;
                btn.style.marginBottom = 2;
                _sidebarContainer.Add(btn);
            }
        }

        private void BuildModeButtons()
        {
            _modeButtonsContainer.Clear();

            Button btnLibrary = new Button(() => SetMode(SubViewMode.Library)) { text = "📚 Библиотека" };
            Button btnCreate = new Button(() => SetMode(SubViewMode.CreateNew)) { text = "➕ Создать новый" };

            btnLibrary.style.flexGrow = 1;
            btnCreate.style.flexGrow = 1;

            _modeButtonsContainer.Add(btnLibrary);
            _modeButtonsContainer.Add(btnCreate);
        }

        private void SelectModule(EditorViewRegistry.ViewData viewData)
        {
            _currentActiveView?.OnDisable();

            _selectedModuleData = viewData;
            _currentActiveView = (AssetEditorView)Activator.CreateInstance(viewData.ViewType);
            _currentActiveView.Initialize(this);

            _modeButtonsContainer.style.display = DisplayStyle.Flex;
            _currentMode = SubViewMode.Library;

            RefreshWorkspaceContent();
        }

        private void SetMode(SubViewMode mode)
        {
            _currentMode = mode;
            RefreshWorkspaceContent();
        }

        // ОБНОВЛЕННЫЙ МЕТОД: Полная автоматическая верстка верхнего бара путей
        private void RefreshWorkspaceContent()
        {
            _moduleContentContainer.Clear();

            if (_selectedModuleData.ViewType == null || _selectedModuleData.AssetType == null)
            {
                _moduleContentContainer.Add(new Label("Выберите модуль в левой панели для начала работы.")
                { style = { alignSelf = Align.Center, marginTop = 50 } });
                return;
            }

            // 1. Создаем контейнер верхнего бара папок
            VisualElement pathBar = new VisualElement { name = "DynamicPathBar" };
            pathBar.style.height = 28;
            pathBar.style.flexDirection = FlexDirection.Row;
            pathBar.style.alignItems = Align.Center;
            pathBar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            pathBar.style.paddingLeft = 5;
            pathBar.style.paddingRight = 5;
            pathBar.style.borderBottomWidth = 1;
            pathBar.style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f);

            pathBar.Add(new Label("Папка сохранения:") { style = { marginRight = 5, fontSize = 11 } });

            // 2. Считываем путь ИЗ ВАШЕГО СЕРВИСА на основе текущего типа ассета
            string currentAssetPath = AssetPathSelector.GetDefaultPathForAsset(_selectedModuleData.AssetType);

            _globalPathTextField = new TextField { value = currentAssetPath, style = { flexGrow = 1, marginRight = 5 } };
            _globalPathTextField.isReadOnly = true;
            pathBar.Add(_globalPathTextField);

            // 3. Создаем кнопку "Обзор" со всей логикой перехвата кликов и записи
            Button browseButton = new Button(() =>
            {
                string currentFolder = _globalPathTextField.value.Replace("Assets", Application.dataPath);
                string absolutePath = EditorUtility.OpenFolderPanel("Выберите папку по умолчанию", currentFolder, "");

                if (!string.IsNullOrEmpty(absolutePath) && absolutePath.StartsWith(Application.dataPath))
                {
                    string relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

                    // ТОЧКА ВЫЗОВА: Вызываем метод записи вашего сервиса!
                    AssetPathSelector.SetDefaultPathForAsset(_selectedModuleData.AssetType, relativePath);

                    // Обновляем визуальное поле
                    _globalPathTextField.value = relativePath;
                }
            })
            { text = "...", style = { width = 30, height = 20 } };

            pathBar.Add(browseButton);

            // Добавляем готовый бар в самый верх рабочего пространства
            _moduleContentContainer.Add(pathBar);

            // 4. Отрисовка контента в зависимости от выбранного режима в хабе
            if (_currentMode == SubViewMode.CreateNew)
            {
                _moduleContentContainer.Add(_currentActiveView.Root);
                _currentActiveView.OpenAsset(null);
            }
            else if (_currentMode == SubViewMode.Library)
            {
                VisualElement libraryRoot = BuildLibraryGrid(_selectedModuleData.AssetType);
                _moduleContentContainer.Add(libraryRoot);
            }
        }

        private VisualElement BuildLibraryGrid(Type assetType)
        {
            ScrollView scrollView = new ScrollView { name = "LibraryGrid" };
            scrollView.style.flexGrow = 1;
            scrollView.style.paddingTop = 10;
            scrollView.style.paddingLeft = 10;

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;
            scrollView.Add(container);

            string[] guids = AssetDatabase.FindAssets($"t:{assetType.Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset == null) continue;

                Button assetCard = new Button(() =>
                {
                    _currentMode = SubViewMode.CreateNew;
                    RefreshWorkspaceContent();
                    _moduleContentContainer.Add(_currentActiveView.Root);
                    _currentActiveView.OpenAsset(asset);
                })
                {
                    text = asset.name
                };

                assetCard.style.width = 100;
                assetCard.style.height = 100;
                assetCard.style.marginRight = 10;
                assetCard.style.marginBottom = 10;
                container.Add(assetCard);
            }

            return scrollView;
        }
    }
}
