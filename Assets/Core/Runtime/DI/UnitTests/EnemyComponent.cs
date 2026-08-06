using UnityEngine;

namespace CrystalEngine.DI.Tests
{
    public class EnemyComponent : MonoBehaviour
    {
        [Inject] private IAudioService _audioService;

        public void Die()
        {
            Debug.Log("[Gameplay] Враг побежден!");
            if (_audioService != null)
            {
                _audioService.PlaySound("Enemy_Death.wav");
            }
            else
            {
                Debug.LogError("[DI TEST FAILED] Поле _audioService в EnemyComponent пустует!");
            }
        }
    }
}