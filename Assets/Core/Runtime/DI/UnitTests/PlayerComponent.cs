using UnityEngine;

namespace CrystalEngine.DI.Tests
{
    public class PlayerComponent : MonoBehaviour
    {
        private IAudioService _audioService;
        private IScoreManager _scoreManager;
        private bool _isInitialized;

        [Inject]
        private void Construct(IAudioService audioService, IScoreManager scoreManager)
        {
            _audioService = audioService;
            _scoreManager = scoreManager;
            _isInitialized = true;

            Debug.Log("<color=cyan>[DI Test] Метод Construct в PlayerComponent успешно вызван!</color>");
        }

        public void CollectCoin()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[DI TEST FAILED] PlayerComponent не был проинициализирован через Construct!");
                return;
            }

            Debug.Log("[Gameplay] Игрок подобрал монетку.");
            _audioService.PlaySound("Coin_Pickup.wav");
            _scoreManager.AddScore(10);
        }
    }
}