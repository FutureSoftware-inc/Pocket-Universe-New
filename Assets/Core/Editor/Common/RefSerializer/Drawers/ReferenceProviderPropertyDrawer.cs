using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CrystalEngine;

namespace CrystalEditor
{
    [CustomPropertyDrawer(typeof(ReferenceProvider<>))]
    public sealed class ReferenceProviderPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type interfaceType = fieldInfo.FieldType.GetGenericArguments()[0];
            SerializedProperty targetObjectProperty = property.FindPropertyRelative("_targetObject");
            SerializedProperty interfaceTypeNameProperty = property.FindPropertyRelative("_interfaceTypeName");
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