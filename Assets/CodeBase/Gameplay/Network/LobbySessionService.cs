using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.Session;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Transporting;
using UnityEngine;

namespace CodeBase.Gameplay.Network
{
    public enum LobbyMode
    {
        Offline,
        Searching,
        Connecting,
        Client,
        StartingHost,
        Host
    }

    public sealed class LobbySessionService : IDisposable
    {
        private const float StopTimeoutSeconds = 5f;

        private readonly ISessionService _sessionService;
        private readonly NameLanDiscoveryTransport _lanDiscovery;
        private readonly IStaticDataService _staticDataService;
        private readonly float _autoRefreshInterval;

        private readonly List<ServerInfo> _foundServers = new();

        private CancellationTokenSource _lifetimeCts = new();
        private CancellationTokenSource _transitionCts;
        private CancellationTokenSource _autoRefreshCts;

        private bool _transitioning;
        private bool _intentionalClientStop;
        private bool _intentionalServerStop;
        private bool _disposed;

        public LevelConfig SelectedLevel { get; set; }
        public VehicleConfig SelectedVehicle { get; set; }

        public LobbyMode Mode { get; private set; } = LobbyMode.Offline;
        public bool IsTransitioning => _transitioning;
        public IReadOnlyList<ServerInfo> FoundServers => _foundServers;
        public string CurrentServerName { get; private set; } = string.Empty;

        public event Action<LobbyMode> ModeChanged;
        public event Action<string> StatusChanged;
        public event Action<IReadOnlyList<ServerInfo>> ServerListChanged;
        public event Action<int> HostPlayersChanged;
        public event Action<bool> ClientConnectionResult;
        public event Action<bool> TransitionStateChanged;
        public event Action<string> ServerNameChanged;
        public event Action<LevelConfig> SelectedLevelChanged;
        public event Action<VehicleConfig> SelectedVehicleChanged;

        public LobbySessionService(
            ISessionService sessionService,
            NameLanDiscoveryTransport lanDiscovery,
            IStaticDataService staticDataService,
            float autoRefreshInterval = 2f)
        {
            _sessionService = sessionService ??
                throw new ArgumentNullException(nameof(sessionService));

            _lanDiscovery = lanDiscovery ? lanDiscovery : throw new ArgumentNullException(nameof(lanDiscovery));
            _staticDataService = staticDataService ?? throw new ArgumentNullException(nameof(staticDataService));

            _autoRefreshInterval = Mathf.Max(1f, autoRefreshInterval);

            if (_staticDataService.LevelsRegistry != null &&
                _staticDataService.LevelsRegistry.Levels.Count > 0)
            {
                SelectedLevel = _staticDataService.LevelsRegistry.Levels[0];
            }

            if (_staticDataService.VehiclesRegistry != null &&
                _staticDataService.VehiclesRegistry.Vehicles.Count > 0)
            {
                SelectedVehicle = _staticDataService.VehiclesRegistry.Vehicles[0];
            }

            _sessionService.ClientConnectionStateChanged += OnClientConnectionState;
            _sessionService.ClientAuthenticated += OnClientAuthenticated;
            _sessionService.ServerConnectionStateChanged += OnServerConnectionState;
            _sessionService.RemoteConnectionStateChanged += OnRemoteConnectionState;

            _lanDiscovery.ServerInfoFound += OnServerInfoFound;
        }

        public void RefreshLobbies()
        {
            if (_disposed || _transitioning || _sessionService.IsServerStarted)
                return;

            _foundServers.Clear();
            ServerListChanged?.Invoke(_foundServers);

            SetMode(LobbyMode.Searching);
            StatusChanged?.Invoke("Обновление серверов...");

            _lanDiscovery.StopSearchingOrAdvertising();
            _lanDiscovery.SearchForServers();
        }

        public void StartHost()
        {
            if (_disposed || _transitioning)
                return;

            RunTransition(StartHostAsync);
        }

        public void SetSelectedLevel(LevelConfig level)
        {
            // if (level == null || SelectedLevel == level)
            //     return;

            SelectedLevel = level;
            SelectedLevelChanged?.Invoke(level);
        }

        public void SetSelectedVehicle(VehicleConfig vehicle)
        {
            // Debug.LogError($"[LobbySessionService] {vehicle}");
            // if (vehicle == null || SelectedVehicle == vehicle)
            // {
            //     return;
            // }

            SelectedVehicle = vehicle;
            SelectedVehicleChanged?.Invoke(vehicle);
        }

        public void ConnectToServer(IPEndPoint endPoint)
        {
            if (_disposed || _transitioning || endPoint == null)
                return;

            RunTransition(token => ConnectToServerAsync(endPoint, token));
        }

