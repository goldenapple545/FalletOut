namespace CodeBase.Infrastructure.Services.Session
{
    public interface ISessionService
    {
        void StartHost();
        void StartClient(string address);
        void Stop();
    }
}