using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    [CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
    public class SerializeReferenceSelectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return new PropertyField(property);
            }

            VisualElement container = new VisualElement();
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            container.Add(root);

            VisualElement selector = new VisualElement();
            IReadOnlyList<Type> implementations = TypeRegistry.GetImplementations(GetBaseType());
            List<Type> availableTypes = new List<Type>() { null };
            availableTypes.AddRange(implementations);
            Type currentValue = property.managedReferenceValue?.GetType();
            PopupField<Type> field = new PopupField<Type>(
                label: "Type",
                choices: availableTypes,
                defaultValue: currentValue ?? availableTypes[0],
                formatSelectedValueCallback: type => type != null ? type.Name : "Null",
                formatListItemCallback: type => type != null ? type.Name : "Null"
            );
            selector.Add(field);
            container.Add(selector);

            VisualElement fields = new VisualElement();
            field.RegisterValueChangedCallback(@event =>
            {
                RegistryValueChange(@event, property, fields);
            });
            container.Add(fields);
            if (currentValue != null)
            {
                PropertyField initialField = new PropertyField(property);
                initialField.Bind(property.serializedObject);
                fields.Add(initialField);
            }
            return container;
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

        private void RegistryValueChange(ChangeEvent<Type> @event, SerializedProperty property, VisualElement fields)
        {
            Type newValue = @event.newValue;
            property.managedReferenceValue = newValue != null ? ReferenceFactory.CreateInstance(newValue) : null;
            property.serializedObject.ApplyModifiedProperties();
            fields.Clear();
            PropertyField field = new PropertyField(property);
            field.Bind(property.serializedObject);
            fields.Add(field);
        }
    }
}