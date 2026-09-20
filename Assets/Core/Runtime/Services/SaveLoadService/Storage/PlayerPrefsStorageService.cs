using System;
using UnityEngine;
#if USE_UNITASK
using Cysharp.Threading.Tasks;
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
using ASYNC_TASK_BYTES = Cysharp.Threading.Tasks.UniTask<byte[]>;
#else
using System.Threading.Tasks;
using ASYNC_TASK = System.Threading.Tasks.Task;
using ASYNC_TASK_BYTES = System.Threading.Tasks.Task<byte[]>;
#endif

namespace CrystalEngine.Services
{
    internal sealed class PlayerPrefsStorageService : IDataStorageService
    {
        public bool Exists(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
        public ASYNC_TASK SaveBytesAsync(string key, byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                string base64String = Convert.ToBase64String(data);
                PlayerPrefs.SetString(key, base64String);
                PlayerPrefs.Save();
            }
#if USE_UNITASK
            return UniTask.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
        public ASYNC_TASK_BYTES LoadBytesAsync(string key)
        {
            if (!PlayerPrefs.HasKey(key))
            {
#if USE_UNITASK
                return UniTask.FromResult(Array.Empty<byte>());
#else
                return Task.FromResult(Array.Empty<byte>());
#endif
            }
            string base64String = PlayerPrefs.GetString(key);
            byte[] bytes = Convert.FromBase64String(base64String);
#if USE_UNITASK
            return UniTask.FromResult(bytes);
#else
            return Task.FromResult(bytes);
#endif
        }
    }
}