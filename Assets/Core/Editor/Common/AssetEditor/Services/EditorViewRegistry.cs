using CrystalEngine.HFSM;
using System;
using System.Collections.Generic;

namespace CrystalEditor
{
    public static class EditorViewRegistry
    {
        private static readonly List<ViewData> RegisteredViews = new();

        static EditorViewRegistry()
        {
            Register("State machine editor", typeof(StateGraphEditorView), typeof(BehaviourGraphData));
            //Register("State machine editor", typeof(StateGraphEditorView), typeof(BehaviourGraphData));
        }

        public static void Register(string displayName, Type viewType, Type assetType)
        {
            RegisteredViews.Add(new ViewData(displayName, viewType, assetType));
        }

        public static IReadOnlyList<ViewData> GetViews() => RegisteredViews;
    }
}