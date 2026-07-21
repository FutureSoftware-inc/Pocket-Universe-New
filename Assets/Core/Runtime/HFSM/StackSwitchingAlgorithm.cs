using System;
using System.Collections.Generic;

namespace Crystal.HFSM
{
    public sealed class StackSwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitсher<TContext> _switcher;

        private readonly Stack<IState<TContext>> _stateStack = new();
        private readonly Dictionary<Type, IState<TContext>> _registry = new();

        public IState<TContext> Current => _stateStack.Count > 0 ? _stateStack.Peek() : null;

        public StackSwitchingAlgorithm<TContext> RegisterState(IState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
            return this;
        }

        public void Initialize(TContext context, IStateSwitсher<TContext> switcher)
        {
            _context = context;
            _switcher = switcher;
        }

        public void PushState<TState>() where TState : IState<TContext>
        {
            if (!_registry.TryGetValue(typeof(TState), out var newState)) return;

            Current?.Exit(_context);
            _stateStack.Push(newState);
            newState.Entry(_context);
        }

        public void PopState()
        {
            if (_stateStack.Count <= 1) return;

            var removedState = _stateStack.Pop();
            removedState.Exit(_context);

            Current?.Entry(_context);
        }

        public void Update()
        {
            IState<TContext> active = Current;
            if (active == null) return;

            if (active is State<TContext> polymorphicState)
            {
                var transitions = polymorphicState.Transitions;
                for (int i = 0; i < transitions.Count; i++)
                {
                    var transition = transitions[i];
                    if (transition.ToTargetState(_context) && transition.TargetState != null)
                    {
                        ExecuteTransition(transition.TargetState);
                        break;
                    }
                }
            }

            Current?.Update(_context);
        }

        public void FixedUpdate() => Current?.FixedUpdate(_context);
        public void LateUpdate() => Current?.LateUpdate(_context);

        public void ExecuteSwitch<TState>() where TState : IState<TContext>
        {
            if (!_registry.TryGetValue(typeof(TState), out var newState)) return;
            ExecuteTransition(newState);
        }

        private void ExecuteTransition(IState<TContext> newState)
        {
            if (newState == null || Current == newState) return;

            Current?.Exit(_context);

            if (_stateStack.Count > 0) _stateStack.Pop();
            _stateStack.Push(newState);

            newState.Entry(_context);
        }
    }
}