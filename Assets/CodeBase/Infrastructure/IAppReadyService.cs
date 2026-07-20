using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure
{
    public interface IAppReadyService
    {
        UniTask WaitUntilReadyAsync();
        void MarkReady();
    }
}