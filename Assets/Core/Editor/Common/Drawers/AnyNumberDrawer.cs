using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using CrystalEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Кастомный отрисовщик свойств (PropertyDrawer) для универсальной числовой структуры <see cref="AnyNumber"/>.
    /// Использует UI Toolkit для динамического перестроения интерфейса инспектора в зависимости от выбранного типа числа.
    /// <br/><br/>
    /// A custom property drawer for the universal numeric structure <see cref="AnyNumber"/>.
    /// Uses UI Toolkit to dynamically rebuild the inspector interface based on the selected numeric type.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnyNumber))]
    public class AnyNumberDrawer : PropertyDrawer
    {
        internal const string PROP_TYPE = "_currentType";
        internal const string PROP_BYTE = "_asByte";
        internal const string PROP_SBYTE = "_asSByte";
        internal const string PROP_UINT16 = "_asUInt16";
        internal const string PROP_INT16 = "_asInt16";
        internal const string PROP_UINT32 = "_asUInt32";
        internal const string PROP_INT32 = "_asInt32";
        internal const string PROP_UINT64 = "_asUInt64";
        internal const string PROP_INT64 = "_asInt64";
        internal const string PROP_SINGLE = "_asSingle";
        internal const string PROP_DOUBLE = "_asDouble";
        internal const string VALUE_LABEL_TEXT = "Value";
        internal const string TYPE_LABEL_TEXT = "Type of numeric variable";

        /// <summary>
        /// Создает и настраивает визуальные элементы (UI Toolkit) для отображения структуры в инспекторе Unity.
        /// <br/><br/>
        /// Creates and configures visual elements (UI Toolkit) for displaying the structure in the Unity Inspector.
        /// </summary>
        /// <param name="property">Сериализованное свойство, представляющее структуру AnyNumber. / The serialized property representing the AnyNumber structure.</param>
        /// <returns>Корневой визуальный элемент интерфейса свойства. / The root visual element of the property GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;

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

            PropertyField typeField = new PropertyField(typeProp, TYPE_LABEL_TEXT);
            root.Add(typeField);

            VisualElement inputHolder = new VisualElement();
            root.Add(inputHolder);

            RebuildInterface(inputHolder, valueProps, (NumericType)typeProp.enumValueIndex);

            // Отслеживаем изменение типа числовой переменной для очистки памяти и перерисовки ввода
            root.TrackPropertyValue(typeProp, (updatedProp) =>
            {
                valueProps[9].doubleValue = default;
                property.serializedObject.ApplyModifiedProperties();
                RebuildInterface(inputHolder, valueProps, (NumericType)typeProp.enumValueIndex);
            });
            return root;
        }

        /// <summary>
        /// Перестраивает интерфейс ввода, создавая LongField или DoubleField в зависимости от выбранного числового типа,
        /// а также настраивает валидацию и ограничение (Math.Clamp) вводимых данных под конкретные границы примитивов.
        /// <br/><br/>
        /// Rebuilds the input interface by creating a LongField or DoubleField depending on the selected numeric type,
        /// and configures data validation and clamping (Math.Clamp) within specific primitive bounds.
        /// </summary>
        /// <param name="container">Контейнер, в который помещается актуальное поле ввода. / The container where the active input field is placed.</param>
        /// <param name="valueProps">Массив всех числовых сериализованных полей структуры. / An array of all numeric serialized properties of the structure.</param>
        /// <param name="currentType">Текущий выбранный числовой тип данных. / The currently selected numeric data type.</param>
        private void RebuildInterface(VisualElement container, SerializedProperty[] valueProps, NumericType currentType)
        {
            container.Clear();
            int index = (int)currentType;
            if (index < 0 || index >= valueProps.Length) return;

            SerializedProperty activeProp = valueProps[index];
            bool isInteger = currentType != NumericType.Single && currentType != NumericType.Double;
            if (isInteger)
            {
                LongField longField = new LongField(VALUE_LABEL_TEXT);
                longField.SetValueWithoutNotify(activeProp.longValue);
                longField.AddToClassList(LongField.alignedFieldUssClassName);

                longField.RegisterValueChangedCallback(@event =>
                {
                    long rawValue = @event.newValue;
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
                        longField.SetValueWithoutNotify(clamped);
                    }
                    activeProp.longValue = clamped;
                    activeProp.serializedObject.ApplyModifiedProperties();
                });
                container.Add(longField);
            }
            else
            {
                DoubleField doubleField = new DoubleField(VALUE_LABEL_TEXT);
                doubleField.SetValueWithoutNotify(activeProp.doubleValue);
                doubleField.AddToClassList(DoubleField.alignedFieldUssClassName);

                doubleField.RegisterValueChangedCallback(@event =>
                {
                    double rawValue = @event.newValue;
                    double clamped = rawValue;
                    if (currentType == NumericType.Single)
                    {
                        clamped = Math.Clamp(rawValue, float.MinValue, float.MaxValue);
                    }
                    if (rawValue != clamped)
                    {
                        doubleField.SetValueWithoutNotify(clamped);
                    }
                    activeProp.doubleValue = clamped;
                    activeProp.serializedObject.ApplyModifiedProperties();
                });
                container.Add(doubleField);
            }
        }
    }
}