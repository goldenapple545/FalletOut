using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Gameplay.Network;
using CodeBase.Gameplay.Network.Match;
using CodeBase.Infrastructure.Services.SceneLoader;
using CodeBase.Infrastructure.Services.Session;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class NetworkInstaller : MonoInstaller
    {
        [SerializeField] private NetworkRuntimeRoot networkRuntimeRootPrefab;

        public override void InstallBindings()
        {
            Container.Bind<NetworkRuntimeRoot>()
                .FromComponentInNewPrefab(networkRuntimeRootPrefab)
                .AsSingle()
                .NonLazy();

            Container.Bind<NameLanDiscoveryTransport>()
                .FromMethod(_ => Container.Resolve<NetworkRuntimeRoot>()
                    .GetComponentInChildren<NameLanDiscoveryTransport>())
                .AsSingle();

            Container.Bind<LobbySessionService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<SessionService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<MatchSceneService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<FishNetSceneFlowAdapter>()
                .AsSingle()
                .NonLazy();
        }
    }
}