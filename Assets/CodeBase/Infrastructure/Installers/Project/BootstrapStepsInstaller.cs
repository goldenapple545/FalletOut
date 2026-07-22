using CodeBase.Infrastructure.BootstrapSteps.GameStateMachine;
using CodeBase.Infrastructure.BootstrapSteps.StaticData;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class BootstrapStepsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<WarmupStaticDataStep>().AsSingle();
            Container.BindInterfacesAndSelfTo<InitializeGameStateStep>().AsSingle();
        }
    }
}