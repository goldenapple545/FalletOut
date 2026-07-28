using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Spawn
{
    public sealed class MatchVehicleSpawner : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private NetworkObject vehiclePrefab;
        [SerializeField] private ArenaSpawnRegistry spawnRegistry;

        private readonly Dictionary<int, NetworkObject> _vehiclesByClientId =
            new();

        private DiContainer _sceneContainer;

        [Inject]
        private void Construct(DiContainer sceneContainer)
        {
            _sceneContainer = sceneContainer;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkManager.SceneManager.OnClientPresenceChangeEnd +=
                HandleClientPresenceChanged;

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

            if (vehiclePrefab == null)
            {
                Debug.LogError(
                    $"[{nameof(MatchVehicleSpawner)}] " +
                    "Vehicle prefab is not assigned.",
                    this);

                return;
            }

            if (spawnRegistry == null || spawnRegistry.Count == 0)
            {
                Debug.LogError(
                    $"[{nameof(MatchVehicleSpawner)}] " +
                    "Spawn registry is not configured.",
                    this);

                return;
            }

            if (_sceneContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(MatchVehicleSpawner)}] " +
                    "Scene DI container is not ready.",
                    this);

                return;
            }

            int spawnIndex = _vehiclesByClientId.Count;
            Transform spawnPoint =
                spawnRegistry.GetSpawnPoint(spawnIndex);

            NetworkObject vehicle = Instantiate(
                vehiclePrefab,
                spawnPoint.position,
                spawnPoint.rotation);

            // Важно: здесь именно SceneContext container.
            _sceneContainer.InjectGameObject(vehicle.gameObject);

            ServerManager.Spawn(vehicle, connection);

            _vehiclesByClientId.Add(connection.ClientId, vehicle);

            Debug.Log(
                $"[{nameof(MatchVehicleSpawner)}] " +
                $"Spawned vehicle for client={connection.ClientId}, " +
                $"position={spawnPoint.position}.",
                this);
        }
    }
}