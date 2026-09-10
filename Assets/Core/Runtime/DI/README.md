Руководство пользователя: Внедрение зависимостей (CrystalEngine.DI)Модуль внедрения зависимостей (Dependency Injection) в CrystalEngine представляет собой высокопроизводительный гибридный DI-контейнер, оптимизированный для работы в Unity 6. Он сочетает в себе удобный синтаксис Fluent API (в стиле Zenject) и высокую скорость работы за счет кэширования метаданных рефлексии (в стиле VContainer).Ключевые возможностиИерархия контейнеров: Поддержка родительских и дочерних контекстов (проект → сцена).Fluent API: Интуитивно понятная цепочка вызовов методов для конфигурации.Условное внедрение (Contextual Binding): Ограничение области видимости зависимостей.Поддержка Unity-префабов: Автоматический спавн префабов с инжекцией во все дочерние MonoBehaviour.Безопасный трейсинг ошибок (Resolution Trace): Подробный вывод цепочки вызовов при возникновении ошибок разрешения зависимостей без утечек стека.Потокобезопасность: Защита от гонок данных и Double-Check Locking для Singleton-объектов.1. Регистрация зависимостейРегистрация связей выполняется на этапе инициализации приложения или сцены (например, внутри ваших классов-инсталляторов).Классическое связывание (Интерфейс → Реализация)Используется, когда код запрашивает интерфейс, а контейнер должен предоставить конкретный C#-класс.csharp// Объект создается один раз и используется как синглтон
container.Bind<IAudioService>().To<AudioService>().AsSingle();

// Новый экземпляр создается при каждом запросе
container.Bind<IWeapon>().To<Shotgun>().AsTransient();
Регистрация класса «Сам на себя»Если у класса нет интерфейса, его можно зарегистрировать на самого себя с помощью метода BindAsSelf.csharpcontainer.BindAsSelf<GameAnalytics>().AsSingle();
Автоматическое связывание интерфейсовМетод BindInterfaces сканирует тип и автоматически привязывает его ко всем интерфейсам, которые он реализует. При этом создается ровно один разделяемый экземпляр объекта (если выбран AsSingle).csharp// Если SaveManager реализует IInitializable и ISaveService:
container.BindInterfaces<SaveManager>().AsSingle();

// Теперь оба запроса вернут один и тот же объект SaveManager:
var init = container.Resolve<IInitializable>();
var save = container.Resolve<ISaveService>();
Привязка к готовому экземпляруЕсли объект уже был создан вручную или является ScriptableObject, передайте его через FromInstance.csharpGameConfig config = Resources.Load<GameConfig>("GameConfig");
container.Bind<GameConfig>().FromInstance(config);
Создание через кастомный метод (Лямбду)Позволяет вручную настроить объект перед передачей в граф зависимостей.csharpcontainer.Bind<ILevelLoader>().ToMethod(ctx => 
{
    var loader = new AdvancedLevelLoader();
    loader.SetMaxThreads(4);
    return loader;
}).AsSingle();
Связывание с Unity-префабомПривязывает компонент на префабе к интерфейсу. Контейнер автоматически создаст префаб на сцене при первом запросе этой зависимости.csharp[SerializeField] private GameObject hudPrefab; // Содержит HUDController

