using UnityEngine;
using static CrystalEngine.DI.Tests.GlobalProjectInstaller;

namespace CrystalEngine.DI.Tests
{
    public class StageFourPlayer : MonoBehaviour
    {
        private IGlobalAnalytics _analytics;
        private bool _isInitialized;

        [Inject]
        private void Construct(IGlobalAnalytics analytics)
        {
            _analytics = analytics;
            _isInitialized = true;
            Debug.Log("<color=cyan>[DI Test 4] Метод Construct в StageFourPlayer успешно вызван с глобальной зависимостью!</color>");
        }

        public void StartGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[DI TEST 4 FAILED] StageFourPlayer не получил глобальную аналитику!");
                return;
            }

            // Вызываем глобальный метод из локального скрипта сцены
            _analytics.SendEvent("Game_Started_From_Local_Scene");
        }
    }
}