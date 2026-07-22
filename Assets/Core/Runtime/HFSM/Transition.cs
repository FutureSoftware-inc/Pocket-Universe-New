using System;
using System.Collections.Generic;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    public class Transition<TContext> where TContext : class
    {
        [SerializeField] private string _targetStateName;

        [SerializeReferenceSelector]
        [SerializeField] private List<Condition<TContext>> _conditions = new();

        [SerializeReferenceSelector]
        private IState<TContext> _targetState;

        public string TargetStateName => _targetStateName;
        public IState<TContext> TargetState => _targetState;

        public void Initialize(IState<TContext> targetState)
        {
            _targetState = targetState ?? throw new ArgumentNullException(nameof(targetState));
        }

        public bool ToTargetState(TContext context)
        {
            if (_conditions == null || _conditions.Count == 0) return true;

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (!_conditions[i].Check(context)) return false;
            }
            return true;
        }
    }
}