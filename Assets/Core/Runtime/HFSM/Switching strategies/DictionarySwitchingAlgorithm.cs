using System;
using System.Collections.Generic;

namespace Crystal.HFSM
{
    public sealed class DictionarySwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitcher<TContext> _switcher;
        private IState<TContext> _current;

        private readonly Dictionary<Type, IState<TContext>> _registry = new();

        public IState<TContext> Current => _current;

        public DictionarySwitchingAlgorithm<TContext> RegisterState(IState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
            return this;
        }

        public void StartWith<TState>() where TState : IState<TContext>
        {
            if (_registry.TryGetValue(typeof(TState), out var state))
            {
                _current = state;
                _current.Entry(_context);
            }
        }

        public void Initialize(TContext context, IStateSwitcher<TContext> switcher)
        {
            _context = context;
            _switcher = switcher;
        }

        public void Update()
        {
            if (_current == null) return;

            if (_current is State<TContext> polymorphicState)
            {
                var transitions = polymorphicState.Transitions;
                for (int i = 0; i < transitions.Count; i++)
                {
                    var transition = transitions[i];
                    if (transition.ToTargetState(_context) && transition.TargetState != null)
                    {
                        PerformTransition(transition.TargetState);
                        break;
                    }
                }
            }

            _current.Update(_context);
        }

        public void FixedUpdate() => _current?.FixedUpdate(_context);
        public void LateUpdate() => _current?.LateUpdate(_context);

        public void ExecuteSwitch<TState>() where TState : IState<TContext>
        {
            if (_registry.TryGetValue(typeof(TState), out var newState))
            {
                PerformTransition(newState);
            }
        }

        private void PerformTransition(IState<TContext> newState)
        {
            if (newState == null || _current == newState) return;

            _current?.Exit(_context);
            _current = newState;
            _current.Entry(_context);
        }
    }
}