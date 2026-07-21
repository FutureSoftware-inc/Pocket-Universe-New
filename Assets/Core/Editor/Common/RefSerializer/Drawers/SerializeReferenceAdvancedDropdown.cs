using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public sealed class SerializeReferenceAdvancedDropdown : AdvancedDropdown
    {
        private readonly Type _baseType;
        private readonly IReadOnlyList<Type> _allProjectTypes;
        private readonly Action<Type> _onTypeSelected;
        private readonly Dictionary<AdvancedDropdownItem, Type> _itemToTypeMap = new();

        public SerializeReferenceAdvancedDropdown(Type baseType, IReadOnlyList<Type> types, Action<Type> onTypeSelected, AdvancedDropdownState state)
            : base(state)
        {
            _baseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            _allProjectTypes = types ?? throw new ArgumentNullException(nameof(types));
            _onTypeSelected = onTypeSelected ?? throw new ArgumentNullException(nameof(onTypeSelected));
            minimumSize = new Vector2(250f, 300f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Select Type");
            AdvancedDropdownItem nullItem = new AdvancedDropdownItem("Select (Empty)");
            root.AddChild(nullItem);
            _itemToTypeMap[nullItem] = null;
            Dictionary<string, AdvancedDropdownItem> folderCache = new Dictionary<string, AdvancedDropdownItem>();
            foreach (Type type in _allProjectTypes)
            {
                SubclassPathAttribute pathAttribute = type.GetCustomAttributes(typeof(SubclassPathAttribute), false)
                    .FirstOrDefault() as SubclassPathAttribute;
                if (pathAttribute == null || string.IsNullOrEmpty(pathAttribute.Path))
                {
                    string displayName = GetTypeName(type);
                    AdvancedDropdownItem item = new AdvancedDropdownItem(displayName);
                    root.AddChild(item);
                    _itemToTypeMap[item] = type;
                    continue;
                }
                string[] sections = pathAttribute.Path.Split('/');
                AdvancedDropdownItem currentParent = root;
                string currentFullPath = string.Empty;
                for (int i = 0; i < sections.Length - 1; i++)
                {
                    string section = sections[i];
                    currentFullPath = string.IsNullOrEmpty(currentFullPath) ? section : $"{currentFullPath}/{section}";
                    if (!folderCache.TryGetValue(currentFullPath, out AdvancedDropdownItem folderItem))
                    {
                        folderItem = new AdvancedDropdownItem(section);
                        currentParent.AddChild(folderItem);
                        folderCache[currentFullPath] = folderItem;
                    }
                    currentParent = folderItem;
                }
                string leafName = sections.Last();
                if (type.IsGenericTypeDefinition && _baseType.IsGenericType)
                {
                    string contextArgs = string.Join(", ", _baseType.GetGenericArguments().Select(t => t.Name));
                    leafName = $"{leafName}<{contextArgs}> (Auto)";
                }
                AdvancedDropdownItem finalItem = new AdvancedDropdownItem(leafName);
                currentParent.AddChild(finalItem);
                _itemToTypeMap[finalItem] = type;
            }
            return root;
        }


        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (!_itemToTypeMap.TryGetValue(item, out Type selectedType))
            {
                return;
            }
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
                    Debug.LogError($"[AdvancedDropdown] Не удалось автоматически собрать дженерик тип {selectedType.Name} " +
                                   $"для контекста {_baseType.Name}. Ошибка: {exception.Message}");
                    return;
                }
            }
            _onTypeSelected?.Invoke(selectedType);
        }

        private string GetTypeName(Type type)
        {
            if (!type.IsGenericTypeDefinition)
            {
                return type.Name;
            }
            string cleanName = type.Name.Split('`')[0];
            if (_baseType.IsGenericType)
            {
                string contextArgs = string.Join(", ", _baseType.GetGenericArguments().Select(t => t.Name));
                return $"{cleanName}<{contextArgs}> (Auto)";
            }
            return $"{cleanName}<>";
        }
    }
}
