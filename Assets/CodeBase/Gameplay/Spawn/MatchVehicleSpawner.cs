using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

namespace CodeBase.Gameplay.Spawn
{
    public sealed class MatchVehicleSpawner : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private NetworkObject vehiclePrefab;
        [SerializeField] private ArenaSpawnRegistry spawnRegistry;

        private readonly Dictionary<int, NetworkObject> _vehiclesByClientId = new();

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkManager.SceneManager.OnClientPresenceChangeEnd +=
                HandleClientPresenceChanged;
            
            foreach (NetworkConnection connection in ServerManager.Clients.Values)
            {
                if (_vehiclesByClientId.ContainsKey(connection.ClientId))
                    return;
                
                if (connection.IsActive)
                    SpawnVehicleFor(connection);
            }
        }

        public override void OnStopServer()
        {
            NetworkManager.SceneManager.OnClientPresenceChangeEnd -=
                HandleClientPresenceChanged;

            _vehiclesByClientId.Clear();

            base.OnStopServer();
        }

        private void HandleClientPresenceChanged(ClientPresenceChangeEventArgs args)
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
                    $"[{nameof(MatchVehicleSpawner)}] Vehicle prefab is not assigned.",
                    this);

                return;
            }

            if (spawnRegistry == null || spawnRegistry.Count == 0)
            {
                Debug.LogError(
                    $"[{nameof(MatchVehicleSpawner)}] Spawn registry is not configured.",
                    this);

                return;
            }

            int spawnIndex = _vehiclesByClientId.Count;
            Transform spawnPoint = spawnRegistry.GetSpawnPoint(spawnIndex);

            NetworkObject vehicle = Instantiate(
                vehiclePrefab,
                spawnPoint.position,
                spawnPoint.rotation);

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