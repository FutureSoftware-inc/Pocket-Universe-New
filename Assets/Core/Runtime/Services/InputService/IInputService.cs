using UnityEngine;
#if USE_UNITASK
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
#else
using ASYNC_TASK = System.Threading.Tasks.Task;
#endif

namespace CrystalEngine.Services
{
    public interface IInputService
    {
        void Lock(string token);
        void Unlock(string token);
        bool IsLocked { get; }
        void SetContext(string contextName);
        bool IsActionBuffered(string actionName, float bufferTime = 0.2f);
        void ClearBuffer();
        ASYNC_TASK WaitForAnyInputAsync();
        ASYNC_TASK StartRebindingAsync(string actionName, int bindingIndex = 0);
        void ResetAllBindings();
    }
}
