using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.CodeBase.Gameplay.Network.Statistics;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Infrastructure.Installers.Scene
{
    public sealed class MatchSceneInstaller : MonoInstaller
    {
        [UnityEngine.SerializeField, UnityEngine.Min(1)]
        private int damageHistoryCapacity = 256;

        [SerializeField] private MatchManager matchManager;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VehicleDamageHistory>()
                .AsSingle()
                .WithArguments(damageHistoryCapacity)
                .NonLazy();

            Container.Bind<MatchManager>().FromInstance(matchManager).AsSingle();
        }
    }
}