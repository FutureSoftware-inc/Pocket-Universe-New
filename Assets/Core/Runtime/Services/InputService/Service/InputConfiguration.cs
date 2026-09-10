using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalEngine.Services
{
    [CreateAssetMenu(fileName = "InputConfiguration", menuName = "CrystalEngine/Input/Configuration")]
    public sealed class InputConfiguration : ScriptableObject, ISaveableDataProvider
    {
        [Header("New Input System")]
        [SerializeField] private InputActionAsset _actionAsset;

        [Header("Compatibility Settings")]
        [SerializeField] private bool _useOldInputManager = false;

        [SaveData] private string _rebindsJson;

        public InputActionAsset ActionAsset => _actionAsset;
        public bool UseOldInputManager => _useOldInputManager;
        public string RebindsJson { get => _rebindsJson; set => _rebindsJson = value; }

        public string DataKey => "PlayerInputOverrides";
        public SaveContext Context => SaveContext.LocalProfile;
    }
}