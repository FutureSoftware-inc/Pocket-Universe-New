using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using CrystalEngine;

namespace CrystalEngineEditor
{
    public static class NodeFactory
    {
        // 1. Создание метаданных для одной ноды
        public static GraphNodeData CreateMetadata(System.Type type, Vector2 position, string customName = "")
        {
            string guid = System.Guid.NewGuid().ToString();
            string name = string.IsNullOrEmpty(customName) ? type.Name : customName;
            return new GraphNodeData(guid, position, name);
        }

        public static void ExtractGraphData(GraphView graphView, out List<GraphNodeData> nodesToSave, out List<ISyncState<IBlackboardProvider>> runtimeStatesToSave)
        {
            nodesToSave = new List<GraphNodeData>();
            runtimeStatesToSave = new List<ISyncState<IBlackboardProvider>>();
            if (graphView == null || graphView.nodes == null) return;
            foreach (var node in graphView.nodes)
            {
                if (node is GridNode visualNode)
                {
                    var currentData = new GraphNodeData(
                        visualNode.Guid,
                        visualNode.GetPosition().position,
                        visualNode.title
                    );
                    nodesToSave.Add(currentData);
                    if (visualNode is StateNode stateNode && stateNode.UnderlyingState != null)
                    {
                        runtimeStatesToSave.Add(stateNode.UnderlyingState);
                    }
                }
            }
        }
    }
}
