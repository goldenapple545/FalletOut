using System.Collections.Generic;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Data;
using CodeBase.Gameplay.Network;
using CodeBase.Gameplay.Spawn;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Network.Match.Spawn
{
    public sealed class MatchVehicleSpawner : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private ArenaSpawnRegistry spawnRegistry;
        [SerializeField] private VehicleColorRegistry colorRegistry;

        private readonly Dictionary<int, NetworkObject> _vehiclesByClientId =
            new();

        private readonly Dictionary<int, VehicleConfig> _clientVehicles =
            new();

        private DiContainer _sceneContainer;
        private MatchManager _matchManager;
        private LobbySessionService _lobbyService;
        private IStaticDataService _staticDataService;

        [Inject]
        private void Construct(
            DiContainer sceneContainer,
            MatchManager matchManager,
            LobbySessionService lobbyService,
            IStaticDataService staticDataService)
        {
            _sceneContainer = sceneContainer;
            _matchManager = matchManager;
            _lobbyService = lobbyService;
            _staticDataService = staticDataService;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkManager.SceneManager.OnClientPresenceChangeEnd +=
                HandleClientPresenceChanged;
            
            _matchManager.OnRoundStartingServer +=
                RepositionPlayersToSpawnPointsServer;

            foreach (NetworkConnection connection in
                     ServerManager.Clients.Values)
            {
                if (!connection.IsActive)
                    continue;

                if (_vehiclesByClientId.ContainsKey(
                        connection.ClientId))
                {
                    continue;
                }

                // Не спавним здесь вслепую.
                // Клиент попадёт в SpawnVehicleFor через
                // OnClientPresenceChangeEnd, когда станет observer сцены.
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Клиент сообщает серверу свою выбранную машинку
            VehicleConfig selected = _lobbyService?.SelectedVehicle;

            if (selected != null && !string.IsNullOrEmpty(selected.Id))
            {
                int clientId = NetworkManager.ClientManager.Connection.ClientId;
                ReportVehicleClientRpc(clientId, selected.Id);
            }
        }

        public override void OnStopServer()
        {
            NetworkManager.SceneManager.OnClientPresenceChangeEnd -=
                HandleClientPresenceChanged;
            
            _matchManager.OnRoundStartingServer -=
                RepositionPlayersToSpawnPointsServer;

            _vehiclesByClientId.Clear();
            _clientVehicles.Clear();

            base.OnStopServer();
        }

        /// <summary>
        /// Клиент вызывает этот RPC при загрузке match сцены,
        /// сообщая серверу свою выбранную машинку.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ReportVehicleClientRpc(int clientId, string vehicleId)
        {
            if (!IsServerStarted || string.IsNullOrEmpty(vehicleId))
                return;

            VehicleConfig config = ResolveVehicleById(vehicleId);

            if (config != null)
            {
                _clientVehicles[clientId] = config;

                Debug.Log(
                    $"[{nameof(MatchVehicleSpawner)}] " +
                    $"Client {clientId} reported vehicle: {vehicleId}",
                    this);

                // Если клиент уже в сцене но ещё не заспавнен — спавним сейчас
                foreach (NetworkConnection connection in ServerManager.Clients.Values)
                {
                    if (connection.ClientId == clientId &&
                        connection.IsActive &&
                        !_vehiclesByClientId.ContainsKey(clientId))
                    {
                        SpawnVehicleFor(connection);
                        break;
                    }
                }
            }
        }

        private VehicleConfig ResolveVehicleById(string vehicleId)
        {
            if (_staticDataService?.VehiclesRegistry == null)
                return null;

            foreach (var v in _staticDataService.VehiclesRegistry.Vehicles)
            {
                if (v.Id == vehicleId)
                    return v;
            }

            return null;
        }

        private VehicleConfig GetVehicleForClient(int clientId)
        {
            if (_clientVehicles.TryGetValue(clientId, out VehicleConfig config))
                return config;

            // Fallback: выбор хоста (для хоста самого)
            return _lobbyService?.SelectedVehicle;
        }

        private void HandleClientPresenceChanged(
            ClientPresenceChangeEventArgs args)
        {
            if (!args.Added)
                return;

            if (args.Scene != gameObject.scene)
                return;

            // Хост (server connection, clientId == 0) спавнится сразу — fallback на SelectedVehicle.
            // Для клиентов ждём ReportVehicleClientRpc.
            int clientId = args.Connection.ClientId;
            bool isHostConnection = clientId == 0;

            if (!isHostConnection && !_clientVehicles.ContainsKey(clientId))
            {
                // Выбор ещё не получен — спавн произойдёт в ReportVehicleClientRpc
                // когда клиент сообщит свою машинку.
                return;
            }

            SpawnVehicleFor(args.Connection);
        }

        private void SpawnVehicleFor(NetworkConnection connection)
        {
            if (connection == null || !connection.IsActive)
                return;

            if (_vehiclesByClientId.ContainsKey(connection.ClientId))
                return;

            VehicleConfig vehicleConfig = GetVehicleForClient(connection.ClientId);

            if (vehicleConfig == null ||
                vehicleConfig.Prefab == null ||
                spawnRegistry == null ||
                spawnRegistry.Count == 0 || colorRegistry == null ||
                colorRegistry.Count == 0 || _sceneContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(MatchVehicleSpawner)}] " +
                    $"Missing configuration for vehicle spawn. Client={connection.ClientId}",
                    this);

                return;
            }

            int spawnIndex = _vehiclesByClientId.Count;
            Transform spawnPoint = spawnRegistry.GetSpawnPoint(spawnIndex);
            VehicleColorEntry colorEntry = colorRegistry.GetEntry(spawnIndex);

            NetworkObject vehicle = Instantiate(
                vehicleConfig.Prefab,
                spawnPoint.position,
                spawnPoint.rotation);
            
            // Важно: здесь именно SceneContext container.
            _sceneContainer.InjectGameObject(vehicle.gameObject);

            PlayerMatchState playerState =
                vehicle.GetComponent<PlayerMatchState>();
            
            playerState.SetPendingIdentity(
                colorEntry.Color,
                colorEntry.Name);
            
            ServerManager.Spawn(vehicle, connection);

            _vehiclesByClientId.Add(connection.ClientId, vehicle);

            _matchManager.SetExpectedPlayerCountServer(
                ServerManager.Clients.Count);
            
            Debug.Log(
                $"[{nameof(MatchVehicleSpawner)}] " +
                $"Spawned vehicle for client={connection.ClientId}, " +
                $"position={spawnPoint.position}.",
                this);
        }
        
        private void RepositionPlayersToSpawnPointsServer(
            IReadOnlyList<PlayerMatchState> players)
        {
            if (!IsServerStarted)
                return;

            if (spawnRegistry == null || spawnRegistry.Count == 0)
                return;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerMatchState player = players[i];

                if (player == null)
                    continue;

                Transform spawnPoint =
                    spawnRegistry.GetSpawnPoint(i);

                Rigidbody rigidbody =
                    player.GetComponent<Rigidbody>();

                if (rigidbody != null)
                {
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }

                player.transform.SetPositionAndRotation(
                    spawnPoint.position,
                    spawnPoint.rotation);
            }
        }
    }
}