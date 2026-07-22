using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Валидатор и инструмент автоматического восстановления YAML файлов ассетов (префабов и ScriptableObject) Unity.
    /// Позволяет обнаруживать и исправлять сломанные ссылки [SerializeReference] (Missing Types), возникающие при переименовании классов.
    /// <br/><br/>
    /// A validator and automatic healing tool for Unity YAML asset files (Prefabs and ScriptableObjects).
    /// Detects and repairs broken [SerializeReference] fields (Missing Types) that occur due to class renaming.
    /// </summary>
    public static class YamlValidator
    {
        /// <summary>
        /// Регулярное выражение для поиска и извлечения сигнатур сериализованных управляемых ссылок в YAML структуре Unity.
        /// <br/><br/>
        /// Compiled regex to locate and parse managed reference class identifiers inside Unity's YAML format.
        /// </summary>
        private static readonly Regex RefIdentifierRegex = new Regex(@"managedReferenceClassIdentifier:\s*([^:\r\n]+):([^:\r\n]+):([^:\r\n]+)", RegexOptions.Compiled);

        /// <summary>
        /// Контейнер данных, содержащий результаты анализа поврежденного ассета проекта.
        /// <br/><br/>
        /// A data structure holding the results of a corrupted asset analysis within the project.
        /// </summary>
        public struct BrokenAssetResult
        {
            /// <summary>
            /// Относительный путь к поврежденному ассету в папке Assets.
            /// <br/><br/>
            /// The relative project path to the corrupted asset file.
            /// </summary>
            public string AssetPath;

            /// <summary>
            /// Имя отсутствующего (удаленного или переименованного) класса C#.
            /// <br/><br/>
            /// The name of the missing (deleted or renamed) C# class.
            /// </summary>
            public string MissingClassName;

            /// <summary>
            /// Полная строка текста из YAML файла, содержащая поврежденный идентификатор класса.
            /// <br/><br/>
            /// The complete text line from the YAML file hosting the invalid class identifier.
            /// </summary>
            public string FullLine;
        }

        /// <summary>
        /// Сканирует весь проект на наличие Префабов и ScriptableObject, проверяя текстовый YAML код на присутствие несуществующих типов C#.
        /// <br/><br/>
        /// Scans the entire project for Prefabs and ScriptableObjects, evaluating raw YAML data for the presence of non-existent C# types.
        /// </summary>
        /// <returns>Список результатов сканирования с подробной информацией обо всех поврежденных ассетах. / A list of scanning results containing detailed information about all corrupted assets.</returns>
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

        /// <summary>
        /// Производит прямую текстовую замену старого имени класса на новое внутри файла ассета и принудительно переимпортирует его в Unity.
        /// <br/><br/>
        /// Performs a direct string replacement of the old class name with the new one inside the asset file and forces a re-import in Unity.
        /// </summary>
        /// <param name="assetPath">Путь к восстанавливаемому файлу ассета. / The path to the asset file being repaired.</param>
        /// <param name="oldClassName">Старое (поврежденное) имя класса. / The old (broken) class name.</param>
        /// <param name="newClassName">Новое актуальное имя класса для замены. / The new valid class name to substitute.</param>
        /// <returns>True, если замена выполнена успешно и ассет обновлен; иначе false. / True if the replacement succeeded and the asset was updated; otherwise, false.</returns>
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

        /// <summary>
        /// Проверяет наличие указанного типа класса в сборках текущего домена приложения с учетом базовых пространств имен проекта.
        /// <br/><br/>
        /// Checks for the existence of the specified class type in the current application domain assemblies, considering project base namespaces.
        /// </summary>
        /// <param name="shortClassName">Короткое или частичное имя проверяемого класса. / The short or partial name of the class to validate.</param>
        /// <returns>True, если тип найден хотя бы в одной из пользовательских сборок; иначе false. / True if the type is resolved in at least one user assembly; otherwise, false.</returns>
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