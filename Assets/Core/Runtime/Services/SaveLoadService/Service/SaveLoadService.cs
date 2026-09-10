using System;
using System.Collections.Generic;
using System.Reflection;
#if USE_UNITASK
using ASYNC_TASK = Cysharp.Threading.Tasks.UniTask;
using Cysharp.Threading.Tasks;
#else
using ASYNC_TASK = System.Threading.Tasks.Task;
using System.Threading.Tasks;
#endif

namespace CrystalEngine.Services
{
    internal sealed class SaveLoadService : ISaveLoadService
    {
        private readonly SaveLoadMetaDataCache _metaDataCache;
        private readonly Dictionary<SerializationFormat, ISerializationStrategy> _serializationStrategies = new();
        private readonly Dictionary<SaveContext, IDataStorageService> _storageRouteMap = new();
        private readonly List<ISaveableDataProvider> _providers = new();
        private readonly object _providersLock = new();
        private SerializationFormat _currentFormat = SerializationFormat.Json;

        public SaveLoadService(SaveLoadMetaDataCache metaDataCache, IReadOnlyList<ISerializationStrategy> strategies)
        {
            _metaDataCache = metaDataCache ?? throw new ArgumentNullException(nameof(metaDataCache));
            if (strategies != null)
            {
                for (int i = 0; i < strategies.Count; i++)
                {
                    var strategy = strategies[i];
                    if (strategy is JsonSerializationStrategy) _serializationStrategies[SerializationFormat.Json] = strategy;
                    else if (strategy is XmlSerializationStrategy) _serializationStrategies[SerializationFormat.Xml] = strategy;
                    else if (strategy is BinarySerializationStrategy) _serializationStrategies[SerializationFormat.Binary] = strategy;
                }
            }
            _serializationStrategies.TryAdd(SerializationFormat.Json, new JsonSerializationStrategy());
            _serializationStrategies.TryAdd(SerializationFormat.Xml, new XmlSerializationStrategy());
            _serializationStrategies.TryAdd(SerializationFormat.Binary, new BinarySerializationStrategy());
        }

        public void RegisterProvider(ISaveableDataProvider provider)
        {
            lock (_providersLock) _providers.Add(provider);
        }

        public void UnregisterProvider(ISaveableDataProvider provider)
        {
            lock (_providersLock) _providers.Remove(provider);
        }

        public void ConfigureStorageRoute(SaveContext context, IDataStorageService storageService)
        {
            _storageRouteMap[context] = storageService;
        }

        public void SetDefaultFormat(SerializationFormat format)
        {
            _currentFormat = format;
        }

        public async ASYNC_TASK SaveContextAsync(SaveContext context, string slotName)
        {
            if (!_storageRouteMap.TryGetValue(context, out IDataStorageService storageService))
            {
                throw new Exception($"[SaveLoad Error] Для контекста {context} не настроен маршрут хранения данных!");
            }
            Dictionary<string, Dictionary<string, object>> globalStateGraph = new();
            lock (_providersLock)
            {
                for (int i = 0; i < _providers.Count; i++)
                {
                    ISaveableDataProvider provider = _providers[i];
                    if (provider.Context != context) continue;
                    globalStateGraph[provider.DataKey] = ExtractProviderData(provider);
                }
            }

            ISerializationStrategy strategy = _serializationStrategies[_currentFormat];

#if USE_UNITASK
            // Выносим в фоновый поток только сериализацию байт и дисковый IO
            await UniTask.RunOnThreadPool(async () =>
            {
                byte[] rawData = strategy.Serialize(globalStateGraph);
                await storageService.SaveBytesAsync(slotName, rawData);
            });
#else
            await Task.Run(async () =>
            {
                ISerializationStrategy strategy = _serializationStrategies[_currentFormat];
                byte[] rawData = strategy.Serialize(globalStateGraph);
                await storageService.SaveBytesAsync(slotName, rawData);
            });
#endif
        }

