using System;
using UnityEngine;

namespace Crystal.Common.Editor
{
    [Serializable]
    public struct GraphNodeData
    {
        [SerializeField] private string _guid;
        [SerializeField] private Vector2 _position;
        [SerializeField] private string _nodeName;

        public string Guid => _guid;
        public Vector2 Position => _position;
        public string NodeName => _nodeName;

        public GraphNodeData(string guid, Vector2 position, string name)
        {
            _guid = guid ?? throw new ArgumentNullException(nameof(guid));
            _position = position;
            _nodeName = name;
        }
    }
}