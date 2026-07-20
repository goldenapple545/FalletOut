using System.Threading;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.BootstrapSteps.StaticData
{
    public interface IStaticDataService
    {
        UniTask WarmupAsync(CancellationToken ct);
    }
}