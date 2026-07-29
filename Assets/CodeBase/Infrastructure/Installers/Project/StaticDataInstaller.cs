using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.BootstrapSteps.StaticData;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class StaticDataInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IStaticDataService>().To<StaticDataService>().AsSingle();
        }
    }
}