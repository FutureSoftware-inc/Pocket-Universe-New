using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Crystal.HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Editor
{
    // Привязываем драйвер к базовому классу Condition.
    // Благодаря true он автоматически перехватит все списки [SerializeReference] с наследниками!
    [CustomPropertyDrawer(typeof(Condition<>), true)]
    public class ConditionDrawer : PropertyDrawer
    {
        internal const string PROP_INVERT = "_invert";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.marginBottom = 8f;

            // Настраиваем красивую карточку-рамку вокруг каждого условия
            root.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.2f));

            // Проверяем: инициализирован ли объект полиморфизма, или там лежит null?
            bool isNull = property.managedReferenceValue == null;

            // 1. СТРОКА ВЫБОРА ТИПА (Dropdown для SerializeReference)
            // Создаем нативный выпадающий список
            List<string> typeNames = new List<string> { "<Null / Empty>" };

            // Через рефлексию находим все классы-наследники от нашего Condition в проекте
            Type baseType = fieldInfo.FieldType;
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                baseType = baseType.GetGenericArguments()[0]; // Вытаскиваем тип внутри List<T>
            }

            // Находим все неабстрактные классы в сборках, которые можно создать
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && IsSubclassOfRawGeneric(baseType.GetGenericTypeDefinition(), t))
                .ToList();

            typeNames.AddRange(derivedTypes.Select(t => t.Name));

            int selectedIndex = isNull ? 0 : derivedTypes.FindIndex(t => t == property.managedReferenceValue.GetType()) + 1;

            PopupField<string> typeSelector = new PopupField<string>("Condition Type", typeNames, selectedIndex);
            typeSelector.style.marginBottom = 4f;
            root.Add(typeSelector);

            // Обработка клика по выпадающему списку типов: создаем объект в памяти!
            typeSelector.RegisterValueChangedCallback(evt =>
            {
                int newIndex = typeSelector.index;
                if (newIndex == 0)
                {
                    property.managedReferenceValue = null; // Сброс в null
                }
                else
                {
                    Type targetType = derivedTypes[newIndex - 1];

                    // Если класс дженериковый (как NumericCondition<TContext>), нам нужно передать ему контекст от родителя!
                    if (targetType.IsGenericTypeDefinition)
                    {
                        Type contextType = baseType.GetGenericArguments()[0]; // Получаем UnitTest контекст
                        targetType = targetType.MakeGenericType(contextType);
                    }

                    // Высшая магия рефлексии: создаем экземпляр класса прямо в ячейке [SerializeReference]!
                    property.managedReferenceValue = Activator.CreateInstance(targetType);
                }

                // Применяем изменения, чтобы Unity мгновенно перерисовала инспектор
                property.serializedObject.ApplyModifiedProperties();
            });

            // Если объект пустой, прекращаем отрисовку (показываем только меню выбора)
            if (isNull) return root;

            // 2. СТРОКА 2: ШАПКА ИНИЦИАЛИЗИРОВАННОГО УСЛОВИЯ (Invert)
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 4f;
            root.Add(header);

            SerializedProperty invertProp = property.FindPropertyRelative(PROP_INVERT);
            if (invertProp != null)
            {
                PropertyField invertField = new PropertyField(invertProp, "Invert Result");
                invertField.AddToClassList(BaseBoolField.alignedFieldUssClassName);
                header.Add(invertField);
            }

            // 3. СТРОКА 3: ДИНАМИЧЕСКИЙ КОНТЕНТНЫЙ БЛОК ПОЛЕЙ НАСЛЕДНИКА
            VisualElement content = new VisualElement();
            content.style.paddingLeft = 15f;
            root.Add(content);

            SerializedProperty iterator = property.Copy();
            int baseDepth = iterator.depth;
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.depth <= baseDepth) break;
                if (iterator.name == "_invert") continue;

                PropertyField childField = new PropertyField(iterator);
                content.Add(childField);
            }

            return root;
        }

        // Вспомогательный Senior-метод для проверки иерархии дженериков через рефлексию
        private bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
