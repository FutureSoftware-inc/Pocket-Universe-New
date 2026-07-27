using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrystalEditor
{
    /// <summary>
    /// Контекст для работы с отдельным generic-параметром (аргументом типа) в редакторе Unity.
    /// Анализирует ограничения (constraints) дженерика и фильтрует список типов проекта, находя только подходящие для подстановки.
    /// <br/><br/>
    /// A context class for handling an individual generic parameter (type argument) within the Unity Editor.
    /// Analyzes generic constraints and filters the project's type list to find only those eligible for substitution.
    /// </summary>
    public sealed class GenericParameterContext
    {
        /// <summary>
        /// Имя аргумента generic-параметра (например, TContext или TInterface).
        /// <br/><br/>
        /// The name of the generic parameter argument (e.g., TContext or TInterface).
        /// </summary>
        public string ArgumentName { get; }

        /// <summary>
        /// Выбранный в данный момент тип для подстановки в этот параметр.
        /// <br/><br/>
        /// The currently selected type to be substituted into this parameter.
        /// </summary>
        public Type SelectedType { get; set; }

        /// <summary>
        /// Список всех типов проекта, которые удовлетворяют ограничениям этого generic-параметра.
        /// <br/><br/>
        /// A read-only list of all project types that satisfy the constraints of this generic parameter.
        /// </summary>
        public IReadOnlyList<Type> AvailableTypes { get; }

        /// <summary>
        /// Инициализирует новый экземпляр контекста generic-параметра и автоматически фильтрует доступные типы проекта.
        /// <br/><br/>
        /// Initializes a new instance of the generic parameter context and automatically filters available project types.
        /// </summary>
        /// <param name="genericParameter">Метаданные исследуемого generic-параметра. Не может быть null.<br/><br/>The metadata of the generic parameter to examine. Cannot be null.</param>
        /// <param name="allProjectTypes">Список всех зарегистрированных в сборках проекта типов. Не может быть null.<br/><br/>The list of all types registered in the project assemblies. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если один из переданных параметров равен null.<br/><br/>Thrown when one of the specified parameters is null.</exception>
        public GenericParameterContext(Type genericParameter, IReadOnlyList<Type> allProjectTypes)
        {
            if (genericParameter == null)
                throw new ArgumentNullException(nameof(genericParameter));
            if (allProjectTypes == null)
                throw new ArgumentNullException(nameof(allProjectTypes));
            ArgumentName = genericParameter.Name;
            SelectedType = null;
            AvailableTypes = FilterValidTypes(genericParameter, allProjectTypes);
        }

        /// <summary>
        /// Проходит по всем типам проекта и отбирает те, которые полностью соответствуют ограничениям дженерика.
        /// <br/><br/>
        /// Iterates through all project types and selects those that fully comply with the generic constraints.
        /// </summary>
        private List<Type> FilterValidTypes(Type genericParameter, IReadOnlyList<Type> allProjectTypes)
        {
            List<Type> validTypes = new List<Type>();
            Type[] constrains = genericParameter.GetGenericParameterConstraints();
            GenericParameterAttributes attributes = genericParameter.GenericParameterAttributes;
            foreach (Type targetType in allProjectTypes)
            {
                if (IsValidForConstrains(targetType, constrains, attributes))
                {
                    validTypes.Add(targetType);
                }
            }
            return validTypes;
        }

        /// <summary>
        /// Выполняет комплексную проверку целевого типа на соответствие всем базовым типам и битовым флагам ограничений.
        /// <br/><br/>
        /// Performs a comprehensive evaluation of the target type against all base types and constraint bitwise flags.
        /// </summary>
        private bool IsValidForConstrains(Type targetType, Type[] constrains, GenericParameterAttributes attributes)
        {
            return SatisfiesBaseTypes(targetType, constrains)
                   && SatisfiesStructConstraint(targetType, attributes)
                   && SatisfiesClassConstraint(targetType, attributes)
                   && SatisfiesConstructorConstraint(targetType, attributes);
        }

        /// <summary>
        /// Проверяет, наследуется ли целевой тип от требуемых классов или реализует ли нужные интерфейсы ограничений.
        /// <br/><br/>
        /// Checks if the target type inherits from the required classes or implements the required constraint interfaces.
        /// </summary>
        private bool SatisfiesBaseTypes(Type targetType, Type[] constrains)
        {
            return constrains.All(constraint => constraint.IsAssignableFrom(targetType));
        }

        /// <summary>
        /// Проверяет ограничение на значимый тип (where T : struct). Корректно отсекает Nullable-структуры.
        /// <br/><br/>
        /// Validates the value type constraint (where T : struct). Correctly filters out Nullable structures.
        /// </summary>
        private bool SatisfiesStructConstraint(Type targetType, GenericParameterAttributes attributes)
        {
            if (!attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                return true;
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null;
        }

        /// <summary>
        /// Проверяет ограничение на ссылочный тип (where T : class).
        /// <br/><br/>
        /// Validates the reference type constraint (where T : class).
        /// </summary>
        private bool SatisfiesClassConstraint(Type targetType, GenericParameterAttributes attributes)
        {
            bool isReferenceType = attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint);
            bool isNotNullable = attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint);
            bool hasClassConstraint = isReferenceType && !isNotNullable;
            if (!hasClassConstraint)
                return true;
            return !targetType.IsValueType;
        }

        /// <summary>
        /// Проверяет ограничение на наличие публичного конструктора без параметров (where T : new()).
        /// <br/><br/>
        /// Validates the parameterless constructor constraint (where T : new()).
        /// </summary>
        private bool SatisfiesConstructorConstraint(Type targetType, GenericParameterAttributes attributes)
        {
            if (!attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                return true;
            if (targetType.IsValueType)
                return true;
            return targetType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) != null;
        }
    }
}