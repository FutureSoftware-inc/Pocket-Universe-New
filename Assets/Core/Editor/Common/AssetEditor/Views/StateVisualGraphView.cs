using Crystal.HFSM;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    public sealed class StateVisualGraphView : GraphView
    {
        public StateVisualGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            style.flexGrow = 1;
        }

        public void PopulateFromAsset(BehaviourGraphData asset)
        {
            DeleteElements(graphElements);

            if (asset == null || asset.EditorNodes == null) return;

            foreach (GraphNodeData nodeData in asset.EditorNodes)
            {
                StateNode visualNode = new StateNode(nodeData);
                AddElement(visualNode);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }
    }
}