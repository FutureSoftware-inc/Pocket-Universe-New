using System;
using UnityEngine;

namespace CrystalEngine
{
    [Serializable]
    public sealed class UUID : ISerializationCallbackReceiver, IEquatable<UUID>
    {
        [SerializeField] private string _value;
        private int _hashCache;

        public string Value => _value ?? string.Empty;

        public int HashCache => _hashCache == 0 ? (_hashCache = Value.GetHashCode()) : _hashCache;

        public UUID()
        {
            _value = Guid.NewGuid().ToString("D");
            _hashCache = _value.GetHashCode();
        }

        public UUID(string value)
        {
            _value = !string.IsNullOrEmpty(value) ? value : Guid.NewGuid().ToString("D");
            _hashCache = _value.GetHashCode();
        }

        public override int GetHashCode() => HashCache;

        public override bool Equals(object obj) => obj is UUID other && Equals(other);

        public bool Equals(UUID other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _hashCache == other._hashCache && Value == other.Value; // Сначала быстрое сравнение по хэшу
        }

        public override string ToString() => Value;

        public void OnBeforeSerialize()
        {
            if (string.IsNullOrEmpty(_value))
            {
                _value = Guid.NewGuid().ToString("D");
            }
            _hashCache = _value.GetHashCode();
        }

        public void OnAfterDeserialize()
        {
            _hashCache = Value.GetHashCode();
        }

        public static bool operator ==(UUID left, UUID right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(UUID left, UUID right) => !(left == right);
    }
}