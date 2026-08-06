using System;
using UnityEngine;

namespace CrystalEngineEditor
{
    [Serializable]
    public class ClipboardPayload
    {
        [SerializeField] private string _assemblyQualifiedName;
        [SerializeField] private string _jsonData;

        public string AssemblyQualifiedName => _assemblyQualifiedName;
        public string JsonData => _jsonData;

        public ClipboardPayload(string assemblyQualifedName, string jsonData)
        {
            _assemblyQualifiedName = assemblyQualifedName;
            _jsonData = jsonData;
        }
    }
}