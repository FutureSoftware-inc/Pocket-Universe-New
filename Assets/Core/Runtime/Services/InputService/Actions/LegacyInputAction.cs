using System;
using UnityEngine;

namespace CrystalEngine.Services
{
    [Serializable]
    public sealed class LegacyInputAction : IInputAction
    {
        [SerializeField] private string _buttonName;

        public string Name => _buttonName;

        public bool WasPressedThisFrame() => !string.IsNullOrEmpty(_buttonName) && Input.GetButtonDown(_buttonName);
        public bool IsPressed() => !string.IsNullOrEmpty(_buttonName) && Input.GetButton(_buttonName);
        public bool WasReleasedThisFrame() => !string.IsNullOrEmpty(_buttonName) && Input.GetButtonUp(_buttonName);
    }
}
