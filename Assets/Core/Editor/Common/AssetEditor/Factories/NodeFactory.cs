using CrystalEngine;
using System;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Фабрика для генерации и подготовки метаданных узлов графа.
    /// </summary>
    public static class NodeFactory
    {
        /// <summary>
        /// Создает заполненный объект метаданных GraphNodeData с уникальным GUID.
        /// </summary>
        public static GraphNodeData CreateMetadata(Type nodeType, Vector2 position)
        {
            if (nodeType == null) throw new ArgumentNullException(nameof(nodeType));

            string newGuid = Guid.NewGuid().ToString();
            string cleanName = nodeType.Name;

            // Используем конструктор строго по твоей сигнатуре (guid, position, name)
            return new GraphNodeData(newGuid, position, cleanName);
        }
    }
}