        public void StopClient()
        {
            if (_disposed || _transitioning)
                return;

            CancelCurrentTransition();

            _intentionalClientStop = true;
            _lanDiscovery.StopSearchingOrAdvertising();
            _sessionService.StopClient();

            SetCurrentServerName(string.Empty);
            SetMode(LobbyMode.Offline);
        }

        public void StopHost()
        {
            if (_disposed || _transitioning)
                return;

            RunTransition(StopHostAsync);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            StopAutoRefresh();
            CancelCurrentTransition();

            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;

            _sessionService.ClientConnectionStateChanged -= OnClientConnectionState;
            _sessionService.ClientAuthenticated -= OnClientAuthenticated;
            _sessionService.ServerConnectionStateChanged -= OnServerConnectionState;
            _sessionService.RemoteConnectionStateChanged -= OnRemoteConnectionState;

            _lanDiscovery.ServerInfoFound -= OnServerInfoFound;
        }

        private void RunTransition(Func<CancellationToken, UniTask> transition)
        {
            CancelCurrentTransition();

            _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(
                _lifetimeCts.Token);

            ExecuteTransitionAsync(transition, _transitionCts.Token).Forget();
        }

        private async UniTaskVoid ExecuteTransitionAsync(
            Func<CancellationToken, UniTask> transition,
            CancellationToken token)
        {
            SetTransitioning(true);

            try
            {
                await transition(token);
            }
            catch (OperationCanceledException)
            {
                // Новый переход или уничтожение ProjectContext.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                StatusChanged?.Invoke("Ошибка сетевого перехода");
                SetMode(LobbyMode.Offline);
            }
            finally
            {
                SetTransitioning(false);
            }
        }

        private async UniTask StartHostAsync(CancellationToken token)
        {
            _lanDiscovery.StopSearchingOrAdvertising();

            SetCurrentServerName(CreateDefaultServerName());
            SetMode(LobbyMode.StartingHost);
            StatusChanged?.Invoke("Запуск сервера...");

            _lanDiscovery.SetAdvertisement(CurrentServerName, 1);

            await StopCurrentSessionAsync(token);

            _intentionalClientStop = false;
            _intentionalServerStop = false;

            _sessionService.StartHost();

            await WaitUntilHostStartedAsync(token);

            _lanDiscovery.AdvertiseServer();

            HostPlayersChanged?.Invoke(_sessionService.ConnectedClientsCount);
        }

        private async UniTask ConnectToServerAsync(
            IPEndPoint endPoint,
            CancellationToken token)
        {
            _lanDiscovery.StopSearchingOrAdvertising();

            ServerInfo foundServer = FindServer(endPoint);
            SetCurrentServerName(
                !string.IsNullOrWhiteSpace(foundServer.Name)
                    ? foundServer.Name
                    : endPoint.ToString());

            SetMode(LobbyMode.Connecting);
            StatusChanged?.Invoke($"Подключение к {CurrentServerName}...");

            await StopCurrentSessionAsync(token);

            _intentionalClientStop = false;
            _intentionalServerStop = false;

            _sessionService.StartClient(endPoint.Address.ToString());
        }

        private async UniTask StopHostAsync(CancellationToken token)
        {
            _lanDiscovery.StopSearchingOrAdvertising();

            _intentionalClientStop = true;
            _intentionalServerStop = true;

            _sessionService.Stop();

            await WaitUntilOfflineAsync(token);

            _intentionalClientStop = false;
            _intentionalServerStop = false;

            SetCurrentServerName(string.Empty);
            SetMode(LobbyMode.Offline);
        }

        private async UniTask StopCurrentSessionAsync(CancellationToken token)
        {
            bool wasOnline =
                _sessionService.IsClientStarted ||
                _sessionService.IsServerStarted;

            if (!wasOnline)
                return;

            _intentionalClientStop = true;
            _intentionalServerStop = true;

            _sessionService.Stop();

            await WaitUntilOfflineAsync(token);

            _intentionalClientStop = false;
            _intentionalServerStop = false;
        }

        private void OnServerInfoFound(ServerInfo info)
        {
            int existingIndex = _foundServers.FindIndex(server =>
                server.EndPoint.Address.Equals(info.EndPoint.Address) &&
                server.EndPoint.Port == info.EndPoint.Port);

            if (existingIndex >= 0)
            {
                _foundServers[existingIndex] = info;
            }
            else
            {
                _foundServers.Add(info);
            }

            ServerListChanged?.Invoke(_foundServers);
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                if (Mode is LobbyMode.Connecting or LobbyMode.StartingHost)
                    StatusChanged?.Invoke("Соединение установлено...");

                return;
            }

            if (args.ConnectionState != LocalConnectionState.Stopped)
                return;

