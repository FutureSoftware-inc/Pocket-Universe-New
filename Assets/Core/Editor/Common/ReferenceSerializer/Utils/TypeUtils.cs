using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;
using AssemblyFlags = UnityEditor.Compilation.AssemblyFlags;

namespace Crystal.Common.Editor
{
    public static class TypeUtils
    {
        internal const string ARRAY_PROPERTY_SUBSTRING = ".Array.data[";

        private static List<Type> _cachedDomainTypes;
        private static IReadOnlyList<Type> _systemObjectType;
        private static Type[] DefaultTypes = new[]
        {
            typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
            typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(string), typeof(Color), typeof(Color32), typeof(Vector2),
            typeof(Vector2Int), typeof(Vector3), typeof(Vector3Int), typeof(Vector4), typeof(Quaternion), typeof(Ray), typeof(Ray2D)
        };

        private static void ResetEditorCaches()
        {
            _cachedDomainTypes = null;
            _systemObjectType = null;
        }

        public static object CreateObjectFromType(Type type)
        {
            if (type == null) return null;
            return type.GetConstructor(Type.EmptyTypes) != null ?
                Activator.CreateInstance(type) :
                FormatterServices.GetUninitializedObject(type);
        }

        public static Type ExtractTypeFromString(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            var assemblyNameEndIndex = typeName.IndexOf(' ');
            if (assemblyNameEndIndex < 0)
            {
                return Type.GetType(typeName, false);
            }
            var assemblyName = typeName.Substring(0, assemblyNameEndIndex);
            assemblyName = assemblyName == "Assembly" ? "Assembly-CSharp" : assemblyName;
            var subStringTypeName = typeName.Substring(assemblyNameEndIndex + 1);
            try
            {
                var assembly = Assembly.Load(assemblyName);
                var type = assembly?.GetType(subStringTypeName);
                if (type != null) return type;
            }
            catch (Exception exeption)
            {
                Debug.LogWarning($"[Crystal HFSM] Couldn't load the assembly '{assemblyName}' for the type '{subStringTypeName}'! " +
                    $"The assembly may not have been compiled yet or the assembly has an invalid name! Error: {exeption.Message}");
            }
            return Type.GetType($"{subStringTypeName}, {assemblyName}", false);
        }

        public static bool IsFinalAssingableType(Type type)
        {
            return type != null && !type.IsAbstract && !type.IsInterface;
        }

        public static bool IsArrayElemnt(this SerializedProperty property)
        {
            return property.propertyPath.Contains(ARRAY_PROPERTY_SUBSTRING);
        }

        public static SerializedProperty GetArrayPropertyFromArrayElement(SerializedProperty property)
        {
            var path = property.propertyPath;
            var index = path.IndexOf(ARRAY_PROPERTY_SUBSTRING);
            return property.serializedObject.FindProperty(path.Remove(index));
        }

        public static IEnumerable<Type> GetAllTypesInCurrentDomain()
        {
            if (_cachedDomainTypes != null)
            {
                return _cachedDomainTypes;
            }
            _cachedDomainTypes = new List<Type>(8192);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    _cachedDomainTypes.AddRange(assembly.GetTypes());
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return _cachedDomainTypes;
        }

        public static Type GetConcreteGenericType(Type propertyType, Type genericType)
        {
            if (TryGetGenericArgumentsFromTargetType(propertyType, genericType, out IReadOnlyList<Type> genericArguments) == false)
            {
                return null;
            }
            if (genericArguments.Any(t => t == null || t.IsAbstract || t.IsInterface) ||
                AreGenericArgumentsValid(genericType, genericArguments) == false)
            {
                return null;
            }
            try
            {
                return genericType.MakeGenericType(genericArguments.ToArray());
            }
            catch (Exception)
            {
                //
                return null;
            }
        }

        public static IReadOnlyCollection<Type> GetAllSystemObjectTypes()
        {
            if (_systemObjectType != null) return _systemObjectType;
            HashSet<string> playerAssemblies = new HashSet<string>(CompilationPipeline.GetAssemblies()
                .Where(assembly => !assembly.flags.HasFlag(AssemblyFlags.EditorAssembly))
                .Select(assembly => assembly.name)
            );
            var customTypes = TypeCache.GetTypesDerivedFrom<object>()
                .Where(type =>
                {
                    if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        return true;
                    }
                    if (type.IsAbstract || type.IsInterface || type.IsGenericType || !type.IsSerializable)
                    {
                        return false;
                    }
                    string assemblyName = type.Assembly.GetName().Name;
                    return playerAssemblies.Contains(assemblyName) || assemblyName == "UnityEngine";
                })
                .OrderBy(type => type.FullName);
            List<Type> typeList = new List<Type>(DefaultTypes);
            typeList.AddRange(customTypes);
            _systemObjectType = typeList.ToArray();
            return _systemObjectType;
        }

        private static bool TryGetGenericArgumentsFromTargetType(Type targetType, Type genericType, out IReadOnlyList<Type> genericArguments)
        {
            genericArguments = null;
            if (targetType?.IsGenericType != true || !genericType?.IsGenericTypeDefinition != true)
            {
                return false;
            }
            Type mathingGenericType = GetMatchingGenericType(targetType, genericType);
            if (mathingGenericType == null)
            {
                return false;
            }
            Dictionary<Type, Type> genericParameterMap = new Dictionary<Type, Type>();
            if (!TryMapGenericArguments(mathingGenericType, targetType, genericParameterMap))
            {
                return false;
            }
            Type[] genericParameters = genericType.GetGenericArguments();
            genericArguments = genericParameters
                .Select(type => genericParameterMap.TryGetValue(type, out Type mappedType) ? mappedType : null).ToArray();
            return true;
        }

