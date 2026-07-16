using System;
using UnityEngine;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SeriazlizeReferenceSelectionAttribute : PropertyAttribute
    {
        public RenderingFlags Flags { get; }

        public SeriazlizeReferenceSelectionAttribute(RenderingFlags flags)
        {
            Flags = flags;
        }
    }
}