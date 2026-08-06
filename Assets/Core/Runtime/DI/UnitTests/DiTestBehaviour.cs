using UnityEngine;

namespace CrystalEngine.DI
{
    // --- Интерфейсы и классы (адаптированные под Unity) ---
    public interface ILogger { void Log(string message); }
    public interface IInputService { void UpdateInput(); }

    public class UnityDebugLogger : ILogger
    {
        // Используем стандартный логгер Unity
        public void Log(string message) => Debug.Log($"[DI Logger] {message}");
    }

    public class MobileInputService : IInputService
    {
        private readonly ILogger _logger;

        public MobileInputService(ILogger logger)
        {
            _logger = logger;
            _logger.Log("MobileInputService успешно создан!");
        }

        public void UpdateInput() => _logger.Log("Считывание касаний экрана (Unity Input)...");
    }

    public class MovementController
    {
        private readonly IInputService _input;
        private readonly ILogger _logger;

        public MovementController(IInputService input, ILogger logger)
        {
            _input = input;
            _logger = logger;
            _logger.Log("MovementController успешно создан со всеми зависимостями!");
        }

        public void Move()
        {
            _input.UpdateInput();
            _logger.Log("Персонаж перемещается в Unity-сцене.");
        }
    }

    // --- Сам компонент для запуска теста ---
    public class DiTestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("<color=green>=== ЗАПУСК ТЕСТА DI-КОНТЕЙНЕРА ===</color>");

            // 1. Инициализация контейнера
            var container = new DIContainer();

            // 2. Регистрация зависимостей
            container.Bind<ILogger>().To<UnityDebugLogger>().AsTransient();
            container.Bind<IInputService>().To<MobileInputService>().AsSingle();
            container.Bind<MovementController>().To<MovementController>().AsSingle();

            Debug.Log("<b>--- Разрешение зависимостей (Resolve) ---</b>");

            // 3. Запрашиваем MovementController (запустит рекурсивную сборку)
            var movementController = container.Resolve<MovementController>();

            Debug.Log("<b>--- Проверка работы собранного контроллера ---</b>");
            movementController.Move();

            Debug.Log("<b>--- Проверка работы Singleton ---</b>");
            var input1 = container.Resolve<IInputService>();
            var input2 = container.Resolve<IInputService>();

            bool isSameInstance = ReferenceEquals(input1, input2);
            Debug.Log($"Оба вызова IInputService вернули один и тот же объект? <b>{isSameInstance}</b>");

            Debug.Log("<color=green>=== ТЕСТ ЗАВЕРШЕН УСПЕШНО ===</color>");
        }
    }
}