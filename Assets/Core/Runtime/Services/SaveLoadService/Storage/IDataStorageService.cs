#if USE_UNITASK
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
using ASYNC_TASK_BYTES = Cysharp.Threading.Tasks.UniTask<byte[]>;
using Cysharp.Threading.Tasks;
#else
using ASYNC_TASK = System.Threading.Tasks.Task;
using ASYNC_TASK_BYTES = System.Threading.Tasks.Task<byte[]>;
using System.Threading.Tasks;
#endif

namespace CrystalEngine.Services
{
    /// <summary>
    /// Интерфейс низкоуровневого ввода-вывода данных. 
    /// Полностью изолирует логику сохранения от физического носителя (диск, сеть, RAM).
    /// </summary>
    public interface IDataStorageService
    {
        ASYNC_TASK SaveBytesAsync(string key, byte[] data);
        ASYNC_TASK_BYTES LoadBytesAsync(string key);
        bool Exists(string key);
    }
}