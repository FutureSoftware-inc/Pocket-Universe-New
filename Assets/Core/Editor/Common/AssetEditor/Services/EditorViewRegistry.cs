using Crystal.HFSM;
using Crystal.HFSM.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public class EditorViewRegistry
    {
        private static readonly List<ViewData> RegisteredViews = new List<ViewData>();

        public struct ViewData
        {
            public string DisplayName { get; }
            public Type ViewType { get; }
            public Type AssetType { get; }

            public ViewData(string displayName, Type viewType, Type assetType)
            {
                DisplayName = displayName;
                ViewType = viewType;
                AssetType = assetType;
            }
        }

        static EditorViewRegistry()
        {
            Register("State machine editor", typeof(StateGraphEditorView), typeof(BehaviourGraphData));
            // Здесь мы регистрируем наши будущие модули. К примеру:
            // Register("Машина состояний (HFSM)", typeof(HFSMGraphEditorView), typeof(HFSMStateMachineAsset));
            // Register("Предметы", typeof(ItemsEditorView), typeof(ItemAsset));
        }

        public static void Register(string displayName, Type viewType, Type assetType)
        {
            RegisteredViews.Add(new ViewData(displayName, viewType, assetType));
        }

        public static List<ViewData> GetViews() => RegisteredViews;
    }
}