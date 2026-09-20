using UnityEngine;

namespace CrystalEngine.DI.Tests
{
    public class DiStageFourLauncher : MonoBehaviour
    {
        [SerializeField] private StageFourPlayer _player;

        private void Start()
        {
            Debug.Log("<color=yellow>=== ЗАПУСК ПРОВЕРКИ ИЕРАРХИИ (ЭТАП 4) ===</color>");

            if (_player == null)
            {
                Debug.LogError("Перетащите ссылку на StageFourPlayer в инспектор скрипта DiStageFourLauncher!");
                return;
            }

            _player.StartGame();

            Debug.Log("<color=green>=== ПРОВЕРКА ЭТАПА 4 ЗАВЕРШЕНА УСПЕШНО ===</color>");
        }
    }
}
