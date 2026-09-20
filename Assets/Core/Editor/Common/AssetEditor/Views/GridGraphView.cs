using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using CrystalEngine;

namespace CrystalEngineEditor
{
    public sealed class GridGraphView : GraphView
    {
        public event Action<Vector2, Vector2> OnNodeSpawnRequested;

        public GridGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            style.position = Position.Relative;
            style.flexGrow = 1;

            style.marginTop = 0;
            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;

            StyleSheet nativeGraphStyle = UnityEditor.EditorGUIUtility.Load("GraphView.uss") as StyleSheet;
            if (nativeGraphStyle != null)
            {
                styleSheets.Add(nativeGraphStyle);
            }
            else
            {
                Debug.LogWarning("[CrystalEngineEditor] Не удалось загрузить системный стиль 'GraphView'. Используется дефолтный фон.");
            }
            RegisterCallback<ContextualMenuPopulateEvent>(PopulateContextMenu);
        }


        private void PopulateContextMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 localGraphPosition = contentViewContainer.WorldToLocal(evt.localMousePosition);
            Vector2 screenPosition = evt.triggerEvent is IMouseEvent mouseEvent
                ? mouseEvent.mousePosition
                : GUIUtility.GUIToScreenPoint(evt.localMousePosition);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Добавить узел...", _ => OnNodeSpawnRequested?.Invoke(localGraphPosition, screenPosition));
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port &&
                    startPort.node != port.node &&
                    startPort.direction != port.direction &&
                    startPort.portType == port.portType)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
    }
}