using System;
using UnityEngine;
using CrystalEngine.Services;

namespace CrystalEngine
{
    [Serializable]
    public sealed class InputCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        // Используем SerializeReference для полиморфной сериализации в инспекторе Unity.
        // Геймдизайнер сможет выбрать в выпадающем списке, какой тип ввода использовать (Old или New).
        [SerializeReferenceSelector]
        [SerializeReference] private IInputAction _inputAction;
        [SerializeField] private InputCheckType _checkType = InputCheckType.Down;

        protected override bool Evaluate(TContext context)
        {
            if (_inputAction == null) return false;

            return _checkType switch
            {
                InputCheckType.Down => _inputAction.WasPressedThisFrame(),
                InputCheckType.Pressed => _inputAction.IsPressed(),
                InputCheckType.Up => _inputAction.WasReleasedThisFrame(),
                _ => false
            };
        }
    }
}
