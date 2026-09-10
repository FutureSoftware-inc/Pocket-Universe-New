using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalEngine.Services
{
    [Serializable]
    public sealed class InputSystemAction : IInputAction
    {
        [SerializeField] private InputActionReference _actionReference;

        public string Name => _actionReference != null ? _actionReference.action.name : string.Empty;

        public bool WasPressedThisFrame() => _actionReference != null && _actionReference.action.WasPressedThisFrame();
        public bool IsPressed() => _actionReference != null && _actionReference.action.IsPressed();
        public bool WasReleasedThisFrame() => _actionReference != null && _actionReference.action.WasReleasedThisFrame();
    }
}
