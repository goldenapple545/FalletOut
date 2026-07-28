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
- **Zenject - DI фреймворк

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

**GameStateMachine** — типизированный конечный автомат (`Dictionary<Type, IState>`):
- **BootstrapState** — начальное состояние, сразу переходит в `GameLoopState`
- **GameLoopState** — сигнализирует `GameplaySceneLifecycle.NotifyGameplaySceneReady()`, подписчики реагируют на готовность геймплей-сцены
- **LoadGameplaySceneState** — сигнализирует `NotifyGameplaySceneUnloading()` при выгрузке сцены
- Переходы driven через FishNet: `FishNetSceneFlowAdapter` подписан на `OnLoadEnd` → `Enter<GameLoopState>()`, `OnUnloadStart` → `Enter<LoadGameplaySceneState>()`

**StaticDataService** — централизованный сервис для работы с ScriptableObject конфигами. Подгружается на этапе `WarmupStaticDataStep` до того, как игра становится готова к работе.

**AppReadyService** — `UniTaskCompletionSource`, сигнализирует о завершении всех bootstrap-шагов. Внешние системы (UI, тесты) ждут через `WaitUntilReadyAsync()`.

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
