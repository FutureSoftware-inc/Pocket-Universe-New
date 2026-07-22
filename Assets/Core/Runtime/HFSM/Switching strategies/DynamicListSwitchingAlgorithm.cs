using System;
using System.Collections.Generic;

namespace Crystal.HFSM
{
    public sealed class DynamicListSwitchingAlgorithm<TContext> : ISwitchingAlgorithm<TContext> where TContext : class
    {
        private TContext _context;
        private IStateSwitcher<TContext> _switcher;
        private IState<TContext> _current;

        // Глобальный упорядоченный список правил переключения
        private readonly List<DynamicRule> _rules = new();
        private readonly Dictionary<Type, IState<TContext>> _registry = new();

        public IState<TContext> Current => _current;

        // Внутренний контейнер для хранения приоритетного правила
        private struct DynamicRule
        {
            public IState<TContext> TargetState;
            public List<Condition<TContext>> Conditions;
        }

        public DynamicListSwitchingAlgorithm<TContext> RegisterState(IState<TContext> state)
        {
            if (state != null) _registry[state.GetType()] = state;
            return this;
        }

        // Метод Fluent API для выстраивания цепочки приоритетов сверху вниз
        public DynamicListSwitchingAlgorithm<TContext> AddPriorityRule<TState>(List<Condition<TContext>> conditions) where TState : IState<TContext>
        {
            if (_registry.TryGetValue(typeof(TState), out var state))
            {
                _rules.Add(new DynamicRule { TargetState = state, Conditions = conditions });
            }
            return this;
        }

        public void Initialize(TContext context, IStateSwitcher<TContext> switcher)
        {
            _context = context;
            _switcher = switcher;
        }

        public void Update()
        {
            // Сквозной ежекадровый опрос правил (Критерий приемки ТЗ: Глобальный приоритет)
            for (int i = 0; i < _rules.Count; i++)
            {
                var rule = _rules[i];
                if (EvaluateConditions(rule.Conditions))
                {
                    PerformTransition(rule.TargetState);
                    break; // Как только верхнее приоритетное правило сработало — прерываем цикл кадра
                }
            }

            _current?.Update(_context);
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

        private bool EvaluateConditions(List<Condition<TContext>> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            for (int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].Check(_context)) return false;
            }
            return true;
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