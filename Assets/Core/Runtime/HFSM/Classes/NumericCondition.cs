using System;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    [SubclassPath("HFSM/Condition/Numeric")]
    public class NumericCondition<TContext> : Condition<TContext> where TContext : class
    {
        [SerializeField] private AnyNumber _selecctionValue;
        [SerializeField] private ComparisonType _comprarisonType = ComparisonType.None;
        private Func<TContext, IComparable> _valueSelector;

        public void Initialize(Func<TContext, IComparable> valueSelector)
        {
            _valueSelector = valueSelector;
        }
        protected override bool Evaluate(TContext context)
        {
            if (_valueSelector == null)
            {
                return false;
            }
            if (_comprarisonType == ComparisonType.None)
            {
                return false;
            }
            IComparable currentValue = _valueSelector(context);
            if (currentValue == null)
            {
                return false;
            }
            int compareResult = _selecctionValue.CompareTo(currentValue);
            ComparisonType currentFrameResultBit = compareResult switch
            {
                > 0 => ComparisonType.Less,
                0 => ComparisonType.Equal,
                < 0 => ComparisonType.Greater
            };
            return (_comprarisonType & currentFrameResultBit) != ComparisonType.None;
        }
    }
}