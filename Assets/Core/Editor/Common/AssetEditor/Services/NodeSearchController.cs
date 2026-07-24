using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Контроллер управления подсистемой контекстного поиска и спавна узлов на холсте.
    /// </summary>
    public sealed class NodeSearchController : IDisposable
    {
        private readonly GridGraphView _graphView;
        private readonly Action<Type, Vector2> _onTypeSelectedCallback;
        private GraphSearchWindowProvider _provider;

        public NodeSearchController(GridGraphView graphView, Action<Type, Vector2> onTypeSelectedCallback)
        {
            _graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            _onTypeSelectedCallback = onTypeSelectedCallback ?? throw new ArgumentNullException(nameof(onTypeSelectedCallback));

            _graphView.OnNodeSpawnRequested += OpenSearchWindow;
        }

        public void Initialize(Type[] availableTypes)
        {
            if (_provider != null) Dispose();

            _provider = ScriptableObject.CreateInstance<GraphSearchWindowProvider>();
            _provider.Initialize(availableTypes, _onTypeSelectedCallback);
        }

        private void OpenSearchWindow(Vector2 localGraphPosition, Vector2 screenPosition)
        {
            if (_provider == null) return;

            _provider.SetTargetPosition(localGraphPosition);
            SearchWindowContext context = new SearchWindowContext(screenPosition);
            SearchWindow.Open(context, _provider);
        }

        public void Dispose()
        {
            if (_graphView != null)
            {
                _graphView.OnNodeSpawnRequested -= OpenSearchWindow;
            }

            if (_provider != null)
            {
                ScriptableObject.DestroyImmediate(_provider);
                _provider = null;
            }
        }
    }
}
