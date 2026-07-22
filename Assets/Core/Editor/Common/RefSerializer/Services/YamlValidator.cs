using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CrystalEditor
{
    public static class YamlValidator
    {
        private static readonly Regex RefIdentifierRegex = new Regex(@"managedReferenceClassIdentifier:\s*([^:\r\n]+):([^:\r\n]+):([^:\r\n]+)", RegexOptions.Compiled);

        public struct BrokenAssetResult
        {
            public string AssetPath;
            public string MissingClassName;
            public string FullLine;
        }

        public static List<BrokenAssetResult> ScanProjectForBrokenReferences()
        {
            var results = new List<BrokenAssetResult>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    Match match = RefIdentifierRegex.Match(line);
                    if (match.Success)
                    {
                        string className = match.Groups[3].Value.Trim();
                        if (!TypeExistsInProject(className))
                        {
                            results.Add(new BrokenAssetResult
                            {
                                AssetPath = path,
                                MissingClassName = className,
                                FullLine = line
                            });
                        }
                    }
                }
            }
            return results;
        }

        public static bool FixBrokenReference(string assetPath, string oldClassName, string newClassName)
        {
            if (!File.Exists(assetPath)) return false;
            try
            {
                string fileText = File.ReadAllText(assetPath);
                if (!fileText.Contains(oldClassName)) return false;
                string updatedText = fileText.Replace(oldClassName, newClassName);
                File.WriteAllText(assetPath, updatedText);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[YamlValidator] Не удалось исправить файл {assetPath}. Ошибка: {exception.Message}");
                return false;
            }
        }

        private static bool TypeExistsInProject(string shortClassName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("System")) continue;
                Type type = assembly.GetType(shortClassName) ?? assembly.GetType($"Crystal.HFSM.{shortClassName}") ?? assembly.GetType($"Crystal.Common.{shortClassName}");
                if (type != null) return true;
            }
            return false;
        }
    }
}