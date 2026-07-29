using System.Threading;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.BootstrapSteps.StaticData
{
    public sealed class WarmupStaticDataStep : IAppBootstrapStep
    {
        private readonly IStaticDataService _staticDataService;

        public WarmupStaticDataStep(IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            return _staticDataService.WarmupAsync(ct);
        }
    }
}