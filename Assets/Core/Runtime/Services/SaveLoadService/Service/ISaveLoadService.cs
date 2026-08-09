#if USE_UNITASK
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
#else
using ASYNC_TASK = System.Threading.Tasks.Task;
#endif

namespace CrystalEngine.Services
{
    public interface ISaveLoadService
    {
        void RegisterProvider(ISaveableDataProvider provider);
        void UnregisterProvider(ISaveableDataProvider provider);
        void ConfigureStorageRoute(SaveContext context, IDataStorageService storageService);
        void SetDefaultFormat(SerializationFormat format);

        ASYNC_TASK SaveContextAsync(SaveContext context, string slotName);
        ASYNC_TASK LoadContextAsync(SaveContext context, string slotName);
    }
}
