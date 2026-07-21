using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public sealed class SerializeReferenceAdvancedDropdown : AdvancedDropdown
    {
        private readonly Type _baseType;
        private readonly IReadOnlyList<Type> _types;
        private readonly Action<Type> _onTypeSelected;
        private readonly Dictionary<AdvancedDropdownItem, Type> _itemToTypeMap = new();
        private readonly Dictionary<int, string> _idToTooltipMap = new();
        private int _currentIdCounter = 1;

        public SerializeReferenceAdvancedDropdown(Type baseType, IReadOnlyList<Type> types, Action<Type> onTypeSelected, AdvancedDropdownState state)
            : base(state)
        {
            _baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _onTypeSelected = onTypeSelected ?? throw new ArgumentNullException(nameof(onTypeSelected));
            minimumSize = new Vector2(250f, 350f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Select Type");

            AdvancedDropdownItem nullItem = new AdvancedDropdownItem("Select (Empty)");
            root.AddChild(nullItem);
            _itemToTypeMap[nullItem] = null;

            var folderCache = new Dictionary<string, AdvancedDropdownItem>();
            _idToTooltipMap.Clear();
            _currentIdCounter = 1;

            foreach (Type type in _types)
            {
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

                if (string.IsNullOrEmpty(path))
                {
                    AdvancedDropdownItem item = new AdvancedDropdownItem(displayName) { icon = icon, id = itemId };
                    root.AddChild(item);
                    _itemToTypeMap[item] = type;
                    continue;
                }

                AdvancedDropdownItem currentParent = GetOrCreateFolderHierarchy(path, root, folderCache);

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

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (!_itemToTypeMap.TryGetValue(item, out Type selectedType)) return;

            ShowTypeTooltip(item);

            if (selectedType == null)
            {
                _onTypeSelected?.Invoke(null);
                return;
            }

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