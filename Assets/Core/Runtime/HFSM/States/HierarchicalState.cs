using System;
using UnityEngine;

namespace Crystal.HFSM
{
    [Serializable]
    public abstract class HierarchicalState<TContext> : State<TContext> where TContext : class
    {
        // Внутренний изолированный автомат для вложенных поведений (Sub-FSM)
        private StateMachine<TContext> _subStateMachine;

        protected StateMachine<TContext> SubStateMachine => _subStateMachine;

        // Конструктор заставляет наследника четко определить, 
        // по какому алгоритму (Словарь, Стек, Очередь) будет работать вложенный слой
        protected HierarchicalState(TContext context, ISwitchingAlgorithm<TContext> subAlgorithm)
        {
            if (subAlgorithm == null) throw new ArgumentNullException(nameof(subAlgorithm));

            // Создаем вложенную запечатанную машину состояний, скармливая ей контекст и алгоритм
            _subStateMachine = new StateMachine<TContext>(context, subAlgorithm);
        }

        public override void Entry(TContext context)
        {
            // Сначала выполняем базовые команды входа родительского класса
            base.Entry(context);

            // ВАЖНО: При входе в иерархический стейт, вложенная машина автоматически 
            // запускает свое стартовое под-состояние (Наследование поведения!)
            _subStateMachine.ActiveState?.Entry(context);
        }

        public override void Update(TContext context)
        {
            // Навешиваем логику тиков: сначала обновляется вложенный автомат,
            // прокручивая свои локальные переходы и комбо-цепочки
            _subStateMachine.Update();

            // Затем тикает базовая логика самого родительского состояния
            base.Update(context);
        }

        public override void FixedUpdate(TContext context)
        {
            _subStateMachine.FixedUpdate();
            base.FixedUpdate(context);
        }

        public override void LateUpdate(TContext context)
        {
            _subStateMachine.LateUpdate();
            base.LateUpdate(context);
        }

        public override void Exit(TContext context)
        {
            // ВАЖНО: Если мы покидаем этот глобальный стейт (например, существо умерло),
            // мы принудительно гасим активное вложенное под-состояние, предотвращая зависание логики!
            _subStateMachine.ActiveState?.Exit(context);

            // Выполняем базовые команды выхода родителя
            base.Exit(context);
        }
    }
}
