using UnityEngine;

namespace CrystalEngine.DI.Tests
{
    // --- ИНТЕРФЕЙСЫ И СЕРВИСЫ ---
    public interface IAudioService { void PlaySound(string soundName); }
    public interface IScoreManager { void AddScore(int points); }

    public class UnityAudioService : IAudioService
    {
        public void PlaySound(string soundName) => Debug.Log($"[DI Audio] Воспроизведение звука: {soundName}");
    }

    public class GameplayScoreManager : IScoreManager
    {
        private int _score;
        public void AddScore(int points)
        {
            _score += points;
            Debug.Log($"[DI Score] Очки изменены! Всего: {_score}");
        }
    }
}
