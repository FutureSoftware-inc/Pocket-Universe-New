using System;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    public sealed class NumericCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        [SerializeField] private AnyNumber _selectionValue;
        [SerializeField] private ComparisonType _comparisonType = ComparisonType.None;

        protected override bool Evaluate(TContext context)
        {
            if (_comparisonType == ComparisonType.None) return false;
            AnyNumber currentValue = GetValueFromBlackboard(context.Blackboard);
            return _comparisonType switch
            {
                ComparisonType.Equal => _selectionValue == currentValue,
                ComparisonType.Less => _selectionValue < currentValue,
                ComparisonType.Greater => _selectionValue > currentValue,
                ComparisonType.Less | ComparisonType.Equal => _selectionValue <= currentValue,
                ComparisonType.Greater | ComparisonType.Equal => _selectionValue >= currentValue,
                _ => false
            };
        }

        private AnyNumber GetValueFromBlackboard(Blackboard blackboard)
        {
            return _selectionValue.CurrentType switch
            {
                NumericType.SByte => blackboard.Get<sbyte>(PropertyName),
                NumericType.Byte => blackboard.Get<byte>(PropertyName),
                NumericType.Int16 => blackboard.Get<short>(PropertyName),
                NumericType.UInt16 => blackboard.Get<ushort>(PropertyName),
                NumericType.Int32 => blackboard.Get<int>(PropertyName),
                NumericType.UInt32 => blackboard.Get<uint>(PropertyName),
                NumericType.Int64 => blackboard.Get<long>(PropertyName),
                NumericType.UInt64 => blackboard.Get<ulong>(PropertyName),
                NumericType.Single => blackboard.Get<float>(PropertyName),
                NumericType.Double => blackboard.Get<double>(PropertyName),
                _ => blackboard.Get<float>(PropertyName)
            };
        }
    }
}