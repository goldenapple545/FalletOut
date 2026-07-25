using CodeBase.Infrastructure.Services.SceneLoader;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class SceneServicesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameplaySceneLifecycle>()
                .AsSingle();
            
            Container.Bind<ISceneLoader>().To<UnitySceneLoader>().AsSingle();
        }
    }
}