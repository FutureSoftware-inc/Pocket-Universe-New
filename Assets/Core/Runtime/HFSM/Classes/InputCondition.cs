using System;
using UnityEngine;
using Crystal.Common;

namespace Crystal.HFSM
{
    [Serializable]
    [SelectorTooltip("Проверяет, нажата ли определенная клавиша на клавиатуре в текущем кадре.")]
    public sealed class InputCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        [SerializeField] private KeyCode _key = KeyCode.Space;
        [SerializeField] private InputCheckType _checkType = InputCheckType.Down;

        protected override bool Evaluate(TContext context)
        {
            return _checkType switch
            {
                InputCheckType.Down => Input.GetKeyDown(_key),
                InputCheckType.Pressed => Input.GetKey(_key),
                InputCheckType.Up => Input.GetKeyUp(_key),
                _ => false
            };
        }
    }
}