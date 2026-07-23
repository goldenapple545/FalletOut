using System;
using FishNet.Connection;
using FishNet.Transporting;

namespace CodeBase.Infrastructure.Services.Session
{
    public interface ISessionService
    {
        bool IsClientStarted { get; }
        bool IsServerStarted { get; }
        bool IsHostStarted { get; }

        int ConnectedClientsCount { get; }

        event Action<ClientConnectionStateArgs> ClientConnectionStateChanged;
        event Action ClientAuthenticated;
        event Action<ServerConnectionStateArgs> ServerConnectionStateChanged;
        event Action<NetworkConnection, RemoteConnectionStateArgs> RemoteConnectionStateChanged;

        void StartHost(string localAddress = "127.0.0.1");
        void StartClient(string address);
        void StopClient();
        void StopServer();
        void Stop();
    }
}