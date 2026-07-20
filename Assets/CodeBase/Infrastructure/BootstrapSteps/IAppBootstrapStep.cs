using System.Threading;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.BootstrapSteps
{
    public interface IAppBootstrapStep
    {
        UniTask ExecuteAsync(CancellationToken ct);
    }
}