using System;
using CodeBase.Gameplay.Network;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using Zenject;

namespace CodeBase.Infrastructure.Services.Session
{
    public sealed class SessionService : ISessionService, IInitializable, IDisposable
    {
        private readonly NetworkRuntimeRoot _networkRuntimeRoot;

        private NetworkManager _networkManager;
        private bool _initialized;
        
        public SessionService(NetworkRuntimeRoot networkRuntimeRoot)
        {
            _networkRuntimeRoot = networkRuntimeRoot;
        }

        public bool IsClientStarted => NetworkManager.IsClientStarted;
        public bool IsServerStarted => NetworkManager.IsServerStarted;
        public bool IsHostStarted => NetworkManager.IsHostStarted;
        public int ConnectedClientsCount => NetworkManager.ServerManager.Clients.Count;

        public event Action ClientAuthenticated;
        public event Action<ClientConnectionStateArgs> ClientConnectionStateChanged;
        public event Action<ServerConnectionStateArgs> ServerConnectionStateChanged;
        public event Action<NetworkConnection, RemoteConnectionStateArgs> RemoteConnectionStateChanged;

        private NetworkManager NetworkManager => _networkRuntimeRoot.NetworkManager;

        public void Initialize()
        {
            if (_initialized)
                return;

            _networkManager = _networkRuntimeRoot.NetworkManager;

            if (_networkManager == null)
            {
                throw new InvalidOperationException(
                    "NetworkRuntimeRoot.NetworkManager is null. " +
                    "Проверь, что NetworkRuntimeRoot заполняет ссылку до Initialize, " +
                    "либо назначь NetworkManager в prefab через Inspector.");
            }

            _networkManager.ClientManager.OnAuthenticated += HandleAuthenticated;
            _networkManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState += HandleServerConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;

            _initialized = true;
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

        public void StopClient()
        {
            if (NetworkManager.IsClientStarted)
                NetworkManager.ClientManager.StopConnection();
        }

        public void StopServer()
        {
            if (NetworkManager.IsServerStarted)
                NetworkManager.ServerManager.StopConnection(true);
        }

        public void Stop()
        {
            StopClient();
            StopServer();
        }

        private void HandleAuthenticated() =>
            ClientAuthenticated?.Invoke();

        private void HandleClientConnectionState(ClientConnectionStateArgs args) =>
            ClientConnectionStateChanged?.Invoke(args);

        private void HandleServerConnectionState(ServerConnectionStateArgs args) =>
            ServerConnectionStateChanged?.Invoke(args);

        private void HandleRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args) =>
            RemoteConnectionStateChanged?.Invoke(conn, args);

        public void Dispose()
        {
            NetworkManager.ClientManager.OnAuthenticated -= HandleAuthenticated;
            NetworkManager.ClientManager.OnClientConnectionState -= HandleClientConnectionState;
            NetworkManager.ServerManager.OnServerConnectionState -= HandleServerConnectionState;
            NetworkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
        }
    }
}