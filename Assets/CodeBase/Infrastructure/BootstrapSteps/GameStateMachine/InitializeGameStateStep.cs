using System.Threading;
using CodeBase.Infrastructure.Services.GameStateMachine;
using CodeBase.Infrastructure.Services.GameStateMachine.States;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.BootstrapSteps.GameStateMachine
{
    public sealed class InitializeGameStateStep : IAppBootstrapStep
    {
        private readonly IGameStateMachine _stateMachine;

        public InitializeGameStateStep(IGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            _stateMachine.Enter<BootstrapState>();
            return UniTask.CompletedTask;
        }
    }
}