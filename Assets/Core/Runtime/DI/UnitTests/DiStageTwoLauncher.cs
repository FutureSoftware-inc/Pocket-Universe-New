using UnityEngine;

namespace CrystalEngine.DI.Tests
{
    public class DiStageTwoLauncher : MonoBehaviour
    {
        [SerializeField] private PlayerComponent _player;
        [SerializeField] private EnemyComponent _enemy;

        private void Start()
        {
            Debug.Log("<color=yellow>=== ЗАПУСК ПРОВЕРКИ ИНТЕГРАЦИИ (ЭТАП 2) ===</color>");

            if (_player == null || _enemy == null)
            {
                Debug.LogError("Перетащите ссылки на Player и Enemy в инспектор скрипта DiStageTwoLauncher!");
                return;
            }

            _player.CollectCoin();
            Debug.Log("------------------------------------");
            _enemy.Die();

            Debug.Log("<color=green>=== ПРОВЕКА ЭТАПА 2 ЗАВЕРШЕНА ===</color>");
        }
    }
}
