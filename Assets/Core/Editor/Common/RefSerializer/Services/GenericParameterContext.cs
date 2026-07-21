using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Crystal.Common.Editor
{
    public sealed class GenericParameterContext
    {
        public string ArgumentName { get; }
        public Type SelectedType { get; set; }
        public IReadOnlyList<Type> AvailableTypes { get; }

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

        private bool IsValidForConstrains(Type targetType, Type[] constrains, GenericParameterAttributes attributes)
        {
            return SatisfiesBaseTypes(targetType, constrains)
                   && SatisfiesStructConstraint(targetType, attributes)
                   && SatisfiesClassConstraint(targetType, attributes)
                   && SatisfiesConstructorConstraint(targetType, attributes);
        }

        private bool SatisfiesBaseTypes(Type targetType, Type[] constrains)
        {
            return constrains.All(constraint => constraint.IsAssignableFrom(targetType));
        }

        private bool SatisfiesStructConstraint(Type targetType, GenericParameterAttributes attributes)
        {
            if (!attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                return true;
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null;
        }

        private bool SatisfiesClassConstraint(Type targetType, GenericParameterAttributes attributes)
        {
            bool isReferenceType = attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint);
            bool isNotNullable = attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint);
            bool hasClassConstraint = isReferenceType && !isNotNullable;
            if (!hasClassConstraint)
                return true;
            return !targetType.IsValueType;
        }

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