void InitializeContainer()
{
    container.Bind<HUDController>().ToPrefab(hudPrefab).AsSingle();
}
2. Способы внедрения зависимостей (Injection)Контейнер поддерживает три классических способа получения зависимостей внутри классов, помеченных атрибутом [Inject].А. Внедрение через конструктор (Рекомендуется для pure C#)Наиболее чистый способ. Контейнер автоматически выберет конструктор с атрибутом [Inject]. Если атрибута нет, будет выбран конструктор с наибольшим количеством параметров.csharppublic class PlayerMovement
{
    private readonly IInputService _inputService;

    public PlayerMovement(IInputService inputService)
    {
        _inputService = inputService;
    }
}
Б. Внедрение в поля (Рекомендуется для MonoBehaviour)Поскольку Unity запрещает использовать конструкторы в MonoBehaviour, для них используется внедрение в приватные или публичные поля. Система CrystalEngine рекурсивно проверяет базовые классы, поэтому приватные инжекты в родительских скриптах также сработают.csharppublic class EnemyAI : MonoBehaviour
{
    [Inject] private IAudioService _audioService;
    [Inject] private PlayerTarget _target;
}
В. Внедрение через методыИспользуется, если после получения всех зависимостей объекту нужно выполнить какую-то стартовую логику.csharppublic class SaveService : ISaveService
{
    private IStorage _storage;

    [Inject]
    private void Construct(IStorage storage)
    {
        _storage = storage;
        _storage.InitializeConnection();
    }
}
3. Разрешение графа зависимостей (Resolution)После того как все связи зарегистрированы, вы можете запрашивать объекты из контейнера.Прямое получение одиночного объектаcsharpIInputService input = container.Resolve<IInputService>();
Получение всех зарегистрированных реализацийПолезно для паттернов вроде "Composite" или систем инициализации. Метод рекурсивно собирает зависимости по всей иерархии контейнеров (проект → сцена) без выделения лишней памяти в куче.csharp// Соберет все синглтоны, реализующие этот интерфейс
IReadOnlyList<IInitializable> initializables = container.ResolveAll<IInitializable>();

foreach (var system in initializables)
{
    system.Initialize();
}
Внедрение в существующий объектЕсли объект был создан методами Unity (например, Instantiate), вы можете принудительно «накатить» на него зависимости.csharpGameObject spawnedEnemy = Object.Instantiate(_enemyPrefab);
container.Inject(spawnedEnemy.GetComponent<EnemyAI>());
4. Динамическое создание объектов (Instantiating)Если вам нужно динамически создавать новые геймплейные объекты во время игры, используйте встроенные фабричные методы контейнера, чтобы новорожденные объекты сразу получали свои зависимости.Создание C# классаcsharp// Создаст экземпляр CombatCalculator и заинжектит в него зависимости
CombatCalculator calculator = container.Instantiate<CombatCalculator>();
Спавн Unity-префабовВнедряет зависимости во все компоненты MonoBehaviour на самом префабе и всех его дочерних объектах (включая неактивные). По соображениям безопасности вызовы разрешены только из Main Thread (главного потока Unity).csharp[SerializeField] private GameObject explosionPrefab;

void TriggerExplosion(Vector3 position)
{
    // Спавнит объект и автоматически инжектит в него всё необходимое
    GameObject fx = container.InstantiatePrefab(explosionPrefab, position, Quaternion.identity, parentTransform);
}
5. Продвинутые возможностиИерархические контейнеры (Scoping)Вы можете создавать дочерние контейнеры. Если дочерний контейнер не находит зависимость у себя, он обращается к родительскому. Обратное направление невозможно (родитель не видит зависимости ребенка).csharp// Контейнер уровня всего проекта (существует всегда)
DIContainer projectContainer = new DIContainer();
projectContainer.Bind<IAudioService>().To<AudioService>().AsSingle();

// Контейнер уровня конкретной сцены (удаляется при смене сцены)
DIContainer sceneContainer = new DIContainer(projectContainer);
sceneContainer.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

// Успешно найдет IAudioService в родительском контейнере
var audio = sceneContainer.Resolve<IAudioService>(); 
Условное связывание (WhenInjectedInto)Позволяет передавать разные реализации одного интерфейса в зависимости от того, какой класс запрашивает эту зависимость.csharp// Внедрять Shotgun только в класс Player
container.Bind<IWeapon>().To<Shotgun>().AsSingle().WhenInjectedInto<Player>();

// Внедрять Pistol во все остальные классы, запрашивающие IWeapon
container.Bind<IWeapon>().To<Pistol>().AsSingle();
6. Отладка и диагностика ошибокЕсли вы забудете зарегистрировать какую-то зависимость, или в графе возникнет циклическая связь (Класс А требует Класс Б, а Класс Б требует Класс А), контейнер выбросит структурированное исключение с трассировкой вызовов (Resolution Trace).Благодаря разметке, в консоли Unity ошибка подсветится красным цветом и покажет точное место излома:text[DI Trace Error] Не удалось разрешить зависимость для типа: IStorage
Цепочка вызовов (Resolution Trace):
  GameInitializer -> SaveService -> [ЗДЕСЬ ОШИБКА!]
Внутреннее исключение: [DI Error] Для типа IStorage не найдено зарегистрированных связей!
