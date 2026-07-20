using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure
{
    public sealed class AppReadyService : IAppReadyService
    {
        private readonly UniTaskCompletionSource _tcs = new();

        public UniTask WaitUntilReadyAsync() => _tcs.Task;

        public void MarkReady()
        {
            _tcs.TrySetResult();
        }
    }
}