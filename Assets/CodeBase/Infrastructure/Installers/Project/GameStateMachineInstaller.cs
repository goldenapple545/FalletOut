using CodeBase.Infrastructure.Services.GameStateMachine;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class GameStateMachineInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();
        }
    }
}