using CrystalEngine.DI;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.Services
{
    public sealed class SaveLoadSystemInstaller : MonoBehaviour
    {
        [SerializeField] private SerializationFormat _defaultFormat = SerializationFormat.Json;

        public void InstallBindings(DIContainer container)
        {
            container.BindAsSelf<SaveLoadMetaDataCache>().AsSingle();
            container.Bind<IDataStorageService>().To<LocalFileStorageService>().AsSingle();
            switch (_defaultFormat)
            {
                case SerializationFormat.Json:
                    container.Bind<ISerializationStrategy>().To<JsonSerializationStrategy>().AsSingle();
                    break;
                case SerializationFormat.Xml:
                    container.Bind<ISerializationStrategy>().To<XmlSerializationStrategy>().AsSingle();
                    break;
                case SerializationFormat.Binary:
                    container.Bind<ISerializationStrategy>().To<BinarySerializationStrategy>().AsSingle();
                    break;
                default:
                    container.Bind<ISerializationStrategy>().To<JsonSerializationStrategy>().AsSingle();
                    break;
            }

            container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle();
            ISaveLoadService saveLoadService = container.Resolve<ISaveLoadService>();
            IDataStorageService fileStorage = container.Resolve<IDataStorageService>();
            saveLoadService.ConfigureStorageRoute(SaveContext.LocalProfile, fileStorage);
            saveLoadService.ConfigureStorageRoute(SaveContext.PlayerState, fileStorage);
            saveLoadService.ConfigureStorageRoute(SaveContext.WorldState, fileStorage);
            IReadOnlyList<ISaveableDataProvider> allProviders = container.ResolveAll<ISaveableDataProvider>();
            for (int i = 0; i < allProviders.Count; i++)
            {
                saveLoadService.RegisterProvider(allProviders[i]);
            }
        }
    }
}