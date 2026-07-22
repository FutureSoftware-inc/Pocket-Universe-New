using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    public abstract class AssetEditorView
    {
        protected EditorWindow HostWindow { get; private set; }

        public VisualElement Root { get; private set; }

        public void Initialize(EditorWindow hostWindow)
        {
            HostWindow = hostWindow;
            Root = new VisualElement { name = "ViewRoot" };
            Root.style.flexGrow = 1;

            OnInitialize();
        }

        protected abstract void OnInitialize();

        public abstract void OpenAsset(ScriptableObject asset);

        public abstract void OnDisable();
    }
}