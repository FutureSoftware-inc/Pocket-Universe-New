using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using YamlDotNet.RepresentationModel;
using Object = UnityEngine.Object;

namespace Crystal.Common.Editor
{
    public static class MissingTypeUtils
    {
        public static string GetDeltaData(this ManagedReferenceMissingType missingTypeData)
        {
            return FormatterDetailData(missingTypeData.assemblyName, missingTypeData.namespaceName, missingTypeData.className,
                missingTypeData.referenceId, missingTypeData.serializedData);
        }

        public static IReadOnlyList<(string propertyPath, long field)> GetMissingPropertyPaths(SerializedProperty property, string assetPath)
        {
            Object targetObject = property.serializedObject.targetObject;
            ManagedReferenceMissingType[] missingTypes = SerializationUtility.GetManagedReferencesWithMissingTypes(targetObject);
            if (missingTypes == null || missingTypes.Length == 0) return Array.Empty<(string, long)>();
            HashSet<string> allSerializeReferencePaths = FindAllSerializeReferencePathsInTargetObject(targetObject);
            List<(string propertyPath, long refId)> missingPaths = new List<(string propertyPath, long refId)>();
            YamlStream yaml = new YamlStream();
            using (var reader = new StreamReader(assetPath))
            {
                yaml.Load(reader);
            }
            YamlDocument document = yaml.Documents.FirstOrDefault();
            if (document == null || document.RootNode == null) return missingPaths;
            YamlMappingNode rootMapping = document.RootNode as YamlMappingNode;
            KeyValuePair<YamlNode, YamlNode>? firstChild = rootMapping?.Children.FirstOrDefault();
            if (firstChild == null) return missingPaths;
            string rootNodeName = firstChild.Value.Key.ToString();
            foreach (var path in allSerializeReferencePaths)
            {
                string shortPath = ConvertPropertyPath(path);
                string[] subPathElements = shortPath.Split('.');
                YamlNode propertyNode = document.RootNode;
                if (propertyNode is YamlMappingNode rootMap && rootMap.Children.TryGetValue(new YamlScalarNode(rootNodeName), out var nextNode))
                {
                    propertyNode = nextNode;
                    foreach (var element in subPathElements)
                    {
                        if (propertyNode is YamlMappingNode currentMap && currentMap.Children.TryGetValue(new YamlScalarNode(element), out var childNode))
                        {
                            propertyNode = childNode;
                        }
                        else
                        {
                            propertyNode = null;
                            break;
                        }
                    }
                }
                else
                {
                    propertyNode = null;
                }
                if (propertyNode is YamlMappingNode map && map.Children.TryGetValue(new YamlScalarNode("rid"), out var ridNode))
                {
                    string ridStr = ridNode.ToString();
                    if (long.TryParse(ridStr, out long numericRid))
                    {
                        if (TryGetMissingReference(numericRid, missingTypes, out var missingProperty))
                        {
                            missingPaths.Add((path, missingProperty.referenceId));
                        }
                    }
                }
            }
            return missingPaths;
        }

        private static HashSet<string> FindAllSerializeReferencePathsInTargetObject(Object targetObject)
        {
            HashSet<string> paths = new HashSet<string>();
            SOUtils.TraverseSO(targetObject, serializeReferenceProperty =>
            {
                paths.Add(serializeReferenceProperty.propertyPath);
                return false;
            });
            return paths;
        }

        private static bool TryGetMissingReference(long targetRid, ManagedReferenceMissingType[] missingTypes, out ManagedReferenceMissingType missingType)
        {
            for (int i = 0; i < missingTypes.Length; i++)
            {
                if (missingTypes[i].referenceId == targetRid)
                {
                    missingType = missingTypes[i];
                    return true;
                }
            }
            missingType = default;
            return false;
        }

        private static string FormatterDetailData(string assemblyName, string namespaceName, string className, long referenceId, string serializedData)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"ASM: {assemblyName}");
            builder.AppendLine($"Namespace: {namespaceName}");
            builder.AppendLine($"Class: {className}");
            builder.AppendLine($"RefId: {referenceId}");
            builder.AppendFormat("\n{0}", serializedData);
            return builder.ToString();
        }

        private static string ConvertPropertyPath(string path)
        {
            return path.Replace(".Array.data", string.Empty);
        }
    }
}