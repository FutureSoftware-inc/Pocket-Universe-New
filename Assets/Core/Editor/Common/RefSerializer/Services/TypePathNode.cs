using System;
using System.Collections.Generic;
using CrystalEngine;
using UnityEngine;

namespace CrystalEngineEditor
{
    public sealed class TypePathNode
    {
        public string Name { get; }
        public TypeMetadata Metadata { get; }
        public Dictionary<string, TypePathNode> SubNodes { get; } = new(StringComparer.Ordinal);
        public bool IsLeaf => Metadata != null;
        public TypePathNode(string name, TypeMetadata metadata = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Metadata = metadata;
        }

        public static TypePathNode BuildTree(IReadOnlyList<TypeMetadata> flatList, string rootName = "Select")
        {
            TypePathNode root = new TypePathNode(rootName);
            for (int i = 0; i < flatList.Count; i++)
            {
                TypeMetadata metadata = flatList[i];
                SubclassPathAttribute attribute = metadata.GetAttribute<SubclassPathAttribute>();
                string rawPath = attribute != null ? attribute.Path : string.Empty;
                string[] parts = string.IsNullOrEmpty(rawPath) ? Array.Empty<string>() : rawPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                TypePathNode current = root;
                for (int j = 0; j < parts.Length; j++)
                {
                    string part = parts[j].Trim();
                    if (string.IsNullOrEmpty(part))
                    {
                        continue;
                    }
                    if (!current.SubNodes.TryGetValue(part, out TypePathNode nextNode))
                    {
                        nextNode = new TypePathNode(part);
                        current.SubNodes[part] = nextNode;
                    }
                    current = nextNode;
                }
                string finalName = metadata.Name;
                if (!current.SubNodes.ContainsKey(finalName))
                {
                    current.SubNodes[finalName] = new TypePathNode(finalName, metadata);
                }
                else
                {
                    string uniqueName = $"{finalName} ({metadata.Type.FullName})";
                    if (!current.SubNodes.ContainsKey(uniqueName))
                    {
                        current.SubNodes[uniqueName] = new TypePathNode(uniqueName, metadata);
                    }
                    Debug.LogWarning($"[CrystalEngine] Duplicate menu item name detected for type:" +
                                    $" {metadata.Type.AssemblyQualifiedName}. Item was renamed to '{uniqueName}' to prevent overwrite.");
                }
            }
            return root;
        }
    }
}