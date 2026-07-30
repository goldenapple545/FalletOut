using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.CodeBase.Gameplay.Network.Statistics;
using CodeBase.CodeBase.Gameplay.Network.UI.Match;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Infrastructure.Installers.Scene
{
    public sealed class MatchSceneInstaller : MonoInstaller
    {
        [UnityEngine.SerializeField, UnityEngine.Min(1)]
        private int damageHistoryCapacity = 256;

        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchHud matchHudPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VehicleDamageHistory>()
                .AsSingle()
                .WithArguments(damageHistoryCapacity)
                .NonLazy();

            Container.BindInterfacesAndSelfTo<PlayerNameResolver>()
                .AsSingle()
                .NonLazy();

            Container.Bind<MatchManager>().FromInstance(matchManager).AsSingle();

            Container.Bind<MatchHud>()
                .FromComponentInNewPrefab(matchHudPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}