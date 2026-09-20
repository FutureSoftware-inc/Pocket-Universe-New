using UnityEngine;

namespace CrystalEngine.DI
{
    [AddComponentMenu("Crystal DI/Default Scene Installer")]
    public sealed class DefaultSceneInstaller : MonoInstaller
    {
        protected override void InstallBindings()
        {
            // Этот инсталлер создан автоматически. 
            // Пиши свои Bind-привязки прямо здесь!
            Debug.Log($"[{gameObject.name}] Навешен автоматический инсталлер. Готов к биндингам.");
        }
    }
}
