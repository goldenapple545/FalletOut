using CodeBase.Infrastructure.BootstrapSteps.Network;
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

            Container.Bind<ISessionService>().To<SessionService>().AsSingle();
        }
    }
}