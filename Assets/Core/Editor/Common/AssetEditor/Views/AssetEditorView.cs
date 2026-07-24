using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public abstract class AssetEditorView
    {
        protected EditorWindow HostWindow { get; private set; }

        /// <summary>
        /// Контейнер внутри главного окна, выделенный под это представление.
        /// </summary>
        protected VisualElement TargetContainer { get; private set; }

        public void Initialize(EditorWindow hostWindow, VisualElement targetContainer)
        {
            HostWindow = hostWindow ?? throw new ArgumentNullException(nameof(hostWindow));
            TargetContainer = targetContainer ?? throw new ArgumentNullException(nameof(targetContainer));

            // Очищаем правый экран от старых модулей перед отрисовкой своего UI
            TargetContainer.Clear();

            OnInitialize();
        }

        protected abstract void OnInitialize();
        public abstract void OpenAsset(ScriptableObject asset);
        public abstract void OnDisable();
    }
}
