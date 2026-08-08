using UnityEngine;

namespace CrystalEngine.DI.Tests
{
    public class GlobalProjectInstaller : MonoInstaller
    {
        // --- ГЛОБАЛЬНЫЙ ИНТЕРФЕЙС И СЕРВИС ---
        public interface IGlobalAnalytics { void SendEvent(string eventName); }

        public class UnityGlobalAnalytics : IGlobalAnalytics
        {
            public void SendEvent(string eventName) =>
                Debug.Log($"<color=orange>[Global Analytics] Событие отправлено: {eventName}</color>");
        }
        protected override void InstallBindings()
        {
            // Регистрируем сервис аналитики как глобальный Синглтон
            Container.Bind<IGlobalAnalytics>().To<UnityGlobalAnalytics>().AsSingle();
            Debug.Log("[Global Installer] Глобальные зависимости успешно привязаны.");
        }
    }
}