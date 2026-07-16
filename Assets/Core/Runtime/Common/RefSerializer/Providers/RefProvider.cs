using System;
using UnityEngine;

namespace Crystal.Common
{
    [Serializable]
    public sealed class RefProvider<TRefType> : IRefProvider<TRefType> where TRefType : class
    {
        [SerializeField] private UnityEngine.Object _host;
        [SerializeField] private long _id;
        [NonSerialized] private WeakReference<TRefType> _cache;

        public UnityEngine.Object Host => _host;
        public long ID => _id;
        public bool IsValid => _host != null && _id != 0;

        internal RefProvider(UnityEngine.Object host, long id)
        {
            _host = host;
            _id = id;
        }

        public TRefType GetRef()
        {
            return RefProvideHandler.GetRef(_host, _id, ref _cache);
        }

        public RefProvider<TRefType> CopyWithNewHost(UnityEngine.Object host)
        {
            return new RefProvider<TRefType>(host, _id);
        }

        public static implicit operator TRefType(RefProvider<TRefType> reference)
        {
            return reference?.GetRef();
        }
    }

    [Serializable]
    public sealed class RefProvider<TRefType, THostType> : IRefProvider<TRefType> where TRefType : class where THostType : UnityEngine.Object
    {
        [SerializeField] private THostType _host;
        [SerializeField] private long _id;
        [NonSerialized] private WeakReference<TRefType> _cache;

        public THostType Host => _host;
        UnityEngine.Object IRefProvider<TRefType>.Host => _host;
        public long ID => _id;
        public bool IsValid => _host != null && _id != 0;

        internal RefProvider(THostType host, long id)
        {
            _host = host;
            _id = id;
        }

        public TRefType GetRef()
        {
            return RefProvideHandler.GetRef(_host, _id, ref _cache);
        }

        public RefProvider<TRefType, THostType> CopyWithNewHost(THostType host)
        {
            return new RefProvider<TRefType, THostType>(host, _id);
        }

        public static implicit operator TRefType(RefProvider<TRefType, THostType> reference)
        {
            return reference?.GetRef();
        }
    }
}