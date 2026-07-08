using Crystal.Common;
using System;
using UnityEngine;

namespace Crystal.HFSM
{
    [Serializable]
    public class BoolCondition<TContext> : Condition<TContext> where TContext : class
    {
        [SerializeField] private bool _expectedValue = false;

        private Func<TContext, bool> _valueSelector;

        public void Initialize(Func<TContext, bool> selector)
        {
            _valueSelector = selector;
        }
        protected override bool Evaluate(TContext context)
        {
            if (_valueSelector == null)
            {
                return false;
            }
            return _valueSelector(context) == _expectedValue;
        }
    }
}