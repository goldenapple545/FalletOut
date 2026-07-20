using CodeBase.Infrastructure.BootstrapSteps;
using CodeBase.Infrastructure.BootstrapSteps.GameStateMachine;
using CodeBase.Infrastructure.BootstrapSteps.Network;
using CodeBase.Infrastructure.BootstrapSteps.StaticData;
using CodeBase.Infrastructure.Services.GameStateMachine;
using CodeBase.Infrastructure.Services.SceneLoader;
using CodeBase.Infrastructure.Services.Session;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public NetworkRuntimeRoot NetworkRuntimeRootPrefab;

        public override void InstallBindings()
        {
            Container.Bind<IAppReadyService>().To<AppReadyService>().AsSingle();

            Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();
            Container.Bind<ISceneLoader>().To<UnitySceneLoader>().AsSingle();
            Container.Bind<ISessionService>().To<SessionService>().AsSingle();

            Container.BindFactory<NetworkRuntimeRoot, NetworkRuntimeRoot.Factory>()
                .FromComponentInNewPrefab(NetworkRuntimeRootPrefab)
                .AsSingle();

            Container.BindInterfacesAndSelfTo<CreateNetworkRuntimeStep>().AsSingle();
            Container.BindInterfacesAndSelfTo<WarmupStaticDataStep>().AsSingle();
            Container.BindInterfacesAndSelfTo<InitializeGameStateStep>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<AppBootstrapper>().AsSingle().NonLazy();
        }
    }
}