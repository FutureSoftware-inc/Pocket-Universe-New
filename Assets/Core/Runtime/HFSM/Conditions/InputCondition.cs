using System;
using UnityEngine;
using CrystalEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Конкретная реализация условия, проверяющая пользовательский ввод (нажатие клавиш) через стандартную систему Input в Unity.
    /// <br/><br/>
    /// A concrete condition implementation that evaluates user input (key presses) using Unity's standard Input system.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    [Serializable]
    [SelectorTooltip("Проверяет, нажата ли определенная клавиша на клавиатуре в текущем кадре.")]
    public sealed class InputCondition<TContext> : Condition<TContext> where TContext : class, IBlackboardProvider
    {
        /// <summary>
        /// Клавиша на клавиатуре или кнопка мыши, состояние которой необходимо проверить.
        /// <br/><br/>
        /// The keyboard key or mouse button whose state needs to be checked.
        /// </summary>
        [SerializeField] private KeyCode _key = KeyCode.Space;

        /// <summary>
        /// Тип проверки ввода (нажатие, удержание или отпускание клавиши).
        /// <br/><br/>
        /// The type of input check (key down, pressed, or key up).
        /// </summary>
        [SerializeField] private InputCheckType _checkType = InputCheckType.Down;

        /// <summary>
        /// Опрашивает состояние заданной клавиши в зависимости от выбранного типа проверки ввода.
        /// <br/><br/>
        /// Queries the state of the specified key depending on the selected input check type.
        /// </summary>
        /// <param name="context">Контекст данных машины состояний.<br/><br/>The data context of the state machine.</param>
        /// <returns>True, если состояние клавиши соответствует выбранному типу проверки; иначе false.<br/><br/>True if the key state matches the selected check type; otherwise, false.</returns>
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
