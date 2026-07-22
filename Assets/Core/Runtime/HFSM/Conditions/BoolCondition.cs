using System;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    [Serializable]
    public sealed class BoolCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        [SerializeField] private bool _expectedValue = false;

        protected override bool Evaluate(TContext context)
        {
            bool currentValue = context.Blackboard.Get<bool>(PropertyName);
            return currentValue == _expectedValue;
        }
    }
}