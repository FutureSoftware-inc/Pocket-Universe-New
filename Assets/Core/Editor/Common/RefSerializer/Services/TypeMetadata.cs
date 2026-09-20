using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngineEditor
{
    public sealed class TypeMetadata
    {
        public Type Type { get; }
        public string Name { get; }

        private readonly Dictionary<Type, Attribute> _attributes = new();

        public TypeMetadata(Type type, string name, IEnumerable<Attribute> attributes)
        {
            Type = type ?? throw new ArgumentException(nameof(type));
            Name = !string.IsNullOrEmpty(name) ? name : type.Name;
            foreach (Attribute attribute in attributes)
            {
                if (attribute != null)
                {
                    _attributes[attribute.GetType()] = attribute;
                }
            }
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return _attributes.TryGetValue(typeof(T), out Attribute attribute) ? (T)attribute : null; 
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            return _attributes.ContainsKey(typeof(T));
        }
    }
}