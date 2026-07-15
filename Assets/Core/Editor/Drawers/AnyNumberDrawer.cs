using Crystal.Common;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Editor
{
    [CustomPropertyDrawer(typeof(AnyNumber))]
    public class AnyNumberDrawer : PropertyDrawer
    {
        public const string PROP_TYPE = "_type";
        public const string PROP_BYTE = "_asByte";
        public const string PROP_SBYTE = "_asSByte";
        public const string PROP_UINT16 = "_asUInt16";
        public const string PROP_INT16 = "_asInt16";
        public const string PROP_UINT32 = "_asUInt32";
        public const string PROP_INT32 = "_asInt32";
        public const string PROP_UINT64 = "_asUInt64";
        public const string PROP_INT64 = "_asInt64";
        public const string PROP_SINGLE = "_asSingle";
        public const string PROP_DOUBLE = "_asDouble";
        public const string LABEL_TEXT = "Value";
        public const string BASE_LABEL_CSS_CLASS = "unity-base-field__label";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;

            SerializedProperty typeProp = property.FindPropertyRelative(PROP_TYPE);
            SerializedProperty[] valueProps = new SerializedProperty[]
            {
                property.FindPropertyRelative(PROP_BYTE),
                property.FindPropertyRelative(PROP_SBYTE),
                property.FindPropertyRelative(PROP_UINT16),
                property.FindPropertyRelative(PROP_INT16),
                property.FindPropertyRelative(PROP_UINT32),
                property.FindPropertyRelative(PROP_INT32),
                property.FindPropertyRelative(PROP_UINT64),
                property.FindPropertyRelative(PROP_INT64),
                property.FindPropertyRelative(PROP_SINGLE),
                property.FindPropertyRelative(PROP_DOUBLE)
            };

            EnumField typeField = new EnumField((NumericType)typeProp.enumValueIndex);
            typeField.style.minWidth = new Length(35, LengthUnit.Percent);
            root.Add(typeField);

            VisualElement inputHolder = new VisualElement();
            inputHolder.style.minWidth = new Length(65, LengthUnit.Percent);
            inputHolder.style.unityTextAlign = TextAnchor.MiddleRight;

            root.Add(inputHolder);

            RebuildInterface(inputHolder, valueProps, (NumericType)typeProp.enumValueIndex);
            root.TrackPropertyValue(typeProp, (updatedProp) =>
            {
                valueProps[9].doubleValue = default;
                property.serializedObject.ApplyModifiedProperties();
                RebuildInterface(inputHolder, valueProps, (NumericType)typeProp.enumValueIndex);
            });
            return root;
        }

        private void RebuildInterface(VisualElement container, SerializedProperty[] valueProps, NumericType currentType)
        {
            container.Clear();
            int index = (int)currentType;
            if (index < 0 || index >= valueProps.Length) return;

            SerializedProperty activeProp = valueProps[index];
            bool isInteger = currentType != NumericType.Single && currentType != NumericType.Double;
            if (isInteger)
            {
                LongField intField = new LongField(LABEL_TEXT);
                intField.SetValueWithoutNotify(activeProp.longValue);
                SetLabelStyle(intField);

                intField.RegisterValueChangedCallback(evt =>
                {
                    long rawValue = evt.newValue;
                    long clamped = currentType switch
                    {
                        NumericType.Byte => Math.Clamp(rawValue, byte.MinValue, byte.MaxValue),
                        NumericType.SByte => Math.Clamp(rawValue, sbyte.MinValue, sbyte.MaxValue),
                        NumericType.UInt16 => Math.Clamp(rawValue, ushort.MinValue, ushort.MaxValue),
                        NumericType.Int16 => Math.Clamp(rawValue, short.MinValue, short.MaxValue),
                        NumericType.UInt32 => Math.Clamp(rawValue, uint.MinValue, uint.MaxValue),
                        NumericType.Int32 => Math.Clamp(rawValue, int.MinValue, int.MaxValue),
                        _ => rawValue
                    };
                    if (rawValue != clamped)
                    {
                        intField.SetValueWithoutNotify(clamped);
                    }
                    activeProp.longValue = clamped;
                    activeProp.serializedObject.ApplyModifiedProperties();
                });
                container.Add(intField);
            }
            else
            {
                DoubleField floatField = new DoubleField(LABEL_TEXT);
                floatField.SetValueWithoutNotify(activeProp.doubleValue);
                SetLabelStyle(floatField);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    double rawValue = evt.newValue;
                    double clamped = rawValue;
                    if (currentType == NumericType.Single)
                    {
                        clamped = Math.Clamp(rawValue, float.MinValue, float.MaxValue);
                    }
                    if (rawValue != clamped)
                    {
                        floatField.SetValueWithoutNotify(clamped);
                    }
                    activeProp.doubleValue = clamped;
                    activeProp.serializedObject.ApplyModifiedProperties();
                });
                container.Add(floatField);
            }
        }

        private void SetLabelStyle(VisualElement field)
        {
            Label nativeLabel = field.Q<Label>(className: BASE_LABEL_CSS_CLASS);
            if (nativeLabel != null)
            {
                nativeLabel.style.flexGrow = 0.05f;
                nativeLabel.style.width = new Length(5, LengthUnit.Percent);
                nativeLabel.style.minWidth = new Length(5, LengthUnit.Percent);
                nativeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
        }
    }
}