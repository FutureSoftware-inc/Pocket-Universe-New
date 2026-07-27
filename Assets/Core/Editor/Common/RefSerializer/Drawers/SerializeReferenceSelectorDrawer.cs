using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Кастомный отрисовщик свойств (PropertyDrawer) для полей с атрибутом <see cref="SerializeReferenceSelectorAttribute"/>.
    /// Реализует удобный визуальный интерфейс для работы с полиморфной сериализацией [SerializeReference].
    /// <br/><br/>
    /// A custom property drawer for fields marked with <see cref="SerializeReferenceSelectorAttribute"/>.
    /// Implements a user-friendly visual interface for working with [SerializeReference] polymorphic serialization.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
    public class SerializeReferenceSelectorDrawer : PropertyDrawer
    {
        private readonly AdvancedDropdownState _dropdownState = new();
        private Type _cachedBaseType;
        private string _cachedBaseTypeName;

        /// <summary>
        /// Создает и настраивает структуру визуальных элементов UI Toolkit для отображения полиморфного поля в инспекторе.
        /// <br/><br/>
        /// Creates and configures the visual element hierarchy using UI Toolkit to display the polymorphic field in the Inspector.
        /// </summary>
        /// <param name="property">Сериализованное управляемое свойство типа ManagedReference.<br/><br/>The serialized managed reference property.</param>
        /// <returns>Корневой визуальный элемент интерфейса свойства.<br/><br/>The root visual element of the property GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Если поле не использует [SerializeReference] (ManagedReference), рисуем его стандартно
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

            // Регистрация событий нажатия кнопок и контекстного меню
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

        /// <summary>
        /// Создает заголовочную область элемента управления (плашка с названием свойства).
        /// <br/><br/>
        /// Creates the header section of the control (a panel displaying the property name).
        /// </summary>
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

        /// <summary>
        /// Создает кнопку-селектор, отображающую понятное имя текущего выбранного типа.
        /// <br/><br/>
        /// Creates the selector button displaying the user-friendly name of the currently selected type.
        /// </summary>
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

        /// <summary>
        /// Создает кнопку сброса значения (установки в null) в форме крестика.
        /// <br/><br/>
        /// Creates the reset button (setting value to null) represented as a cross icon.
        /// </summary>
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

        /// <summary>
        /// Создает кнопку со значком C# скрипта для мгновенного открытия файла исходного кода класса.
        /// <br/><br/>
        /// Creates the button with a C# script icon for instantly opening the class source code file.
        /// </summary>
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

        /// <summary>
        /// Создает контейнер-подложку, в который будут динамически отрисовываться внутренние поля выбранного класса.
        /// <br/><br/>
        /// Creates a container layout where internal fields of the selected class will be dynamically drawn.
        /// </summary>
        private VisualElement CreateFieldsContainer()
        {
            VisualElement fieldsContainer = new VisualElement();
            fieldsContainer.style.marginLeft = 15f;

            return fieldsContainer;
        }

        /// <summary>
        /// Инициализирует и открывает кастомное расширенное выпадающее меню со списком всех доступных реализаций базового типа.
        /// <br/><br/>
        /// Initializes and opens the custom advanced dropdown menu displaying all available implementations of the base type.
        /// </summary>
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
        /// <summary>
        /// Формирует и отображает контекстное меню (Copy/Paste) при нажатии правой кнопкой мыши по селектору типа.
        /// <br/><br/>
        /// Constructs and displays the context menu (Copy/Paste) upon right-clicking the type selector.
        /// </summary>
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

            if (SelectorCopyPaste.CanPaste(_cachedBaseType))
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

        /// <summary>
        /// Перерисовывает и связывает (Bind) внутренние сериализуемые поля выбранного класса внутри контейнера.
        /// <br/><br/>
        /// Redraws and binds internal serializable fields of the selected class inside the target container.
        /// </summary>
        private void RefreshField(SerializedProperty property, VisualElement target)
        {
            PropertyField initialField = new PropertyField(property, _cachedBaseTypeName);

            initialField.RegisterCallback<AttachToPanelEvent>(@event =>
            {
                ExpandFoldout(initialField);
            });
            initialField.Bind(property.serializedObject);
            target.Add(initialField);
        }

        /// <summary>
        /// Определяет базовый тип поля через рефлексию, корректно обрабатывая массивы и обобщенные списки List.
        /// <br/><br/>
        /// Determines the base type of the field via reflection, correctly handling arrays and generic List types.
        /// </summary>
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

        /// <summary>
        /// Регистрирует изменение типа данных, осуществляет миграцию полей (DataMigrator) со старого объекта на новый и обновляет UI элементы.
        /// <br/><br/>
        /// Registers the data type change, performs property migration (DataMigrator) from the old object to the new one, and updates UI elements.
        /// </summary>
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

        /// <summary>
        /// Автоматически разворачивает элемент Foldout при его появлении в инспекторе для мгновенного доступа к полям.
        /// <br/><br/>
        /// Automatically expands the Foldout element upon its appearance in the Inspector for instant field access.
        /// </summary>
        private void ExpandFoldout(PropertyField field)
        {
            Foldout foldout = field.Q<Foldout>();
            if (foldout != null)
            {
                foldout.value = true;
            }
        }

        /// <summary>
        /// Находит исходный C# файл текущего класса через AssetDatabase и открывает его во внешнем IDE-редакторе.
        /// <br/><br/>
        /// Finds the source C# file of the current class using AssetDatabase and opens it inside an external IDE editor.
        /// </summary>
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

        /// <summary>
        /// Возвращает чистое, отформатированное имя типа, удаляя технические мета-символы дженериков (`1) и подставляя аргументы контекста.
        /// <br/><br/>
        /// Returns a clean, formatted type name, stripping technical generic meta-characters (`1) and appending context arguments.
        /// </summary>
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

        /// <summary>
        /// Возвращает чистое имя базового типа без символов обобщения.
        /// <br/><br/>
        /// Returns the clean name of the base type without generic qualifiers.
        /// </summary>
        private string GetBaseTypeName(Type baseType)
        {
            if (baseType == null)
                return "Null";
            return baseType.IsGenericType ? baseType.Name.Split('`')[0] : baseType.Name;
        }
    }
}