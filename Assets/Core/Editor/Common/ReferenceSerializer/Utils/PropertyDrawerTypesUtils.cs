using Crystal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using SerializationUtility = UnityEditor.SerializationUtility;

namespace Crystal.Common.Editor
{
    public static class PropertyDrawerTypesUtils
    {
        internal const string NULL_NAME = "null";

        private static readonly Dictionary<Type, string> BuiltInTypeNames = new()
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(float), "float" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(double), "double" },
            { typeof(object), "object" },
            { typeof(string), "string" },
            { typeof(decimal), "decimal" }
        };

        public static string GetTypeName(Type type)
        {
            if (type == null) return NULL_NAME;
            if (BuiltInTypeNames.TryGetValue(type, out var builtInTypeNames))
            {
                return builtInTypeNames;
            }
            var customTypeName = GetCustomTypeName(type);
            if (customTypeName != null)
            {
                return type.IsGenericType ? customTypeName + GetGenericArgumentsName(type) : customTypeName;
            }
            if (type.IsGenericType)
            {
                return ObjectNames.NicifyVariableName(GetGenericTypeNameWithoutArity(type) + GetGenericArgumentsName(type));
            }
            if (type.IsNested)
            {
                var typeName = type.FullName;
                var lastDot = typeName?.LastIndexOf('.');
                if (lastDot > 0)
                {
                    typeName = typeName.Substring(lastDot.Value + 1);
                }
                return ObjectNames.NicifyVariableName(typeName);
            }
            return ObjectNames.NicifyVariableName(type.Name);
        }

        private static string GetCustomTypeName(Type type)
        {
            var typesWithNames = TypeCache.GetTypesWithAttribute(typeof(SerializeReferenceNameAttribute));
            if (typesWithNames.Contains(type))
            {
                return type.GetCustomAttribute<SerializeReferenceNameAttribute>()?.Name ?? NULL_NAME;
            }
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (typesWithNames.Contains(genericTypeDefinition))
                {
                    return genericTypeDefinition.GetCustomAttribute<SerializeReferenceNameAttribute>()?.Name ?? NULL_NAME;
                }
            }
            return null;
        }



        private static string GetGenericArgumentsName(Type type)
        {
            var args = type.GetGenericArguments();
            if (args.Length == 0) return string.Empty;
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('<');
            for ( var i = 0; i < args.Length; i++)
            {
                stringBuilder.Append(GetTypeName(args[i]));
                if (i < args.Length - 1) stringBuilder.Append(", ");
            }
            stringBuilder.Append(">");
            return stringBuilder.ToString();
        }

        private static string GetGenericTypeNameWithoutArity(Type type)
        {
            var name = type.Name;
            var arityIndex = name.IndexOf('`');
            return arityIndex > 0 ? name : name.Substring(0, arityIndex);
        }
    }
}