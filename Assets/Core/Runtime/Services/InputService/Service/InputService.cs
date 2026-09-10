using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

#if USE_UNITASK
using Cysharp.Threading.Tasks;
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
#else
using System.Threading.Tasks;
using ASYNC_TASK = System.Threading.Tasks.Task;
#endif

namespace CrystalEngine.Services
{
    public sealed class InputService : IInputService, IDisposable
    {
        private readonly InputConfiguration _config;
        private readonly InputActionAsset _actionAsset;
        private readonly ISaveLoadService _saveLoadService;
        private readonly HashSet<string> _lockTokens = new();
        private readonly Dictionary<string, float> _inputBuffer = new(StringComparer.OrdinalIgnoreCase);
        public bool IsLocked => _lockTokens.Count > 0;
        public InputService(InputConfiguration config, ISaveLoadService saveLoadService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
            _actionAsset = _config.ActionAsset;
            if (!_config.UseOldInputManager && _actionAsset != null)
            {
                _actionAsset.Enable();
                foreach (InputActionMap map in _actionAsset.actionMaps)
                {
                    map.actionTriggered += OnActionTriggered;
                }
                _saveLoadService.RegisterProvider(_config);
                _saveLoadService.LoadEntity(_config);
                if (!string.IsNullOrEmpty(_config.RebindsJson))
                {
                    _actionAsset.LoadBindingOverridesFromJson(_config.RebindsJson);
                }
            }
        }
        public void Lock(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            _lockTokens.Add(token);
            EvaluateInputState();
        }
        public void Unlock(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            _lockTokens.Remove(token);
            EvaluateInputState();
        }
        private void EvaluateInputState()
        {
            if (_config.UseOldInputManager || _actionAsset == null) return;
            if (IsLocked)
                _actionAsset.Disable();
            else
                _actionAsset.Enable();
        }
        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            _inputBuffer[context.action.name] = Time.time;
        }
        public bool IsActionBuffered(string actionName, float bufferTime = 0.2f)
        {
            if (IsLocked || string.IsNullOrEmpty(actionName)) return false;
            if (_config.UseOldInputManager)
            {
                return Input.GetButtonDown(actionName);
            }
            if (_inputBuffer.TryGetValue(actionName, out float lastPressedTime))
            {
                return (Time.time - lastPressedTime) <= bufferTime;
            }
            return false;
        }
        public void ClearBuffer() => _inputBuffer.Clear();
        public async ASYNC_TASK StartRebindingAsync(string actionName, int bindingIndex = 0)
        {
            if (_config.UseOldInputManager || _actionAsset == null) return;
            InputAction action = _actionAsset.FindAction(actionName);
            if (action == null) return;
            Lock("Rebinding_Process");
            action.Disable();
            var utcs = new UniTaskCompletionSource();
            var operation = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsExcluding("Mouse")
                .OnComplete(op => { utcs.TrySetResult(); op.Dispose(); })
                .OnCancel(op => { utcs.TrySetResult(); op.Dispose(); });
            operation.Start();
            await utcs.Task;
            action.Enable();
            Unlock("Rebinding_Process");
            _config.RebindsJson = _actionAsset.SaveBindingOverridesAsJson();
            _saveLoadService.SaveEntity(_config);
        }
        public void ResetAllBindings()
        {
            if (_config.UseOldInputManager || _actionAsset == null) return;
            _actionAsset.RemoveAllBindingOverrides();
            _config.RebindsJson = string.Empty;
            _saveLoadService.SaveEntity(_config);
        }
        public void Enable()
        {
            _lockTokens.Clear();
            EvaluateInputState();
        }
        public void Disable()
        {
            Lock("Manual_Global_Disable");
        }
        public void SetContext(string contextName)
        {
            if (IsLocked || _config.UseOldInputManager || _actionAsset == null) return;
            foreach (InputActionMap map in _actionAsset.actionMaps)
            {
                if (map.name.Equals(contextName, StringComparison.OrdinalIgnoreCase))
                    map.Enable();
                else
                    map.Disable();
            }
        }
        public async ASYNC_TASK WaitForAnyInputAsync()
        {
            if (IsLocked) return;
#if USE_UNITASK
            if (_config.UseOldInputManager)
            {
                await UniTask.WaitUntil(() => Input.anyKeyDown);
            }
            else
            {
                await UniTask.WaitUntil(() =>
                    (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                    (Pointer.current != null && Pointer.current.press.wasPressedThisFrame));
            }
#else
            while (!IsLocked)
            {
                if (_config.UseOldInputManager && Input.anyKeyDown) return;
                if (!_config.UseOldInputManager)
                {
                    if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return;
                    if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) return;
                }
                await Task.Delay(16);
            }
#endif
        }
        public void Dispose()
        {
            if (!_config.UseOldInputManager)
            {
                if (_saveLoadService != null && _config != null)
                {
                    _saveLoadService.UnregisterProvider(_config);
                }
                if (_actionAsset != null)
                {
                    foreach (InputActionMap map in _actionAsset.actionMaps)
                    {
                        map.actionTriggered -= OnActionTriggered;
                    }
                    _actionAsset.Disable();
                }
            }
        }
    }
}