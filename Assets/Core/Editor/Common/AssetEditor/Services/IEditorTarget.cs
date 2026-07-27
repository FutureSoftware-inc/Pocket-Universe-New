using System;

namespace CrystalEditor
{
    public interface IEditorTarget
    {
        string AssetPath { get; }
        Type TargetType { get; }
        object RawObject { get; }
        T GetAs<T>() where T : class;
    }
}