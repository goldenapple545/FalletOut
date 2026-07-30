using System;
using System.Threading;
using CodeBase.CodeBase.Data;
using CodeBase.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.CodeBase.Infrastructure.Services.StaticData
{
    public sealed class StaticDataService : IStaticDataService
    {
        private const string MatchRulesConfigPath = "StaticData/MatchRulesConfig";
        private const string LevelsRegistryPath = "StaticData/Levels/LevelsRegistry";
        private const string VehiclesRegistryPath = "StaticData/Vehicles/VehiclesRegistry";

        public MatchRulesConfig MatchRulesConfig { get; private set; }
        public LevelsRegistry LevelsRegistry { get; private set; }
        public VehiclesRegistry VehiclesRegistry { get; private set; }

        public async UniTask WarmupAsync(CancellationToken ct)
        {
            MatchRulesConfig = await LoadAsync<MatchRulesConfig>(MatchRulesConfigPath, ct);
            LevelsRegistry = await LoadAsync<LevelsRegistry>(LevelsRegistryPath, ct);
            VehiclesRegistry = await LoadAsync<VehiclesRegistry>(VehiclesRegistryPath, ct);
        }

        private static async UniTask<T> LoadAsync<T>(string path, CancellationToken ct)
            where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);

            await request.ToUniTask(cancellationToken: ct);

            T asset = request.asset as T;
            if (asset == null)
                throw new Exception($"Static data asset of type {typeof(T).Name} not found at Resources/{path}");

            return asset;
        }
    }
}