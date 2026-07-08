using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using Crystal.Common;

namespace Crystal.Editor
{
    [CustomPropertyDrawer(typeof(AnyNumber))]
    public class AnyNumberDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceBetween;

            SerializedProperty typeProp = property.FindPropertyRelative("_type");
            SerializedProperty byteProp = property.FindPropertyRelative("_asByte");
            SerializedProperty sbyteProp = property.FindPropertyRelative("_asSByte");
            SerializedProperty uint16Prop = property.FindPropertyRelative("_asUInt16");
            SerializedProperty int16Prop = property.FindPropertyRelative("_asInt16");
            SerializedProperty uint32Prop = property.FindPropertyRelative("_asUInt32");
            SerializedProperty int32Prop = property.FindPropertyRelative("_asInt32");
            SerializedProperty uint64Prop = property.FindPropertyRelative("_asUInt64");
            SerializedProperty int64Prop = property.FindPropertyRelative("_asInt64");
            SerializedProperty singleProp = property.FindPropertyRelative("_asSingle");
            SerializedProperty doubleProp = property.FindPropertyRelative("_asDouble");

            PropertyField typeField = new PropertyField(typeProp, "");
            typeField.style.flexGrow = 0.3f;
            typeField.style.marginRight = 4;
            container.Add(typeField);

            VisualElement valueContainer = new VisualElement();
            valueContainer.style.flexGrow = 0.7f;
            container.Add(valueContainer);
        }
    }
}