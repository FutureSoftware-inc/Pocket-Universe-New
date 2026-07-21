using System;
using System.Collections.Generic;

namespace Crystal.HFSM
{
    public sealed class Blackboard
    {
        private static class Storage<T>
        {
            public static readonly Dictionary<Blackboard, Dictionary<string, T>> Data = new();

            public static Dictionary<string, T> GetRegistry(Blackboard instance)
            {
                if (!Data.TryGetValue(instance, out var registry))
                {
                    registry = new Dictionary<string, T>();
                    Data[instance] = registry;
                }
                return registry;
            }
        }

        public void Set<T>(string key, T value)
        {
            Storage<T>.GetRegistry(this)[key] = value;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            var registry = Storage<T>.GetRegistry(this);
            return registry.TryGetValue(key, out T value) ? value : defaultValue;
        }

        public bool HasKey<T>(string key)
        {
            return Storage<T>.GetRegistry(this).ContainsKey(key);
        }

        public void ClearAll()
        {
            // Метод для очистки кэша при необходимости, чтобы не копить ссылки в Storage<T>
        }
    }
}