        private static bool TryMapGenericArguments(Type sourceType, Type targetType, Dictionary<Type, Type> genericParameterMap)
        {
            if (sourceType.IsGenericParameter)
            {
                if (genericParameterMap.TryGetValue(sourceType, out var mappedType))
                {
                    return mappedType == targetType;
                }
                genericParameterMap[sourceType] = targetType;
                return true;
            }
            if (sourceType.IsArray && targetType.IsArray)
            {
                return TryMapGenericArguments(sourceType.GetElementType(), targetType.GetElementType(),
                    genericParameterMap);
            }
            if (sourceType.IsGenericType && targetType.IsGenericType && sourceType.GetGenericTypeDefinition() == targetType.GetGenericTypeDefinition())
            {
                Type[] sourceArguments = sourceType.GetGenericArguments();
                Type[] targetArguments = targetType.GetGenericArguments();
                for (int i = 0; i < sourceArguments.Length; i++)
                {
                    if (TryMapGenericArguments(sourceArguments[i], targetArguments[i], genericParameterMap) == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            return sourceType == targetType;
        }

        private static Type GetMatchingGenericType(Type targetType, Type genericType)
        {
            Type targetGenericDefinition = targetType.GetGenericTypeDefinition();
            Type[] genericInterfaces = genericType.GetInterfaces();
            Type matchingInterface = genericInterfaces.FirstOrDefault(IsMatchingGenericType);
            if (matchingInterface != null)
            {
                return matchingInterface;
            }
            Type currentBaseType = genericType.BaseType;
            while (currentBaseType != null)
            {
                if (IsMatchingGenericType(currentBaseType))
                {
                    return currentBaseType;
                }
                currentBaseType = currentBaseType.BaseType;
            }
            return null;
            bool IsMatchingGenericType(Type type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == targetGenericDefinition;
            }
        }

        private static bool AreGenericArgumentsValid(Type genericType, IReadOnlyList<Type> genericArguments)
        {
            if (genericType?.IsGenericType != true)
            {
                return false;
            }
            Type[] genericParameters = genericType.GetGenericArguments();
            if (genericParameters.Length != genericArguments.Count)
            {
                return false;
            }
            for (int i = 0; i < genericParameters.Length; i++)
            {
                if (!IsGenericArgumentValid(genericParameters[i], genericArguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsGenericArgumentValid(Type genericParameter, Type genericArgument)
        {
            if (genericParameter?.IsGenericParameter != true || genericArgument == null) return false;
            var attributes = genericParameter.GenericParameterAttributes;
            var specialConstraints = attributes & GenericParameterAttributes.SpecialConstraintMask;
            if (specialConstraints.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && genericArgument.IsValueType)
            {
                return false;
            }
            if (specialConstraints.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
                (genericArgument.IsValueType == false || Nullable.GetUnderlyingType(genericArgument) != null))
            {
                return false;
            }
            if (specialConstraints.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
                HasDefaultConstructor(genericArgument) == false)
            {
                return false;
            }
            var constraints = genericParameter.GetGenericParameterConstraints();
            foreach (var constraint in constraints)
            {
                if (SatisfiesGenericConstraint(constraint, genericArgument) == false) return false;
            }
            return true;
            bool HasDefaultConstructor(Type type)
            {
                return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
            }
            bool SatisfiesGenericConstraint(Type constraint, Type argument)
            {
                if (constraint.IsAssignableFrom(argument)) return true;
                if (constraint.IsGenericType == false) return false;
                var constraintDefinition = constraint.GetGenericTypeDefinition();
                return argument.GetInterfaces().Any(IsMatchingGenericConstraint) ||
                       IsMatchingBaseGenericConstraint(argument.BaseType);
                bool IsMatchingGenericConstraint(Type type)
                {
                    return type.IsGenericType && type.GetGenericTypeDefinition() == constraintDefinition;
                }
                bool IsMatchingBaseGenericConstraint(Type type)
                {
                    while (type != null)
                    {
                        if (IsMatchingGenericConstraint(type)) return true;
                        type = type.BaseType;
                    }
                    return false;
                }
            }
        }

        public static List<Type> GetAssignableSerializeReferenceTypes(SerializedProperty property)
        {
            var propertyType = ExtractTypeFromString(property.managedReferenceFieldTypename);
            return GetAssignableSerializeReferenceTypes(propertyType);
        }

        public static List<Type> GetAssignableSerializeReferenceTypes(Type propertyType)
        {
            var derivedTypes = TypeCache.GetTypesDerivedFrom(propertyType);
            var nonUnityTypes = derivedTypes.Prepend(propertyType).Where(IsAssignableNonUnityType).ToList();
            nonUnityTypes.Insert(0, null);
            if (propertyType.IsGenericType)
            {
                var allTypes = GetAllTypesInCurrentDomain().Where(IsAssignableNonUnityType)
                    .Where(t => t.IsGenericType);

                var assignableGenericTypes = allTypes.Where(IsAssignableGenericTypeFromGenericProperty);
                nonUnityTypes.AddRange(assignableGenericTypes);
            }

            return nonUnityTypes.Distinct().ToList();

            bool IsAssignableNonUnityType(Type type)
            {
                return IsFinalAssignableType(type) && !type.IsSubclassOf(typeof(UnityEngine.Object));
            }

            bool IsAssignableGenericTypeFromGenericProperty(Type type)
            {
                return TryGetGenericArgumentsFromTargetType(propertyType, type, out _);
            }
        }

        public static bool IsFinalAssignableType(Type type)
        {
            return type.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;
        }
    }
}