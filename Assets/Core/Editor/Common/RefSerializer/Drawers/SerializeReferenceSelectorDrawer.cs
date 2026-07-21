using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    [CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
    public class SerializeReferenceSelectorDrawer : PropertyDrawer
    {
        private readonly AdvancedDropdownState _dropdownState = new();
        private Type _cachedBaseType;
        private string _cachedBaseTypeName;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return new PropertyField(property);
            }

            if (_cachedBaseType == null)
            {
                _cachedBaseType = GetBaseType();
                _cachedBaseTypeName = GetBaseTypeName(_cachedBaseType);
            }

            VisualElement root = new VisualElement();

            VisualElement header = CreateHeader(property);
            root.Add(header);

            Type currentValue = property.managedReferenceValue?.GetType();
            Button selectorButton = CreateSelectorButton(currentValue);
            header.Add(selectorButton);

            Button resetButton = CreateResetButton(currentValue);
            header.Add(resetButton);

            Button openScriptButton = CreateOpenScriptButton(currentValue);
            header.Add(openScriptButton);

            VisualElement fieldsContainer = CreateFieldsContainer();
            root.Add(fieldsContainer);

            selectorButton.clicked += () => ShowDropdownMenu(property, _cachedBaseType, selectorButton, fieldsContainer, resetButton, openScriptButton);
            resetButton.clicked += () => RegistryValueChange(null, property, fieldsContainer, selectorButton, resetButton, openScriptButton);
            openScriptButton.clicked += () => OpenScriptFile(property);

            selectorButton.RegisterCallback<ContextClickEvent>(evt => ShowContextMenu(evt, property, fieldsContainer, selectorButton, resetButton, openScriptButton));

            if (currentValue != null)
            {
                RefreshField(property, fieldsContainer);
            }

            return root;
        }

        private VisualElement CreateHeader(SerializedProperty property)
        {
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 2f;

            Label propertyLabel = new Label(property.displayName);
            propertyLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            header.Add(propertyLabel);

            return header;
        }

        private Button CreateSelectorButton(Type currentValue)
        {
            Button selectorButton = new Button
            {
                text = GetCleanTypeName(currentValue, _cachedBaseType)
            };
            selectorButton.style.flexGrow = 1f;
            selectorButton.style.marginLeft = 4f;

            return selectorButton;
        }

        private Button CreateResetButton(Type currentValue)
        {
            Button resetButton = new Button { text = "✕" };
            resetButton.style.width = 20f;
            resetButton.style.height = 18f;
            resetButton.style.marginLeft = 2f;
            resetButton.style.paddingLeft = 0f;
            resetButton.style.paddingRight = 0f;
            resetButton.style.color = new Color(0.7f, 0.3f, 0.3f);
            resetButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            resetButton.style.display = currentValue != null ? DisplayStyle.Flex : DisplayStyle.None;

            return resetButton;
        }

        private Button CreateOpenScriptButton(Type currentValue)
        {
            Button openScriptButton = new Button();
            openScriptButton.style.width = 20f;
            openScriptButton.style.height = 18f;
            openScriptButton.style.marginLeft = 2f;
            openScriptButton.style.paddingLeft = 0f;
            openScriptButton.style.paddingRight = 0f;

            Texture2D scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            if (scriptIcon != null)
            {
                openScriptButton.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                openScriptButton.style.backgroundImage = scriptIcon;
            }
            else
            {
                openScriptButton.text = "#";
            }

            openScriptButton.style.display = currentValue != null ? DisplayStyle.Flex : DisplayStyle.None;

            return openScriptButton;
        }

        private VisualElement CreateFieldsContainer()
        {
            VisualElement fieldsContainer = new VisualElement();
            fieldsContainer.style.marginLeft = 15f;

            return fieldsContainer;
        }

        private void ShowDropdownMenu(SerializedProperty property, Type baseType, Button selectorButton, VisualElement fieldsContainer, Button resetButton, Button openScriptButton)
        {
            IReadOnlyList<Type> implementations = TypeRegistry.GetImplementations(baseType);
            Rect buttonRect = selectorButton.worldBound;

            var dropdown = new SerializeReferenceAdvancedDropdown(baseType, implementations, selectedType =>
            {
                RegistryValueChange(selectedType, property, fieldsContainer, selectorButton, resetButton, openScriptButton);
            }, _dropdownState);

            dropdown.Show(buttonRect);
        }

        private void ShowContextMenu(ContextClickEvent evt, SerializedProperty property, VisualElement fieldsContainer, Button selectorButton, Button resetButton, Button openScriptButton)
        {
            GenericMenu menu = new GenericMenu();

            if (property.managedReferenceValue != null)
            {
                menu.AddItem(new GUIContent("Copy"), false, () => SelectorCopyPaste.Copy(property.managedReferenceValue));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Copy"));
            }

            if (SelectorCopyPaste.CanPaste())
            {
                menu.AddItem(new GUIContent("Paste"), false, () =>
                {
                    object pastedObject = SelectorCopyPaste.Paste();
                    if (pastedObject != null)
                    {
                        RegistryValueChange(pastedObject.GetType(), property, fieldsContainer, selectorButton, resetButton, openScriptButton);
                        property.managedReferenceValue = pastedObject;
                        property.serializedObject.ApplyModifiedProperties();
                        fieldsContainer.Clear();
                        RefreshField(property, fieldsContainer);
                    }
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste"));
            }

            menu.ShowAsContext();
            evt.StopPropagation();
        }

        private void RefreshField(SerializedProperty property, VisualElement target)
        {
            PropertyField initialField = new PropertyField(property, _cachedBaseTypeName);
            initialField.RegisterCallback<GeometryChangedEvent>(@event =>
            {
                ExpandFoldout(initialField);
            });
            initialField.Bind(property.serializedObject);
            target.Add(initialField);
        }

        private Type GetBaseType()
        {
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }
            return fieldType;
        }

        private void RegistryValueChange(Type newValue, SerializedProperty property, VisualElement fieldsContainer, Button selectorButton, Button resetButton, Button openScriptButton)
        {
            Undo.RecordObject(property.serializedObject.targetObject, "Change SerializeReference Type");

            object oldObject = property.managedReferenceValue;
            object newObject = newValue != null ? ReferenceFactory.CreateInstance(newValue) : null;

            if (oldObject != null && newObject != null)
            {
                DataMigrator.MigrateData(oldObject, newObject);
            }

            property.managedReferenceValue = newObject;
            property.serializedObject.ApplyModifiedProperties();

            selectorButton.text = GetCleanTypeName(newValue, _cachedBaseType);
            fieldsContainer.Clear();

            bool hasValue = newValue != null;
            resetButton.style.display = hasValue ? DisplayStyle.Flex : DisplayStyle.None;
            openScriptButton.style.display = hasValue ? DisplayStyle.Flex : DisplayStyle.None;

            if (hasValue)
            {
                RefreshField(property, fieldsContainer);
            }
        }

        private void ExpandFoldout(PropertyField field)
        {
            Foldout foldout = field.Q<Foldout>();
            if (foldout != null)
            {
                foldout.value = true;
            }
        }

        private void OpenScriptFile(SerializedProperty property)
        {
            object managedObject = property.managedReferenceValue;
            if (managedObject == null) return;

            Type type = managedObject.GetType();
            if (type.IsGenericType) { type = type.GetGenericTypeDefinition(); }
            string searchName = type.Name.Contains('`') ? type.Name.Split('`')[0] : type.Name;
            string[] guids = AssetDatabase.FindAssets($"{searchName} t:MonoScript");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning($"[Selector] Не удалось найти C# файл для класса: {searchName}");
                return;
            }
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (script != null && script.GetClass() == type)
                {
                    AssetDatabase.OpenAsset(script);
                    return;
                }
            }
            string backupPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            MonoScript backupScript = AssetDatabase.LoadAssetAtPath<MonoScript>(backupPath);
            if (backupScript != null)
            {
                AssetDatabase.OpenAsset(backupScript);
            }
        }
        private string GetCleanTypeName(Type type, Type baseType)
        {
            if (type == null)
                return "Select";
            if (!type.IsGenericType)
                return type.Name;
            string cleanName = type.Name.Split('`')[0];
            if (baseType.IsGenericType)
            {
                string contextArgs = string.Join(", ", baseType.GetGenericArguments().Select(t => t.Name));
                return $"{cleanName}<{contextArgs}>";
            }
            return $"{cleanName}<>";
        }

        private string GetBaseTypeName(Type baseType)
        {
            if (baseType == null)
                return "Null";
            return baseType.IsGenericType ? baseType.Name.Split('`')[0] : baseType.Name; }
    }
}