using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public sealed class IconMetadataExtension : TypeMetadataExtension
    {
        public Texture2D Icon { get; private set; }

        public override void Initialize(Type type, Type baseType)
        {
            var attr = type.GetCustomAttributes(typeof(SelectorIconAttribute), false).FirstOrDefault() as SelectorIconAttribute;
            if (attr == null || string.IsNullOrEmpty(attr.IconName)) return;

            Icon = EditorGUIUtility.IconContent(attr.IconName)?.image as Texture2D
                   ?? AssetDatabase.LoadAssetAtPath<Texture2D>(attr.IconName);
        }
    }
}