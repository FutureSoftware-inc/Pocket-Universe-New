#if USE_UNITASK
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
using ASYNC_TASK_BYTES = Cysharp.Threading.Tasks.UniTask<byte[]>;
using Cysharp.Threading.Tasks;
#else
using ASYNC_TASK = System.Threading.Tasks.Task;
using ASYNC_TASK_BYTES = System.Threading.Tasks.Task<byte[]>;
using System.Threading.Tasks;
#endif

using System;
using System.IO;
using UnityEngine;

namespace CrystalEngine.Services
{
    public sealed class LocalFileStorageService : IDataStorageService
    {
        private readonly string _baseStoragePath;

        public LocalFileStorageService()
        {
            _baseStoragePath = Application.persistentDataPath;
        }
        public bool Exists(string key)
        {
            return File.Exists(GetFullPath(key));
        }

        public async ASYNC_TASK SaveBytesAsync(string key, byte[] data)
        {
            string primaryPath = GetFullPath(key);
            string tempPath = primaryPath + ".tmp";
            string backupPath = primaryPath + ".bak";

            try
            {
                using (FileStream stream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
#if USE_UNITASK
                    await stream.WriteAsync(data, 0, data.Length).AsUniTask();
#else
                    await stream.WriteAsync(data, 0, data.Length);
#endif
                }
                if (File.Exists(primaryPath))
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    File.Move(primaryPath, backupPath);
                }
                File.Move(tempPath, primaryPath);
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Storage Error] Ошибка асинхронной записи файла {key}: {ex.Message}");
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
        }

        public async ASYNC_TASK_BYTES LoadBytesAsync(string key)
        {
            string primaryPath = GetFullPath(key);
            string backupPath = primaryPath + ".bak";
            string pathToRead = primaryPath;

            if (!File.Exists(primaryPath) && File.Exists(backupPath))
            {
                Debug.LogWarning($"[Storage Warning] Основной файл {key} поврежден или отсутствует, восстанавливаем из .bak!");
                pathToRead = backupPath;
            }
            if (!File.Exists(pathToRead))
            {
#if USE_UNITASK
                return Array.Empty<byte>();
#else
                return await Task.FromResult(Array.Empty<byte>());
#endif
            }
            try
            {
                using (FileStream stream = new(pathToRead, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    byte[] result = new byte[stream.Length];
#if USE_UNITASK
                    // UniTask под капотом сам гарантирует полное вычитывание буфера
                    await stream.ReadAsync(result, 0, result.Length).AsUniTask();
#else
                    int bytesRead = 0;
                    while (bytesRead < result.Length)
                    {
                        int read = await stream.ReadAsync(result, bytesRead, result.Length - bytesRead);
                        if (read == 0)
                        {
                            break;
                        }
                        bytesRead += read;
                    }
#endif
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Storage Error] Не удалось прочитать файл {key}: {ex.Message}");
                throw;
            }
        }

        private string GetFullPath(string key)
        {
            return Path.Combine(_baseStoragePath, key);
        }
    }
}