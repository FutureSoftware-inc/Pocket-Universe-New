using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrystalEngineEditor
{
    public sealed class SelectorDropdownMenu
    {
        private readonly SerializedProperty _property;
        private readonly Type _baseFieldType;

        public SelectorDropdownMenu(SerializedProperty property)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _baseFieldType = property.propertyType == SerializedPropertyType.ManagedReference
                ? property.GetFieldType()
                : null;
        }

        public void Show(Rect buttonRect)
        {
            if (_baseFieldType == null)
            {
                Debug.LogWarning("[CrystalEngine] Dropdown menu can only be opened for ManagedReference fields.");
                return;
            }
            IReadOnlyList<TypeMetadata> flatTypes = TypeRegistry.GetImplementations(_baseFieldType);
            if (flatTypes == null || flatTypes.Count == 0)
            {
                Debug.LogWarning($"[CrystalEngine] No valid implementations found for type: {_baseFieldType.FullName}");
                return;
            }
            TypePathNode menuTree = TypePathNode.BuildTree(flatTypes, "Select Type");
            GenericMenu menu = new();
            bool isNull = _property.managedReferenceValue == null;
            menu.AddItem(new GUIContent("None"), isNull, () =>
            {
                _property.managedReferenceValue = null;
                _property.serializedObject.ApplyModifiedProperties();
            });
            menu.AddSeparator(string.Empty);
            PopulateMenu(menu, menuTree, string.Empty);
            menu.DropDown(buttonRect);
        }

        private void PopulateMenu(GenericMenu menu, TypePathNode currentNode, string currentPath)
        {
            foreach (KeyValuePair<string, TypePathNode> keyValuePair in currentNode.SubNodes)
            {
                string nodeName = keyValuePair.Key;
                TypePathNode childNode = keyValuePair.Value;
                string nextPath = string.IsNullOrEmpty(currentPath) ? nodeName : $"{currentPath}/{nodeName}";
                if (childNode.IsLeaf)
                {
                    Type selectedType = childNode.Metadata.Type;
                    bool isCurrent = _property.managedReferenceValue != null && _property.managedReferenceValue.GetType() == selectedType;
                    string finalMenuPath = nextPath;
                    if (selectedType.IsGenericTypeDefinition && _baseFieldType.IsGenericType)
                    {
                        Type[] genericArgumants =_baseFieldType.GetGenericArguments();
                        Type closedType = selectedType.MakeGenericType(genericArgumants);
                        finalMenuPath = string.IsNullOrEmpty(currentPath) ? closedType.GetGenericName() : $"{currentPath}/{closedType.GetGenericName()}";
                    }
                    menu.AddItem(new GUIContent(finalMenuPath), isCurrent, () =>
                    {
                        try
                        {
                            object instance;

                            // ВСЕГДА проверяем, является ли выбранный класс открытым дженериком (как NumericCondition<T>)
                            if (selectedType.IsGenericTypeDefinition)
                            {
                                // Извлекаем аргументы из базового типа поля (например, из Condition<UnitTest> достаем UnitTest)
                                Type[] genericArguments = _baseFieldType.GetGenericArguments();

                                // Если базовое поле не дженерик (например, это просто массив Element), 
                                // то поднимаемся выше по иерархии к родителю выбранного типа, чтобы найти аргумент контекста
                                if (genericArguments.Length == 0 && selectedType.BaseType != null && selectedType.BaseType.IsGenericType)
                                {
                                    genericArguments = selectedType.BaseType.GetGenericArguments();
                                }

                                // Конструируем полноценный закрытый тип: NumericCondition<UnitTest>
                                Type closedType = selectedType.MakeGenericType(genericArguments);

                                // Создаем готовый, закрытый объект!
                                instance = Activator.CreateInstance(closedType);
                            }
                            else
                            {
                                // Обычный класс без дженериков
                                instance = Activator.CreateInstance(selectedType);
                            }

                            _property.managedReferenceValue = instance;
                            _property.serializedObject.ApplyModifiedProperties();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CrystalEngine] Failed to create closed generic instance. Exception: {ex.Message}");
                        }
                    });

                }
                else
                {
                    PopulateMenu(menu, childNode, nextPath);
                }
            }
        }
    }
}