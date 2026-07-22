using UnityEngine;
using UnityEditor.Experimental.GraphView;
using CrystalEngine;

namespace CrystalEditor
{
    public class StateNode : Node
    {
        private GraphNodeData _nodeData; // Теперь храним как легкую структуру данных
        private Port _inputPort;
        private Port _outputPort;
        private object _targetContextData;

        public GraphNodeData NodeData => _nodeData;
        public string Guid => _nodeData.Guid;
        public object TargetContextData => _targetContextData;

        public StateNode(GraphNodeData nodeData)
        {
            _nodeData = nodeData;

            viewDataKey = nodeData.Guid;
            SetPosition(new Rect(nodeData.Position, Vector2.zero));
            title = nodeData.NodeName;

            CreatePorts();
            RefreshExpandedState();
            RefreshPorts();
        }

        public void BindContext(object contextData)
        {
            _targetContextData = contextData;
        }

        private void CreatePorts()
        {
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            _inputPort.portName = "Enter";
            inputContainer.Add(_inputPort);

            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            _outputPort.portName = "Transition";
            outputContainer.Add(_outputPort);
        }
    }
}