using System.Threading;
using CodeBase.CodeBase.Data;
using CodeBase.Data;
using Cysharp.Threading.Tasks;

namespace CodeBase.CodeBase.Infrastructure.Services.StaticData
{
    public interface IStaticDataService
    {
        MatchRulesConfig MatchRulesConfig { get; }
        
        UniTask WarmupAsync(CancellationToken ct);
    }
}