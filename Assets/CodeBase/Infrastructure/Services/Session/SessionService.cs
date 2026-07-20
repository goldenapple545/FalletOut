using CodeBase.Infrastructure.BootstrapSteps.Network;
using FishNet.Managing;

namespace CodeBase.Infrastructure.Services.Session
{
    public sealed class SessionService : ISessionService
    {
        private readonly NetworkRuntimeRoot _networkRuntimeRoot;

        public SessionService(NetworkRuntimeRoot networkRuntimeRoot)
        {
            _networkRuntimeRoot = networkRuntimeRoot;
        }

        public void StartHost()
        {
            NetworkManager networkManager = _networkRuntimeRoot.NetworkManager;

            networkManager.ServerManager.StartConnection();
            networkManager.ClientManager.StartConnection();
        }

        public void StartClient(string address)
        {
            NetworkManager networkManager = _networkRuntimeRoot.NetworkManager;

            networkManager.TransportManager.Transport.SetClientAddress(address);
            networkManager.ClientManager.StartConnection();
        }

        public void Stop()
        {
            NetworkManager networkManager = _networkRuntimeRoot.NetworkManager;

            if (networkManager.IsClientStarted)
                networkManager.ClientManager.StopConnection();

            if (networkManager.IsServerStarted)
                networkManager.ServerManager.StopConnection(true);
        }
    }
}