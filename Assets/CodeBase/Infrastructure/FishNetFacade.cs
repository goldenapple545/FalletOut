using CodeBase.Gameplay.Network;

public interface IFishNetFacade
{
    bool IsClientStarted { get; }
    bool IsServerStarted { get; }

    void StartHost();
    void StartClient();
    void StartServer();
    void StopClient();
    void StopServer();
}

public sealed class FishNetFacade : IFishNetFacade
{
    private readonly NetworkRuntimeRoot _runtimeRoot;

    public FishNetFacade(NetworkRuntimeRoot runtimeRoot)
    {
        _runtimeRoot = runtimeRoot;
    }

    public bool IsClientStarted => _runtimeRoot.NetworkManager.IsClientStarted;
    public bool IsServerStarted => _runtimeRoot.NetworkManager.IsServerStarted;

    public void StartHost()
    {
        _runtimeRoot.NetworkManager.ServerManager.StartConnection();
        _runtimeRoot.NetworkManager.ClientManager.StartConnection();
    }

    public void StartClient()
    {
        _runtimeRoot.NetworkManager.ClientManager.StartConnection();
    }

    public void StartServer()
    {
        _runtimeRoot.NetworkManager.ServerManager.StartConnection();
    }

    public void StopClient()
    {
        _runtimeRoot.NetworkManager.ClientManager.StopConnection();
    }

    public void StopServer()
    {
        _runtimeRoot.NetworkManager.ServerManager.StopConnection(true);
    }
}