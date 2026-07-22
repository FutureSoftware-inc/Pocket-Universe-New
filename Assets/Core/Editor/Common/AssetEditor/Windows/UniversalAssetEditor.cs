using Crystal.Common.Editor;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    public class UniversalAssetEditor : EditorWindow
    {
        private VisualElement _sidebarContainer;
        private VisualElement _workspaceContainer;
        private VisualElement _topBarContainer;
        private VisualElement _moduleContentContainer;

        private AssetEditorView _currentActiveView;
        private EditorViewRegistry.ViewData _selectedModuleData;

        // Режимы работы правой панели
        private enum SubViewMode { CreateNew, Library }
        private SubViewMode _currentMode = SubViewMode.CreateNew;

        [MenuItem("Tools/Universal Asset Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<UniversalAssetEditor>();
            window.titleContent = new GUIContent("Universe Hub");
            window.minSize = new Vector2(900, 600);
            window.Show();
        }

        public void CreateGUI()
        {
            // Корневой контейнер всего окна (горизонтальное разделение)
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // 1. ЛЕВАЯ ПАНЕЛЬ (Меню выбора редакторов)
            _sidebarContainer = new VisualElement { name = "Sidebar" };
            _sidebarContainer.style.width = 240;
            _sidebarContainer.style.borderRightColor = new Color(0.15f, 0.15f, 0.15f);
            _sidebarContainer.style.borderRightWidth = 2;
            _sidebarContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            root.Add(_sidebarContainer);

            // 2. ПРАВАЯ РАБОЧАЯ ОБЛАСТЬ (Вертикальное разделение: Топ-бар + Контент)
            _workspaceContainer = new VisualElement { name = "Workspace" };
            _workspaceContainer.style.flexGrow = 1;
            root.Add(_workspaceContainer);

            // 2.1 Топ-бар (Кнопки переключения режимов)
            _topBarContainer = new VisualElement { name = "TopBar" };
            _topBarContainer.style.height = 40;
            _topBarContainer.style.flexDirection = FlexDirection.Row;
            _topBarContainer.style.alignItems = Align.Center;
            _topBarContainer.style.justifyContent = Justify.FlexEnd; // Справа вверху
            _topBarContainer.style.paddingRight = 10;
            _topBarContainer.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f);
            _topBarContainer.style.borderBottomWidth = 1;
            _workspaceContainer.Add(_topBarContainer);

            // 2.2 Контейнер для вывода графики модуля
            _moduleContentContainer = new VisualElement { name = "ModuleContent" };
            _moduleContentContainer.style.flexGrow = 1;
            _workspaceContainer.Add(_moduleContentContainer);

            // Рендерим стартовое состояние
            BuildSidebar();
            UpdateTopBar();
            ShowPlaceholder("Выберите редактор в левом меню для начала работы.");
        }

        // Заполнение левого меню кнопками модулей
        private void BuildSidebar()
        {
            _sidebarContainer.Clear();

            Label title = new Label("РЕДАКТОРЫ СИСТЕМ") { name = "SidebarTitle" };
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.paddingLeft = 10;
            title.style.paddingTop = 10;
            title.style.paddingBottom = 10;
            title.style.color = new Color(0.7f, 0.7f, 0.7f);
            _sidebarContainer.Add(title);

            var views = EditorViewRegistry.GetViews();
            if (views.Count == 0)
            {
                Label emptyLabel = new Label("Нет зарегистрированных окон") { name = "EmptyLabel" };
                emptyLabel.style.paddingLeft = 10;
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _sidebarContainer.Add(emptyLabel);
                return;
            }

            foreach (var viewData in views)
            {
                Button modButton = new Button(() => SelectModule(viewData)) { text = viewData.DisplayName };
                modButton.style.height = 32;
                modButton.style.unityTextAlign = TextAnchor.MiddleLeft;
                modButton.style.marginTop = 2;
                modButton.style.marginBottom = 2;
                _sidebarContainer.Add(modButton);
            }
        }

        // Логика выбора конкретного окна редактора (например, HFSM)
        private void SelectModule(EditorViewRegistry.ViewData viewData)
        {
            if (_currentActiveView != null)
            {
                _currentActiveView.OnDisable();
                _currentActiveView = null;
            }

            _selectedModuleData = viewData;
            _currentActiveView = (AssetEditorView)Activator.CreateInstance(viewData.ViewType);
            _currentActiveView.Initialize(this);

            _currentMode = SubViewMode.CreateNew; // Сбрасываем на дефолтный режим

            UpdateTopBar();
            RefreshWorkspaceContent();
        }

        // Отрисовка верхней правой панели переключения режимов
        private void UpdateTopBar()
        {
            _topBarContainer.Clear();

            if (_currentActiveView == null) return;

            // Кнопка: Создать новый экземпляр
            Button btnCreate = new Button(() => SetSubMode(SubViewMode.CreateNew)) { text = "➕ Создать новый" };
            btnCreate.style.backgroundColor = _currentMode == SubViewMode.CreateNew ? new Color(0.3f, 0.4f, 0.3f) : new Color(0.25f, 0.25f, 0.25f);
            _topBarContainer.Add(btnCreate);

            // Кнопка: Готовые ассеты (Сетка / Список)
            Button btnLibrary = new Button(() => SetSubMode(SubViewMode.Library)) { text = "🗂️ Готовые ассеты" };
            btnLibrary.style.backgroundColor = _currentMode == SubViewMode.Library ? new Color(0.3f, 0.3f, 0.4f) : new Color(0.25f, 0.25f, 0.25f);
            _topBarContainer.Add(btnLibrary);
        }

        private void SetSubMode(SubViewMode mode)
        {
            if (_currentMode == mode) return;
            _currentMode = mode;
            UpdateTopBar();
            RefreshWorkspaceContent();
        }

        // Обновление контента в зависимости от выбранного режима
        private void RefreshWorkspaceContent()
        {
            _moduleContentContainer.Clear();

            if (_currentActiveView == null) return;

            if (_currentMode == SubViewMode.CreateNew)
            {
                // Режим создания нового экземпляра: даем модулю чистый холст
                _moduleContentContainer.Add(_currentActiveView.Root);
                _currentActiveView.OpenAsset(null);
            }
            else if (_currentMode == SubViewMode.Library)
            {
                // Режим библиотеки: выводим список/сетку всех ScriptableObject данного типа в проекте
                VisualElement libraryRoot = BuildLibraryGrid(_selectedModuleData.AssetType);
                _moduleContentContainer.Add(libraryRoot);
            }
        }

        // Универсальный генератор сетки готовых ассетов
        private VisualElement BuildLibraryGrid(Type assetType)
        {
            ScrollView scrollView = new ScrollView { mode = ScrollViewMode.VerticalAndHorizontal };
            scrollView.style.flexGrow = 1;
            scrollView.style.paddingTop = 15;
            scrollView.style.paddingLeft = 15;

            // Оформляем как сетку (Wrap)
            VisualElement gridContainer = new VisualElement();
            gridContainer.style.flexDirection = FlexDirection.Row;
            gridContainer.style.flexWrap = Wrap.Wrap;
            scrollView.Add(gridContainer);

            // Ищем все ассеты этого типа в Unity проекте
            string[] guids = AssetDatabase.FindAssets($"t:{assetType.Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                // Элемент карточки ассета в сетке
                Button assetCard = new Button(() => OnAssetCardClicked(asset))
                {
                    text = asset.name
                };
                assetCard.style.width = 110;
                assetCard.style.height = 100;
                assetCard.style.marginRight = 10;
                assetCard.style.marginBottom = 10;
                assetCard.style.whiteSpace = WhiteSpace.Normal;

                gridContainer.Add(assetCard);
            }

            if (guids.Length == 0)
            {
                gridContainer.Add(new Label("Готовые ассеты данного типа не найдены.") { style = { unityFontStyleAndWeight = FontStyle.Italic, paddingTop = 20 } });
            }

            return scrollView;
        }

        // Клик по готовому ассету: переключаем режим в "Редактирование" и передаем данные ассета в View
        private void OnAssetCardClicked(ScriptableObject asset)
        {
            _currentMode = SubViewMode.CreateNew; // Переключаем вкладку визуально на "Окно редактирования"
            UpdateTopBar();

            _moduleContentContainer.Clear();
            _moduleContentContainer.Add(_currentActiveView.Root);

            _currentActiveView.OpenAsset(asset); // Модуль подгружает данные этого ассета в свой инспектор или GraphView
        }

        private void ShowPlaceholder(string text)
        {
            _moduleContentContainer.Clear();
            Label placeholder = new Label(text);
            placeholder.style.flexGrow = 1;
            placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholder.style.unityFontStyleAndWeight = FontStyle.Italic;
            placeholder.style.fontSize = 14;
            _moduleContentContainer.Add(placeholder);
        }

        private void OnDisable()
        {
            _currentActiveView?.OnDisable();
        }
    }
}