using CrystalEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    [CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
    public class SerializeReferenceSelectorDrawer : PropertyDrawer
    {
        internal const string SELECT_TEXT = "Select";
        internal const string SELECTOR_BUTTON_NAME = "selector-button";
        internal const string CONTENT_AREA_NAME = "content-area";

        private VisualElement _root;
        private VisualElement _header;
        private Button _selectorButton;
        private VisualElement _contentArea;

        private Button _openScriptButton;
        private Button _deleteButton;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();
            _header = CreateHeader(property);
            _selectorButton = CreateSelectorButton();
            _contentArea = CreateContetArea();
            _openScriptButton = CreateIconButton("d_ScriptableObject Icon", "Open C# Script Code");
            _deleteButton = CreateIconButton("d_TreeEditor.Trash", "Delete Instance");
            BuildContextMenu(property);
            _selectorButton.clicked += () => OnSelectorButtonClicked(property);
            _openScriptButton.clicked += () => OnOpenScriptClicked(property);
            _deleteButton.clicked += () => OnDeleteClicked(property);
            _header.Add(_selectorButton);
            _header.Add(_openScriptButton);
            _header.Add(_deleteButton);
            _root.Add(_header);
            _root.Add(_contentArea);

            Refresh(property);
            _root.TrackPropertyValue(property, (prop) => Refresh(prop));
            return _root;
        }

        private VisualElement CreateHeader(SerializedProperty property)
        {
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 4;

            SerializeReferenceSelectorAttribute attribute = this.attribute as SerializeReferenceSelectorAttribute;
            Label label = new Label()
            {
                text = !string.IsNullOrEmpty(attribute?.Title) ? attribute.Title : property.displayName
            };
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(label);
            return header;
        }

        private Button CreateSelectorButton()
        {
            _selectorButton = new Button()
            {
                name = SELECTOR_BUTTON_NAME,
            };
            _selectorButton.style.flexGrow = 1;
            _selectorButton.style.marginLeft = 8;
            return _selectorButton;
        }

        private VisualElement CreateContetArea()
        {
            VisualElement contetArea = new VisualElement()
            {
                name = CONTENT_AREA_NAME
            };
            // Небольшой отступ слева для вложенных полей, создающий красивую иерархию (сдвиг вправо)
            contetArea.style.marginLeft = 15;
            return contetArea;
        }

        // Универсальный генератор маленьких квадратных кнопок с иконками Unity
        private Button CreateIconButton(string iconName, string tooltip)
        {
            Button btn = new Button();
            btn.tooltip = tooltip;
            btn.style.width = 20;
            btn.style.height = 18;
            btn.style.marginLeft = 2;
            btn.style.paddingLeft = 0;
            btn.style.paddingRight = 0;

            // Загружаем родную иконку редактора Unity по ее внутреннему имени
            Texture2D icon = EditorGUIUtility.IconContent(iconName)?.image as Texture2D;
            if (icon != null)
            {
                btn.style.backgroundImage = icon;
                // Заставляем иконку аккуратно вписаться в размеры кнопки
                btn.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
            return btn;
        }

        private void BuildContextMenu(SerializedProperty property)
        {
            _selectorButton.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Cut",
                    action => EditorClipboard.Cut(property),
                    property.managedReferenceValue != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvt.menu.AppendAction("Copy",
                    action => EditorClipboard.Copy(property),
                    property.managedReferenceValue != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvt.menu.AppendAction("Paste",
                    action => EditorClipboard.Paste(property),
                    EditorClipboard.CanPaste(property) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvt.menu.AppendSeparator();

                menuEvt.menu.AppendAction("Duplicate",
                    action => EditorClipboard.Duplicate(property),
                    property.managedReferenceValue != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                menuEvt.menu.AppendAction("Delete",
                    action => OnDeleteClicked(property),
                    property.managedReferenceValue != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        // ИСПРАВЛЕНО: Убрана лишняя кнопка из аргументов, берется поле класса
        private void OnSelectorButtonClicked(SerializedProperty property)
        {
            Rect buttonRect = _selectorButton.worldBound;
            SelectorDropdownMenu dropdown = new SelectorDropdownMenu(property);
            dropdown.Show(buttonRect);
        }

        // ЛОГИКА КНОПКИ: Поиск MonoScript типа в базе ассетов и открытие его в IDE
        private void OnOpenScriptClicked(SerializedProperty property)
        {
            if (property.managedReferenceValue == null) return;

            string typeName = property.managedReferenceValue.GetType().Name;
            // Ищем скрипт в проекте по имени его класса
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeName}");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null)
                {
                    AssetDatabase.OpenAsset(script);
                    return;
                }
            }
            Debug.LogWarning($"[CrystalEngine] Could not find C# script asset for type '{typeName}'.");
        }

        // ЛОГИКА КНОПКИ: Сброс инстанса в null
        private void OnDeleteClicked(SerializedProperty property)
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void Refresh(SerializedProperty property)
        {
            _contentArea.Clear();

            if (property.managedReferenceValue != null)
            {
                _openScriptButton.style.display = DisplayStyle.Flex; // Показываем кнопку кода
                _deleteButton.style.display = DisplayStyle.Flex;     // Показываем кнопку корзины

                // ИСПРАВЛЕНО: Полностью убрали кривой обход TypeRegistry.
                // Наш форматер сам залезет в инстанс, проверит атрибуты и выдаст красивое имя типа!
                _selectorButton.text = property.GetDisplayValueName();
                _selectorButton.style.unityFontStyleAndWeight = FontStyle.Normal;

                GenerateChildProperties(property);
            }
            else
            {
                _openScriptButton.style.display = DisplayStyle.None; // Скрываем кнопку кода
                _deleteButton.style.display = DisplayStyle.None;     // Скрываем кнопку корзины

                _selectorButton.text = SELECT_TEXT;
                _selectorButton.style.unityFontStyleAndWeight = FontStyle.Italic;
            }
        }


        private void GenerateChildProperties(SerializedProperty property)
        {
            SerializedProperty propertyCopy = property.Copy();
            if (propertyCopy.NextVisible(true))
            {
                int endDepth = property.depth;
                do
                {
                    if (propertyCopy.depth <= endDepth) break;
                    PropertyField childField = new PropertyField(propertyCopy.Copy());
                    childField.Bind(property.serializedObject);
                    _contentArea.Add(childField);
                }
                while (propertyCopy.NextVisible(false));
            }
        }
    }
}
