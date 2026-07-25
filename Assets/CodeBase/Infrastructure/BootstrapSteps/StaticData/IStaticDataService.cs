using System.Threading;
using CodeBase.Data;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.BootstrapSteps.StaticData
{
    public interface IStaticDataService
    {
        VehicleConfig VehicleConfig { get; }
        CollisionDamageConfig CollisionDamageConfig { get; }
        MatchRulesConfig MatchRulesConfig { get; }
        
        UniTask WarmupAsync(CancellationToken ct);
    }
}