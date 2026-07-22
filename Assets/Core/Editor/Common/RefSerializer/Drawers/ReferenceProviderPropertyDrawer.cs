using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Кастомный отрисовщик свойств (PropertyDrawer) для обобщенного класса <see cref="ReferenceProvider{TInterface}"/>.
    /// Настраивает поле ObjectField в инспекторе, обеспечивая строгую валидацию типов интерфейса при перетаскивании (Drag and Drop) и выборе объектов.
    /// <br/><br/>
    /// A custom property drawer for the generic <see cref="ReferenceProvider{TInterface}"/> class.
    /// Configures an ObjectField in the Inspector, ensuring strict interface type validation during drag-and-drop operations and object selection.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReferenceProvider<>))]
    public sealed class ReferenceProviderPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Создает визуальный элемент ObjectField для инспектора, автоматически инициализирует имя типа интерфейса и настраивает колбэки валидации.
        /// <br/><br/>
        /// Creates the ObjectField visual element for the Inspector, automatically initializes the interface type name, and sets up validation callbacks.
        /// </summary>
        /// <param name="property">Сериализованное свойство, представляющее экземпляр ReferenceProvider. / The serialized property representing the ReferenceProvider instance.</param>
        /// <returns>Визуальный элемент ObjectField для отображения в инспекторе. / The ObjectField visual element to display in the Inspector.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type interfaceType = fieldInfo.FieldType.GetGenericArguments()[0];
            SerializedProperty targetObjectProperty = property.FindPropertyRelative("_targetObject");
            SerializedProperty interfaceTypeNameProperty = property.FindPropertyRelative("_interfaceTypeName");

            // Если имя типа интерфейса еще не записано в ассет/компонент, инициализируем его полным системным именем
            if (string.IsNullOrEmpty(interfaceTypeNameProperty.stringValue))
            {
                interfaceTypeNameProperty.stringValue = interfaceType.AssemblyQualifiedName;
                interfaceTypeNameProperty.serializedObject.ApplyModifiedProperties();
            }

            ObjectField objectField = new ObjectField
            {
                label = property.displayName,
                objectType = typeof(UnityEngine.Object),
                allowSceneObjects = true
            };
            objectField.BindProperty(targetObjectProperty);

            // Валидация объекта непосредственно во время перетаскивания (Drag and Drop) над полем инспектора
            objectField.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0) return;
                UnityEngine.Object draggedObject = DragAndDrop.objectReferences[0];
                if (IsValidInterfaceProvider(draggedObject, interfaceType))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    evt.StopPropagation();
                }
            });

            // Валидация объекта при окончательном изменении значения поля (например, при выборе через окно поиска Unity)
            objectField.RegisterValueChangedCallback(evt =>
            {
                UnityEngine.Object newValue = evt.newValue;
                if (newValue != null && !IsValidInterfaceProvider(newValue, interfaceType))
                {
                    targetObjectProperty.objectReferenceValue = null;
                    targetObjectProperty.serializedObject.ApplyModifiedProperties();
                    Debug.LogWarning($"[ReferenceProvider] Объект '{newValue.name}' не реализует интерфейс {interfaceType.Name}!");
                }
            });

            return objectField;
        }

        /// <summary>
        /// Проверяет, реализует ли переданный объект Unity (или компоненты на его GameObject) целевой интерфейс.
        /// <br/><br/>
        /// Checks if the specified Unity object (or components on its GameObject) implements the target interface.
        /// </summary>
        /// <param name="obj">Проверяемый объект Unity. / The Unity object to evaluate.</param>
        /// <param name="interfaceType">Тип проверяемого интерфейса. / The type of the interface to validate against.</param>
        /// <returns>True, если объект или один из его компонентов реализует интерфейс; иначе false. / True if the object or one of its components implements the interface; otherwise, false.</returns>
        private bool IsValidInterfaceProvider(UnityEngine.Object obj, Type interfaceType)
        {
            if (obj == null) return true;
            if (interfaceType.IsAssignableFrom(obj.GetType())) return true;
            if (obj is GameObject gameObject)
            {
                return gameObject.GetComponent(interfaceType) != null;
            }
            if (obj is Component component)
            {
                return component.GetComponent(interfaceType) != null;
            }
            return false;
        }
    }
}