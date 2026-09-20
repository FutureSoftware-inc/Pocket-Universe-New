using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using CrystalEngine;

namespace CrystalEngineEditor
{
    public class GridNode : Node
    {
        private readonly GraphNodeData _nodeData;
        private object _targetContextData;

        public GraphNodeData NodeData => _nodeData;
        public string Guid => _nodeData.Guid;
        public object TargetContextData => _targetContextData;

        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public GridNode(GraphNodeData nodeData)
        {
            _nodeData = nodeData;

            viewDataKey = nodeData.Guid;
            title = nodeData.NodeName;
            SetPosition(new Rect(nodeData.Position, Vector2.zero));

            // СНАЧАЛА накладываем нативный визуал, чтобы дочерние элементы унаследовали стили
            ApplyNativeGraphStyles();
            BuildNodeInterface();
        }

        public void BindContext(object contextData)
        {
            _targetContextData = contextData;
            OnContextBound();
        }

        protected virtual void OnContextBound() { }

        /// <summary>
        /// Внедряет оригинальные USS-классы стилей и структуры, зашитые в ресурсах Unity GraphView.
        /// </summary>
        private void ApplyNativeGraphStyles()
        {
            // Подключаем системный файл стилей нод Unity, если он еще не подтянут холстом
            StyleSheet nodeStyle = UnityEditor.EditorGUIUtility.Load("GraphView.uss") as StyleSheet;
            if (nodeStyle != null)
            {
                styleSheets.Add(nodeStyle);
            }

            // Добавляем оригинальные классы стилизации окон ShaderGraph/Animator
            AddToClassList("node");
            titleContainer.AddToClassList("title");

            // Базовая нативная геометрия узла по стандартам Unity
            style.minWidth = 140;
            style.position = Position.Absolute;
        }

        private void BuildNodeInterface()
        {
            InputPort = CreatePort(Direction.Input, "In");
            OutputPort = CreatePort(Direction.Output, "Out");

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        private Port CreatePort(Direction direction, string portName)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(bool));
            port.portName = portName;

            // Стилизуем порты под аккуратные круглые точки оригинального API
            port.style.backgroundColor = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 0.3f));

            return port;
        }
    }
}
