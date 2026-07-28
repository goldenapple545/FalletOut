using System.Collections.Generic;
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
        [SerializeField] private NetworkObject vehiclePrefab;
        [SerializeField] private ArenaSpawnRegistry spawnRegistry;
        [SerializeField] private VehicleColorRegistry colorRegistry;

        private readonly Dictionary<int, NetworkObject> _vehiclesByClientId =
            new();

        private DiContainer _sceneContainer;
        private MatchManager _matchManager;

        [Inject]
        private void Construct(DiContainer sceneContainer, MatchManager matchManager)
        {
            _sceneContainer = sceneContainer;
            _matchManager = matchManager;
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

        public override void OnStopServer()
        {
            NetworkManager.SceneManager.OnClientPresenceChangeEnd -=
                HandleClientPresenceChanged;
            
            _matchManager.OnRoundStartingServer -=
                RepositionPlayersToSpawnPointsServer;

            _vehiclesByClientId.Clear();

            base.OnStopServer();
        }

        private void HandleClientPresenceChanged(
            ClientPresenceChangeEventArgs args)
        {
            if (!args.Added)
                return;

            if (args.Scene != gameObject.scene)
                return;

            SpawnVehicleFor(args.Connection);
        }

        private void SpawnVehicleFor(NetworkConnection connection)
        {
            if (connection == null || !connection.IsActive)
                return;

            if (_vehiclesByClientId.ContainsKey(connection.ClientId))
                return;

            if (vehiclePrefab == null || spawnRegistry == null ||
                spawnRegistry.Count == 0 || colorRegistry == null ||
                colorRegistry.Count == 0 || _sceneContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(MatchVehicleSpawner)}] " +
                    "Missing configuration for vehicle spawn.",
                    this);

                return;
            }

            int spawnIndex = _vehiclesByClientId.Count;
            Transform spawnPoint = spawnRegistry.GetSpawnPoint(spawnIndex);
            VehicleColorEntry colorEntry = colorRegistry.GetEntry(spawnIndex);

            NetworkObject vehicle = Instantiate(
                vehiclePrefab,
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