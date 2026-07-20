using System;
using UnityEngine;

namespace Crystal.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SerializeReferenceSelectorAttribute : PropertyAttribute
    {
        public bool DisplayAsDropdown { get; set; } = true;
        public string Title { get; }

        public SerializeReferenceSelectorAttribute()
        {
            Title = null;
        }

        public SerializeReferenceSelectorAttribute(string title)
        {
            Title = title;
        }
    }
}