using System;

namespace CrystalEditor
{
    public readonly struct ViewData
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
}