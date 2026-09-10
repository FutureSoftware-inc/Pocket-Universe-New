using UnityEngine;
using CrystalEngine.Services;

namespace CrystalEngine.DI
{
    /// <summary>
    /// Инсталлер для автоматической сборки и регистрации Системы Ввода в DI-контейнере.
    /// Поддерживает как ручное назначение конфига через инспектор, так и динамическую подгрузку.
    /// </summary>
    [AddComponentMenu("CrystalEngine/DI/Installers/Input System Installer")]
    public sealed class InputSystemInstaller : MonoInstaller
    {
        [Header("Configuration")]
        [Tooltip("Конфигурационный ассет ввода. Если планируется загрузка через Addressables, оставьте это поле пустым.")]
        [SerializeField] private InputConfiguration _configuration;

        /// <summary>
        /// Вызывается DI-контейнером при сборке контекста (ProjectContext или SceneContext).
        /// </summary>
        protected override void InstallBindings()
        {
            // Сценарий 1: Конфигурация задана вручную в инспекторе
            if (_configuration != null)
            {
                // Регистрируем сам конфиг, чтобы к нему был доступ, если понадобится
                Container.BindAsSelf<InputConfiguration>().FromInstance(_configuration);

                // Регистрируем наш сервис ввода по интерфейсу как глобальный синглтон
                Container.BindInterfaces<InputService>().AsSingle();

                Debug.Log("[CrystalEngine] Система ввода успешно инициализирована через Инспектор.");
                return;
            }

            // Сценарий 2: Поле пустое — значит, проект полагается на внешнюю загрузку (Addressables)
            // Мы регистрируем ленивую привязку интерфейсов, но сам конфиг должен быть заинжектен в контейнер ДО вызова Resolve
            Container.BindInterfaces<InputService>().AsSingle();
            Debug.LogWarning("[CrystalEngine] InputConfiguration не задан в инспекторе. Ожидается динамическое внедрение через Addressables перед вызовом зависимостей ввода.");
        }
    }
}