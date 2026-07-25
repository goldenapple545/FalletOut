using CodeBase.Infrastructure.Services.SceneLoader;

namespace CodeBase.Infrastructure.Services.GameStateMachine.States
{
    public sealed class LoadGameplaySceneState : IState
    {
        private readonly GameplaySceneLifecycle _sceneLifecycle;

        public LoadGameplaySceneState(GameplaySceneLifecycle sceneLifecycle)
        {
            _sceneLifecycle = sceneLifecycle;
        }

        public void Enter()
        {
            _sceneLifecycle.NotifyGameplaySceneUnloading();
        }

        public void Exit()
        {
        }
    }
}