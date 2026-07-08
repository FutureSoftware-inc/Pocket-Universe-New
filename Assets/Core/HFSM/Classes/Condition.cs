using System;
using UnityEngine;

namespace Crystal.HFSM
{
    [Serializable]
    public abstract class Condition<TContext> where TContext : class
    {
        [SerializeField] private string _propertyName = "Property name";
        [SerializeField] private bool _invert = false;

        protected abstract bool Evaluate(TContext context);
        public bool Check(TContext context)
        {
            return _invert ? !Evaluate(context) : Evaluate(context);
        }
    }
}