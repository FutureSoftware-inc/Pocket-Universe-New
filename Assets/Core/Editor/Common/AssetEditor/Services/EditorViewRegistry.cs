using System;
using System.Collections.Generic;

namespace Crystal.Common.Editor
{
    public class EditorViewRegistry
    {
        private static readonly List<ViewData> RegisteredViews = new List<ViewData>();

        public struct ViewData
        {
            public string DisplayName;
            public Type ViewType;
            public Type AssetType;
        }

        static EditorViewRegistry()
        {
            // Здесь мы регистрируем наши будущие модули. К примеру:
            // Register("Машина состояний (HFSM)", typeof(HFSMGraphEditorView), typeof(HFSMStateMachineAsset));
            // Register("Предметы", typeof(ItemsEditorView), typeof(ItemAsset));
        }

        public static void Register(string displayName, Type viewType, Type assetType)
        {
            RegisteredViews.Add(new ViewData
            {
                DisplayName = displayName,
                ViewType = viewType,
                AssetType = assetType
            });
        }

        public static List<ViewData> GetViews() => RegisteredViews;
    }
}