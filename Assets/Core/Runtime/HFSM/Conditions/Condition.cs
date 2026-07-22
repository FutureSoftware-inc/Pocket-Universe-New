using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crystal.HFSM
{
    [Serializable]
    public abstract class Condition<TContext> where TContext : class
    {
        [FormerlySerializedAs("_propertyName")]
        [InspectorName("Property Name")]
        [SerializeField] private string _propertyName = "Property name";
        [SerializeField] private bool _invert = false;

        public string PropertyName => _propertyName;

        protected abstract bool Evaluate(TContext context);

        public bool Check(TContext context)
        {
            return _invert ? !Evaluate(context) : Evaluate(context);
        }
    }
}
