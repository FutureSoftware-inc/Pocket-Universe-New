using System;
using UnityEngine;

namespace Crystal.Common
{
    [Serializable]
    public sealed class ReferenceProvider<TInterface> where TInterface : class
    {
        [SerializeField] private UnityEngine.Object _targetObject;
        [SerializeField] private string _interfaceTypeName;

        private TInterface _cachedReference;        

        public ReferenceProvider(TInterface target)
        {
            if (target is UnityEngine.Object obj)
            {
                _targetObject = obj;
                _interfaceTypeName = typeof(TInterface).AssemblyQualifiedName;
            }
        }

        public TInterface Value
        {
            get
            {
                if (_cachedReference == null && _targetObject != null)
                {
                    _cachedReference = _targetObject as TInterface;
                    if (_cachedReference == null && _targetObject is GameObject go)
                    {
                        _cachedReference = go.GetComponent<TInterface>();
                    }
                }
                return _cachedReference;
            }
        }
    }
}
