using System;
using UnityEngine;

namespace Crystal.HFSM
{
    public sealed class StateMachine<TContext> : IStateSwitcher<TContext> where TContext : class
    {
        private readonly TContext _context;
        private readonly ISwitchingAlgorithm<TContext> _algorithm;

        public IState<TContext> ActiveState => _algorithm.Current;

        public StateMachine(TContext context, ISwitchingAlgorithm<TContext> algorithm)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _algorithm.Initialize(_context, this);
        }

        public void Update() => _algorithm.Update();
        public void FixedUpdate() => _algorithm.FixedUpdate();
        public void LateUpdate() => _algorithm.LateUpdate();

        public void SwitchTo<TState>() where TState : IState<TContext>
        {
            _algorithm.ExecuteSwitch<TState>();
        }
    }
}