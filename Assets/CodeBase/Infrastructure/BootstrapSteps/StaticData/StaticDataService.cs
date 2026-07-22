using System;
using System.Threading;
using CodeBase.Infrastructure.BootstrapSteps.StaticData.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.BootstrapSteps.StaticData
{
    public sealed class StaticDataService : IStaticDataService
    {
        private const string VehicleConfigPath = "StaticData/VehicleConfig";
        private const string CollisionDamageConfigPath = "StaticData/CollisionDamageConfig";
        private const string MatchRulesConfigPath = "StaticData/MatchRulesConfig";

        public VehicleConfig VehicleConfig { get; private set; }
        public CollisionDamageConfig CollisionDamageConfig { get; private set; }
        public MatchRulesConfig MatchRulesConfig { get; private set; }

        public async UniTask WarmupAsync(CancellationToken ct)
        {
            VehicleConfig = await LoadAsync<VehicleConfig>(VehicleConfigPath, ct);
            CollisionDamageConfig = await LoadAsync<CollisionDamageConfig>(CollisionDamageConfigPath, ct);
            MatchRulesConfig = await LoadAsync<MatchRulesConfig>(MatchRulesConfigPath, ct);
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