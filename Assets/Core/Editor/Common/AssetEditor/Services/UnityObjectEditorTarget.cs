using System;
using UnityEngine;

namespace CrystalEditor
{
    public class UnityObjectEditorTarget : IEditorTarget
    {
        public string AssetPath { get; }
        public Type TargetType => RawObject?.GetType();
        public object RawObject { get; }

        public UnityObjectEditorTarget(UnityEngine.Object unityObject, string assetPath)
        {
            RawObject = unityObject ?? throw new ArgumentNullException(nameof(unityObject));
            AssetPath = assetPath;
        }

        public T GetAs<T>() where T : class
        {
            return RawObject as T;
        }
    }
}