        public async ASYNC_TASK LoadContextAsync(SaveContext context, string slotName)
        {
            if (!_storageRouteMap.TryGetValue(context, out IDataStorageService storageService))
            {
                throw new Exception($"[SaveLoad Error] Для контекста {context} не настроен маршрут загрузки данных!");
            }
            if (!storageService.Exists(slotName)) return;

            Dictionary<string, Dictionary<string, object>> globalStateGraph = null;
            ISerializationStrategy strategy = _serializationStrategies[_currentFormat];

#if USE_UNITASK
            await UniTask.RunOnThreadPool(async () =>
            {
                byte[] rawData = await storageService.LoadBytesAsync(slotName);
                globalStateGraph = strategy.Deserialize(rawData);
            });
#else
            await Task.Run(async () =>
            {
                byte[] rawData = await storageService.LoadBytesAsync(slotName);
                globalStateGraph = strategy.Deserialize(rawData);
            });
#endif

            if (globalStateGraph == null) return;
            lock (_providersLock)
            {
                for (int i = 0; i < _providers.Count; i++)
                {
                    ISaveableDataProvider provider = _providers[i];
                    if (provider.Context != context) continue;
                    if (!globalStateGraph.TryGetValue(provider.DataKey, out Dictionary<string, object> providerData)) continue;
                    ApplyProviderData(provider, providerData);
                }
            }
        }

        public void SaveEntity(ISaveableDataProvider provider)
        {
            if (!_storageRouteMap.TryGetValue(provider.Context, out IDataStorageService storageService))
            {
                throw new Exception($"[SaveLoad Error] Для контекста {provider.Context} не настроен маршрут хранения данных!");
            }
            Dictionary<string, Dictionary<string, object>> singleEntityGraph = new()
            {
                [provider.DataKey] = ExtractProviderData(provider)
            };
            byte[] rawData = _serializationStrategies[_currentFormat].Serialize(singleEntityGraph);
            _ = storageService.SaveBytesAsync($"entity_{provider.DataKey}.dat", rawData);
        }

        public void LoadEntity(ISaveableDataProvider provider)
        {
            if (!_storageRouteMap.TryGetValue(provider.Context, out IDataStorageService storageService))
            {
                throw new Exception($"[SaveLoad Error] Для контекста {provider.Context} не настроен маршрут загрузки данных!");
            }
            string entitySlotName = $"entity_{provider.DataKey}.dat";
            if (!storageService.Exists(entitySlotName)) return;
            byte[] rawData;
#if USE_UNITASK
            rawData = UniTask.RunOnThreadPool(async () => await storageService.LoadBytesAsync(entitySlotName)).GetAwaiter().GetResult();
#else
            rawData = Task.Run(async () => await storageService.LoadBytesAsync(entitySlotName)).GetAwaiter().GetResult();
#endif

            Dictionary<string, Dictionary<string, object>> globalStateGraph = _serializationStrategies[_currentFormat].Deserialize(rawData);
            if (globalStateGraph == null || !globalStateGraph.TryGetValue(provider.DataKey, out Dictionary<string, object> providerData)) return;
            ApplyProviderData(provider, providerData);
        }

        private Dictionary<string, object> ExtractProviderData(ISaveableDataProvider provider)
        {
            IReadOnlyList<FieldInfo> fields = _metaDataCache.GetSerializableFields(provider.GetType());
            IReadOnlyList<PropertyInfo> properties = _metaDataCache.GetSerializableProperties(provider.GetType());
            Dictionary<string, object> providerData = new();
            for (int f = 0; f < fields.Count; f++)
            {
                FieldInfo field = fields[f];
                providerData[field.Name] = field.GetValue(provider);
            }
            for (int p = 0; p < properties.Count; p++)
            {
                PropertyInfo property = properties[p];
                providerData[property.Name] = property.GetValue(provider);
            }
            return providerData;
        }

        private void ApplyProviderData(ISaveableDataProvider provider, Dictionary<string, object> providerData)
        {
            IReadOnlyList<FieldInfo> fields = _metaDataCache.GetSerializableFields(provider.GetType());
            IReadOnlyList<PropertyInfo> properties = _metaDataCache.GetSerializableProperties(provider.GetType());
            for (int f = 0; f < fields.Count; f++)
            {
                FieldInfo field = fields[f];
                if (providerData.TryGetValue(field.Name, out object savedValue))
                {
                    field.SetValue(provider, savedValue);
                }
            }
            for (int p = 0; p < properties.Count; p++)
            {
                PropertyInfo property = properties[p];
                if (providerData.TryGetValue(property.Name, out object savedValue))
                {
                    property.SetValue(provider, savedValue);
                }
            }
        }
    }
}