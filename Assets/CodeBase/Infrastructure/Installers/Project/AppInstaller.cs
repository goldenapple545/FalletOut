using CodeBase.Infrastructure;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class AppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAppReadyService>().To<AppReadyService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AppBootstrapper>().AsSingle().NonLazy();
        }
    }
}