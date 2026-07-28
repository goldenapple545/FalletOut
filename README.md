# FalletOut

Многопользовательская игра на Unity с сетевым взаимодействием, включающая управление автомобилями, систему повреждений и клиентское предсказание.

## Оглавление

- [Обзор](#обзор)
- [Технологии](#технологии)
  - [Движок и рендеринг](#движок-и-рендеринг)
  - [Сетевое взаимодействие](#сетевое-взаимодействие)
  - [Архитектура и реактивное программирование](#архитектура-и-реактивное-программирование)
  - [Инструменты разработки](#инструменты-разработки)
  - [Платформы](#платформы)
- [Структура проекта](#структура-проекта)
  - [Infrastructure](#infrastructure--загрузка-и-жизненный-цикл-приложения)
    - [Точка входа: AppBootstrapper](#точка-входа-appbootstrapper)
    - [GameStateMachine](#gamestatemachine)
    - [Переходы между сценами](#переходы-между-сценами)
    - [Подгрузка конфигов](#подгрузка-конфигов-staticdataservice)
    - [SessionService](#sessionservice)
  - [Мультиплеер](#мультиплеер--лобби-матч-клиентское-предсказание)
    - [Лобби-система](#лобби-система)
    - [Матч-система](#матч-система)
    - [Клиентское предсказание](#клиентское-предсказание-client-prediction)
    - [Сеть автомобилей](#сеть-автомобилей)

## Обзор

FalletOut — это мультиплеерный проект, разрабатываемый на Unity с использованием сетевого фреймворка FishNet. Проект включает систему управления транспортом с физикой подвески, клиентское предсказание для отзывчивого геймплея, а также синхронизацию состояний через сеть для всех ключевых игровых механик.

## Технологии

### Движок и рендеринг
- **Unity 6** (URP — Universal Render Pipeline)
- **Input System** — новая система ввода Unity

### Сетевое взаимодействие
- **FishNet** (`FISHNET;FISHNET_V4`) — сетевой фреймворк для мультиплеера
- **Unity Multiplayer Playmode** — тестирование мультиплеера в редакторе
- Клиентское предсказание (client prediction) и синхронизация состояний (ReconcileData)

### Архитектура и реактивное программирование
- **R3 (Reactive Extensions v3)** — реактивная библиотека от Cysharp для событийной архитектуры
- **Zenject** - DI фреймворк

### Инструменты разработки
- **Unity MCP** — интеграция с AI-ассистентами (CoplayDev/unity-mcp)
- **Rider** — IDE для разработки
- **Unity Test Framework** — модульные и play-mode тесты

### Платформы
- **Standalone** (Windows/macOS/Linux)
- **Android** (API 25+)
- **iOS**

---

## Структура проекта

### Infrastructure — загрузка и жизненный цикл приложения

Весь процесс запуска приложения построен вокруг **ProjectContext** (Zenject) и последовательного выполнения bootstrap-шагов:

```
Assets/CodeBase/Infrastructure/
├── AppBootstrapper.cs          # Точка входа — запускает последовательное выполнение шагов
├── AppReadyService.cs          # Сигнал готовности приложения (UniTaskCompletionSource)
├── FishNetFacade.cs            # Фасад над FishNet NetworkManager
├── BootstrapSteps/             # Шаги начальной загрузки
│   ├── GameStateMachine/
│   │   └── InitializeGameStateStep.cs   # Запускает GameStateMachine в BootstrapState
│   └── StaticData/
│       ├── WarmupStaticDataStep.cs      # Асинхронная подгрузка всех конфигов/скриптаблов
│       ├── StaticDataService.cs         # Сервис статических данных
│       └── IStaticDataService.cs
├── Installers/
│   ├── Project/                # Инсталлеры ProjectContext (одиночки на всю жизнь приложения)
│   │   ├── AppInstaller.cs              # Биндит AppBootstrapper + AppReadyService
│   │   ├── BootstrapStepsInstaller.cs   # Собирает все IAppBootstrapStep в список
│   │   ├── GameStateMachineInstaller.cs # Биндит GameStateMachine
│   │   ├── NetworkInstaller.cs          # Сетевые сервисы (SessionService, LanDiscovery)
│   │   ├── StaticDataInstaller.cs       # StaticDataService
│   │   └── ...
│   └── Scene/                  # Инсталлеры SceneContext (пересоздаются при загрузке сцен)
└── Services/
    ├── GameStateMachine/       # Конечный автомат состояний игры
    ├── SceneLoader/            # Асинхронная загрузка сцен
    └── Session/                # Управление сетевой сессией
```

#### Точка входа: AppBootstrapper

`AppBootstrapper` регистрируется как `NonLazy` синглтон и запускает все `IAppBootstrapStep` последовательно:

```csharp
public sealed class AppBootstrapper : IInitializable, IDisposable
{
    private readonly IReadOnlyList<IAppBootstrapStep> _steps;
    private readonly IAppReadyService _appReadyService;

    public void Initialize()
    {
        RunAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid RunAsync(CancellationToken ct)
    {
        try
        {
            foreach (var step in _steps)
                await step.ExecuteAsync(ct);

            _appReadyService.MarkReady();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
```

Шаги собираются через Zenject в `BootstrapStepsInstaller`:

```csharp
Container.BindInterfacesAndSelfTo<WarmupStaticDataStep>().AsSingle();
Container.BindInterfacesAndSelfTo<InitializeGameStateStep>().AsSingle();
```

#### GameStateMachine

Типизированный конечный автомат, хранящий состояния в `Dictionary<Type, IState>`. Переходы выполняются по типу состояния:

```csharp
public sealed class GameStateMachine : IGameStateMachine
{
    private readonly Dictionary<Type, IState> _states;
    private IState _currentState;

    public GameStateMachine(GameplaySceneLifecycle SceneLifecycle)
    {
        _states = new Dictionary<Type, IState>();
        _states[typeof(BootstrapState)] = new BootstrapState(EnterByType);
        _states[typeof(LoadGameplaySceneState)] = new LoadGameplaySceneState(SceneLifecycle);
        _states[typeof(GameLoopState)] = new GameLoopState(SceneLifecycle);
    }

    public void Enter<TState>() where TState : class, IState
    {
        _currentState?.Exit();
        IState state = _states[typeof(TState)];
        _currentState = state;
        state.Enter();
    }
}
```

**Состояния:**

- **BootstrapState** — pass-through, сразу переходит в `GameLoopState`:
  ```csharp
  public sealed class BootstrapState : IState
  {
      private readonly Action<Type> _enter;
      public BootstrapState(Action<Type> enter) => _enter = enter;
      public void Enter() => _enter(typeof(GameLoopState));
      public void Exit() { }
  }
  ```

- **GameLoopState** — сигнализирует о готовности геймплей-сцены:
  ```csharp
  public void Enter() => _sceneLifecycle.NotifyGameplaySceneReady();
  public void Exit() => _sceneLifecycle.NotifyGameplaySceneUnloading();
  ```

- **LoadGameplaySceneState** — сигнализирует о выгрузке сцены.

#### Переходы между сценами

Сцены загружаются/выгружаются через FishNet, а `FishNetSceneFlowAdapter` мостит это с GameStateMachine:

```csharp
public sealed class FishNetSceneFlowAdapter : IInitializable, IDisposable
{
    private readonly NetworkManager _networkManager;
    private readonly IGameStateMachine _gameStateMachine;

    public void Initialize()
    {
        _networkManager.SceneManager.OnLoadEnd += OnLoadEnd;
        _networkManager.SceneManager.OnUnloadStart += OnUnloadStart;
    }

    private void OnLoadEnd(SceneLoadEndEventArgs args) =>
        _gameStateMachine.Enter<GameLoopState>();

    private void OnUnloadStart(SceneUnloadStartEventArgs args) =>
        _gameStateMachine.Enter<LoadGameplaySceneState>();
}
```

**GameplaySceneLifecycle** — флаг-based менеджер с событиями:

```csharp
public sealed class GameplaySceneLifecycle : IGameplaySceneLifecycle
{
    public bool IsGameplaySceneReady { get; private set; }
    public event Action GameplaySceneReady;
    public event Action GameplaySceneUnloading;

    public void NotifyGameplaySceneReady()
    {
        if (IsGameplaySceneReady) return;
        IsGameplaySceneReady = true;
        GameplaySceneReady?.Invoke();
    }

    public void NotifyGameplaySceneUnloading()
    {
        if (!IsGameplaySceneReady) return;
        IsGameplaySceneReady = false;
        GameplaySceneUnloading?.Invoke();
    }
}
```

Загрузка сцен — обёртка над `SceneManager.LoadSceneAsync` с UniTask:

```csharp
public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, CancellationToken ct = default)
{
    await SceneManager.LoadSceneAsync(sceneName, mode).ToUniTask(cancellationToken: ct);
}
```

#### Подгрузка конфигов (StaticDataService)

`StaticDataService` загружает ScriptableObject-конфиги из `Resources/` на этапе `WarmupStaticDataStep`:

```csharp
public sealed class StaticDataService : IStaticDataService
{
    private const string VehicleConfigPath = "StaticData/VehicleConfig";
    private const string CollisionDamageConfigPath = "StaticData/CollisionDamageConfig";
    private const string MatchRulesConfigPath = "StaticData/MatchRulesConfig";

    public VehicleConfig VehicleConfig { get; private set; }
    public CollisionDamageConfig CollisionDamageConfig { get; private set; }
    public MatchRulesConfig MatchRulesConfig { get; private set; }

    public async UniTask WarmupAsync(CancellationToken ct)
    {
        VehicleConfig = await LoadAsync<VehicleConfig>(VehicleConfigPath, ct);
        CollisionDamageConfig = await LoadAsync<CollisionDamageConfig>(CollisionDamageConfigPath, ct);
        MatchRulesConfig = await LoadAsync<MatchRulesConfig>(MatchRulesConfigPath, ct);
    }

    private static async UniTask<T> LoadAsync<T>(string path, CancellationToken ct) where T : UnityEngine.Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        await request.ToUniTask(cancellationToken: ct);

        T asset = request.asset as T;
        if (asset == null)
            throw new Exception($"Static data asset of type {typeof(T).Name} not found at Resources/{path}");

        return asset;
    }
}
```

Шаг загрузки:

```csharp
public sealed class WarmupStaticDataStep : IAppBootstrapStep
{
    private readonly IStaticDataService _staticDataService;
    public WarmupStaticDataStep(IStaticDataService staticDataService) => _staticDataService = staticDataService;
    public UniTask ExecuteAsync(CancellationToken ct) => _staticDataService.WarmupAsync(ct);
}
```

#### SessionService

Тонкий фасад над FishNet `NetworkManager`:

```csharp
public sealed class SessionService : ISessionService, IInitializable, IDisposable
{
    public bool IsClientStarted => NetworkManager.IsClientStarted;
    public bool IsServerStarted => NetworkManager.IsServerStarted;
    public bool IsHostStarted => NetworkManager.IsHostStarted;
    public int ConnectedClientsCount => NetworkManager.ServerManager.Clients.Count;

    public event Action ClientAuthenticated;
    public event Action<ClientConnectionStateArgs> ClientConnectionStateChanged;
    public event Action<ServerConnectionStateArgs> ServerConnectionStateChanged;
    public event Action<NetworkConnection, RemoteConnectionStateArgs> RemoteConnectionStateChanged;

    public void Initialize()
    {
        _networkManager.ClientManager.OnAuthenticated += HandleAuthenticated;
        _networkManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
        _networkManager.ServerManager.OnServerConnectionState += HandleServerConnectionState;
        _networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
    }

    public void StartHost(string localAddress = "127.0.0.1")
    {
        NetworkManager.TransportManager.Transport.SetClientAddress(localAddress);
        NetworkManager.ServerManager.StartConnection();
        NetworkManager.ClientManager.StartConnection();
    }

    public void StartClient(string address)
    {
        NetworkManager.TransportManager.Transport.SetClientAddress(address);
        NetworkManager.ClientManager.StartConnection();
    }
}
```

---

### Мультиплеер — лобби, матч, клиентское предсказание

```
Assets/CodeBase/Gameplay/Network/
├── NetworkRuntimeRoot.cs       # Корневой компонент сети (держит FishNet NetworkManager)
├── LobbySessionService.cs      # Управление лобби: поиск, хост, подключение
├── LanDiscoveryTransport.cs    # LAN Discovery (UDP broadcast)
├── Lobby/
│   ├── ILobbyRosterService.cs  # Список игроков в лобби
│   └── LobbyPlayerInfo.cs      # Информация об одном игроке
├── Match/
│   ├── MatchManager.cs         # NetworkBehaviour — управляет фазами матча
│   ├── MatchPhase.cs           # WaitingForPlayers → RoundInProgress → RoundEnded
│   ├── LastManStandingMode.cs  # Режим "последний выживший"
│   ├── PlayerMatchState.cs     # Состояние игрока в матче (здоровье, цвет, alive)
│   ├── MatchSceneService.cs    # Сервис управления сценой матча
│   └── Spawn/                  # Спавн транспорта и игроков
└── UI/                         # Сетевой UI (меню подключения, лобби)
```

#### Лобби-система

**LobbySessionService** — центральный сервис управления сессиями. Реализует полный жизненный цикл мультиплеерной сессии:

- **Режимы**: `Offline` → `Searching` → `Connecting` → `Client` / `StartingHost` / `Host`
- **LAN Discovery** — автоматический поиск серверов через UDP broadcast с автообновлением каждые 2 секунды
- **Transition-система** — все переходы (старт хоста, подключение, остановка) выполняются асинхронно через `UniTask` с cancellation и таймаутами
- **Автостарт раунда** — когда все ожидаемые игроки подключились, матч начинается автоматически

#### Матч-система

**MatchManager** (FishNet `NetworkBehaviour`) управляет фазами матча:
- **WaitingForPlayers** — ожидание подключения всех игроков
- **RoundInProgress** — раунд активен
- **RoundEnded** — раунд завершён, есть победитель

Регистрация игроков через `RegisterPlayerServer()`, после достижения ожидаемого количества — автоматический старт. Режим **LastManStanding** определяет победителя как последнего выжившего игрока.

#### Клиентское предсказание (Client Prediction)

Автомобиль использует FishNet **Replicate/Reconcile** паттерн для отзывчивого управления:

**Replicate фаза (клиентская симуляция):**
- `OnTick()` — каждый тик сервера. Если `IsOwner`, читает ввод и пакует в `VehicleReplicateData` (throttle, steering, handbrake)
- Вызывается `[Replicate] RunInputs()` → `physicsMotor.Simulate(...)` с `PredictionRigidbody`
- FishNet пере-запускает этот метод на клиенте для прошлых тиков во время reconciliation

**Reconcile фаза (серверная коррекция):**
- `OnPostTick()` → `CreateReconcile()` (только сервер) — пакует текущее физическое состояние в `VehicleReconcileData`:
  - `PredictionRigidbody` (позиция, ротация, velocity, angular velocity)
  - Оси контроллера: `SteeringAxis`, `ThrottleAxis`, `DriftingAxis`
  - Флаги: `IsDrifting`, `IsTractionLocked`
  - Смещения подвески: 4 float (по одному на колесо, в метрах)
- `[Reconcile] ReconcileState()` — клиент применяет:
  - `_predictionRigidbody.Reconcile(data.Rigidbody)` — snap физики к серверному состоянию
  - `physicsMotor.ApplyReconcile(data)` — восстановление осей и флагов контроллера

**Поток:**
```
Client: OnTick → read input → [Replicate] RunInputs → simulate locally
Server: OnTick → receive input → [Replicate] RunInputs → simulate
Server: OnPostTick → CreateReconcile → package state → [Reconcile] ReconcileState
Client: receives reconcile → snap rigidbody → re-simulate from last confirmed input
```

> Подвеска синхронизируется как raw offset в метрах (не compression ratio 0..1) — это avoids ошибки от mismatched maxTravel между сервером и клиентом. Клиент применяет offset только к `VehicleWheelVisuals`, НЕ к `WheelCollider` физике.

#### Сеть автомобилей

- **NetworkVehicleController** — обёртка над физикой с сетевой синхронизацией
- **VehiclePhysicsMotor** — ядро физики автомобиля (ускорение, торможение, поворот)
- **PrometeoCarController** — контроллер управления автомобилем
- Повреждения синхронизированы через FishNet SyncVar и систему `VehicleDamageSystem`**
