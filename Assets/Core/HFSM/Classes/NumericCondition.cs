using System;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    public class NumericCondition<TContext> : Condition<TContext> where TContext : class
    {
        [SerializeField] private NumericType _numericType = NumericType.Byte;
        [SerializeField] private ComprassionType _comprassionType = ComprassionType.None;
        protected override bool Evaluate(TContext context)
        {
            throw new System.NotImplementedException();
            
        }
    }
}