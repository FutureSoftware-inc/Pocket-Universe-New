using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    public abstract class AssetEditorView
    {
        protected EditorWindow HostWindow { get; private set; }
        protected VisualElement TargetContainer { get; private set; }

        /// <summary>
        /// Обязательный тип данных, с которым умеет работать данный модуль.
        /// </summary>
        public abstract Type TargetAssetType { get; }

        public void Initialize(EditorWindow hostWindow, VisualElement targetContainer)
        {
            HostWindow = hostWindow ?? throw new ArgumentNullException(nameof(hostWindow));
            TargetContainer = targetContainer ?? throw new ArgumentNullException(nameof(targetContainer));
            TargetContainer.Clear();
            OnInitialize();
        }

        protected abstract void OnInitialize();
        public abstract void OpenTarget(IEditorTarget target);
        public abstract void OnDisable();
    }
}
