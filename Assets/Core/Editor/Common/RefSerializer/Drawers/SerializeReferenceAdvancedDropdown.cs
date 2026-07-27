using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Кастомное иерархическое выпадающее меню (AdvancedDropdown), отображающее список доступных подклассов и интерфейсов.
    /// Автоматически извлекает метаданные типов (пути, иконки, подсказки, имена) и формирует из них древовидную структуру папок.
    /// <br/><br/>
    /// A custom hierarchical dropdown menu (AdvancedDropdown) that displays a list of available subclasses and interfaces.
    /// Automatically extracts type metadata (paths, icons, tooltips, display names) to generate a tree-structured folder hierarchy.
    /// </summary>
    public sealed class SerializeReferenceAdvancedDropdown : AdvancedDropdown
    {
        private readonly Type _baseType;
        private readonly IReadOnlyList<Type> _types;
        private readonly Action<Type> _onTypeSelected;
        private readonly Dictionary<AdvancedDropdownItem, Type> _itemToTypeMap = new();
        private readonly Dictionary<int, string> _idToTooltipMap = new();
        private int _currentIdCounter = 1;

        /// <summary>
        /// Инициализирует новый экземпляр выпадающего списка с указанием базового типа, списка реализаций и обратного вызова при выборе.
        /// <br/><br/>
        /// Initializes a new instance of the advanced dropdown with the specified base type, list of implementations, and selection callback.
        /// </summary>
        /// <param name="baseType">Базовый тип или интерфейс сериализуемого поля.<br/><br/>The base type or interface of the serializable field.</param>
        /// <param name="types">Список всех доступных типов-реализаций для отображения.<br/><br/>The list of all available implementation types to display.</param>
        /// <param name="onTypeSelected">Делегат, вызываемый при выборе элемента из списка.<br/><br/>The delegate invoked when an item is selected from the list.</param>
        /// <param name="state">Текущее сериализуемое состояние окна выпадающего списка.<br/><br/>The current serialized state of the dropdown window.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если один из обязательных параметров равен null.<br/><br/>Thrown when one of the specified parameters is null.</exception>
        public SerializeReferenceAdvancedDropdown(Type baseType, IReadOnlyList<Type> types, Action<Type> onTypeSelected, AdvancedDropdownState state)
            : base(state)
        {
            _baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _onTypeSelected = onTypeSelected ?? throw new ArgumentNullException(nameof(onTypeSelected));
            minimumSize = new Vector2(250f, 350f);
        }

        /// <summary>
        /// Строит корневой элемент дерева и наполняет выпадающий список дочерними элементами на основе метаданных из реестра типов.
        /// <br/><br/>
        /// Builds the root element of the tree and populates the dropdown menu with child items based on metadata from the type registry.
        /// </summary>
        /// <returns>Корневой элемент иерархии выпадающего списка.<br/><br/>The root item of the dropdown hierarchy.</returns>
        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Select Type");

            // Создаем и регистрируем пустой элемент для возможности сброса поля в null
            AdvancedDropdownItem nullItem = new AdvancedDropdownItem("Select (Empty)");
            root.AddChild(nullItem);
            _itemToTypeMap[nullItem] = null;

            var folderCache = new Dictionary<string, AdvancedDropdownItem>();
            _idToTooltipMap.Clear();
            _currentIdCounter = 1;

            foreach (Type type in _types)
            {
                // Извлекаем расширения метаданных через реестр типов
                var pathMeta = TypeRegistry.GetExtension<PathMetadataExtension>(type, _baseType);
                var nameMeta = TypeRegistry.GetExtension<DisplayNameMetadataExtension>(type, _baseType);
                var iconMeta = TypeRegistry.GetExtension<IconMetadataExtension>(type, _baseType);
                var tooltipMeta = TypeRegistry.GetExtension<TooltipMetadataExtension>(type, _baseType);

                int itemId = 0;
                if (tooltipMeta != null && !string.IsNullOrEmpty(tooltipMeta.Tooltip))
                {
                    itemId = _currentIdCounter++;
                    _idToTooltipMap[itemId] = tooltipMeta.Tooltip;
                }

                Texture2D icon = iconMeta?.Icon;
                string displayName = nameMeta?.DisplayName ?? type.Name;
                string path = pathMeta?.Path;

                // Если путь не задан (нет атрибута SubclassPath), добавляем элемент прямо в корень
                if (string.IsNullOrEmpty(path))
                {
                    AdvancedDropdownItem item = new AdvancedDropdownItem(displayName) { icon = icon, id = itemId };
                    root.AddChild(item);
                    _itemToTypeMap[item] = type;
                    continue;
                }

                AdvancedDropdownItem currentParent = GetOrCreateFolderHierarchy(path, root, folderCache);

                // Помечаем открытые дженерики, которые закроются автоматически на основе базового типа
                if (type.IsGenericTypeDefinition && _baseType.IsGenericType)
                {
                    displayName = $"{displayName} (Auto)";
                }

                AdvancedDropdownItem finalItem = new AdvancedDropdownItem(displayName) { icon = icon, id = itemId };
                currentParent.AddChild(finalItem);
                _itemToTypeMap[finalItem] = type;
            }

            return root;
        }
        /// <summary>
        /// Вызывается при выборе элемента пользователем. 
        /// Обрабатывает извлечение типа, автоматическое закрытие дженериков под контекст и генерирует событие выбора.
        /// <br/><br/>
        /// Triggered when an item is selected by the user. 
        /// Processes type extraction, automatic generic closure for the context, and invokes the selection event.
        /// </summary>
        /// <param name="item">Выбранный элемент выпадающего списка.<br/><br/>The selected dropdown item.</param>
        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (!_itemToTypeMap.TryGetValue(item, out Type selectedType)) return;

            ShowTypeTooltip(item);

            // Если выбран пустой элемент, сбрасываем ссылку в null
            if (selectedType == null)
            {
                _onTypeSelected?.Invoke(null);
                return;
            }

            // Автоматически закрываем открытый generic-тип (например, MyNode<>) аргументами базового типа (например, IState<MyContext>)
            if (selectedType.IsGenericTypeDefinition && _baseType.IsGenericType)
            {
                try
                {
                    Type[] genericArguments = _baseType.GetGenericArguments();
                    Type closedType = selectedType.MakeGenericType(genericArguments);
                    _onTypeSelected?.Invoke(closedType);
                    return;
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[AdvancedDropdown] Не удалось автоматически собрать дженерик тип {selectedType.Name} для контекста {_baseType.Name}. Ошибка: {exception.Message}");
                    return;
                }
            }

            _onTypeSelected?.Invoke(selectedType);
        }

        /// <summary>
        /// Рекурсивно парсит переданную строку пути и строит иерархию папок в дереве выпадающего списка, используя кэш.
        /// <br/><br/>
        /// Recursively parses the provided path string and constructs a folder hierarchy within the dropdown tree using a cache.
        /// </summary>
        private AdvancedDropdownItem GetOrCreateFolderHierarchy(string path, AdvancedDropdownItem root, Dictionary<string, AdvancedDropdownItem> folderCache)
        {
            string[] sections = path.Split('/');
            AdvancedDropdownItem currentParent = root;
            string currentFullPath = string.Empty;

            for (int i = 0; i < sections.Length - 1; i++)
            {
                string section = sections[i];
                currentFullPath = string.IsNullOrEmpty(currentFullPath) ? section : $"{currentFullPath}/{section}";

                if (!folderCache.TryGetValue(currentFullPath, out AdvancedDropdownItem folderItem))
                {
                    var folderContent = EditorGUIUtility.IconContent("Folder Icon");
                    folderItem = new AdvancedDropdownItem(section) { icon = folderContent?.image as Texture2D };
                    currentParent.AddChild(folderItem);
                    folderCache[currentFullPath] = folderItem;
                }

                currentParent = folderItem;
            }

            return currentParent;
        }

        /// <summary>
        /// Находит текстовое описание подсказки по идентификатору элемента и выводит её в виде всплывающего уведомления Unity (Notification).
        /// <br/><br/>
        /// Resolves the tooltip text by the item ID and displays it as a floating Unity editor notification.
        /// </summary>
        private void ShowTypeTooltip(AdvancedDropdownItem item)
        {
            if (item != null && _idToTooltipMap.TryGetValue(item.id, out string tooltipText))
            {
                var window = EditorWindow.focusedWindow;
                if (window != null)
                {
                    window.ShowNotification(new GUIContent(tooltipText), 1.5f);
                }
            }
        }
    }
}