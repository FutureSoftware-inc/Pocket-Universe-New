using System;
using System.Reflection;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public static class GenericParameterContext
    {
        public static bool IsTypeValidForGenericArgument(Type targetType, Type genericArgumentType)
        {
            Type[] constrains = genericArgumentType.GetGenericParameterConstraints();
            foreach (Type type in constrains)
            {

            }
            return true;
        } 
    }
}