            if (Mode == LobbyMode.Connecting && !_intentionalClientStop)
            {
                StatusChanged?.Invoke("Не удалось подключиться");
                ClientConnectionResult?.Invoke(false);
                SetCurrentServerName(string.Empty);
                SetMode(LobbyMode.Offline);
            }
            else if (Mode == LobbyMode.Client && !_intentionalClientStop)
            {
                StatusChanged?.Invoke("Соединение потеряно");
                SetCurrentServerName(string.Empty);
                SetMode(LobbyMode.Offline);
            }
        }

        private void OnClientAuthenticated()
        {
            if (_sessionService.IsHostStarted)
            {
                SetMode(LobbyMode.Host);
                StatusChanged?.Invoke("Сервер запущен");
                HostPlayersChanged?.Invoke(_sessionService.ConnectedClientsCount);

                return;
            }

            SetMode(LobbyMode.Client);
            StatusChanged?.Invoke($"Подключено к {CurrentServerName}");
            ClientConnectionResult?.Invoke(true);
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState != LocalConnectionState.Stopped)
                return;

            if (_intentionalServerStop)
                return;

            if (!_sessionService.IsClientStarted)
            {
                _lanDiscovery.StopSearchingOrAdvertising();
                SetCurrentServerName(string.Empty);
                SetMode(LobbyMode.Offline);
            }
        }

        private void OnRemoteConnectionState(
            NetworkConnection connection,
            RemoteConnectionStateArgs args)
        {
            if (!_sessionService.IsServerStarted)
                return;

            int players = _sessionService.ConnectedClientsCount;

            _lanDiscovery.SetAdvertisement(CurrentServerName, players);
            HostPlayersChanged?.Invoke(players);
        }

        private void SyncFromCurrentNetworkState()
        {
            if (_sessionService.IsHostStarted)
            {
                SetMode(LobbyMode.Host);
                HostPlayersChanged?.Invoke(_sessionService.ConnectedClientsCount);

                if (!_lanDiscovery.IsAdvertising)
                    _lanDiscovery.AdvertiseServer();

                return;
            }

            if (_sessionService.IsClientStarted)
            {
                SetMode(LobbyMode.Client);
                return;
            }

            SetMode(LobbyMode.Offline);
        }

        private ServerInfo FindServer(IPEndPoint endPoint)
        {
            return _foundServers.Find(server =>
                server.EndPoint.Address.Equals(endPoint.Address) &&
                server.EndPoint.Port == endPoint.Port);
        }

        private void SetCurrentServerName(string name)
        {
            if (CurrentServerName == name)
                return;

            CurrentServerName = name;
            ServerNameChanged?.Invoke(name);
        }

        private void SetMode(LobbyMode newMode)
        {
            if (Mode == newMode)
                return;

            Mode = newMode;
            ModeChanged?.Invoke(newMode);

            if (newMode is LobbyMode.Offline or LobbyMode.Searching)
                StartAutoRefresh();
            else
                StopAutoRefresh();
        }

        private void StartAutoRefresh()
        {
            if (_disposed)
                return;

            StopAutoRefresh();

            _autoRefreshCts = CancellationTokenSource.CreateLinkedTokenSource(
                _lifetimeCts.Token);

            AutoRefreshLoopAsync(_autoRefreshCts.Token).Forget();
        }

        private void StopAutoRefresh()
        {
            if (_autoRefreshCts == null)
                return;

            _autoRefreshCts.Cancel();
            _autoRefreshCts.Dispose();
            _autoRefreshCts = null;
        }

        private async UniTaskVoid AutoRefreshLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_autoRefreshInterval),
                    cancellationToken: token);

                if (!token.IsCancellationRequested)
                    RefreshLobbies();
            }
        }

        private async UniTask WaitUntilHostStartedAsync(CancellationToken token)
        {
            float startedAt = Time.realtimeSinceStartup;

            while (!_sessionService.IsHostStarted)
            {
                token.ThrowIfCancellationRequested();

                if (Time.realtimeSinceStartup - startedAt > StopTimeoutSeconds)
                    throw new TimeoutException("Не удалось запустить host.");

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private async UniTask WaitUntilOfflineAsync(CancellationToken token)
        {
            float startedAt = Time.realtimeSinceStartup;

            while (_sessionService.IsClientStarted || _sessionService.IsServerStarted)
            {
                token.ThrowIfCancellationRequested();

                if (Time.realtimeSinceStartup - startedAt > StopTimeoutSeconds)
                    throw new TimeoutException("Сетевая сессия не остановилась вовремя.");

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private void SetTransitioning(bool value)
        {
            if (_transitioning == value)
                return;

            _transitioning = value;
            TransitionStateChanged?.Invoke(value);
        }

        private void CancelCurrentTransition()
        {
            if (_transitionCts == null)
                return;

            _transitionCts.Cancel();
            _transitionCts.Dispose();
            _transitionCts = null;
        }

        private static string CreateDefaultServerName()
        {
            int id = UnityEngine.Random.Range(100, 1000);
            return $"Сервер {id}";
        }
    }
}