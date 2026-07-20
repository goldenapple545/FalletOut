using UnityEngine;

namespace CodeBase.Infrastructure.Services.GameStateMachine.States
{
    public sealed class GameLoopState : IState
    {
        public void Enter()
        {
            Debug.Log("Entered MainMenuState");
        }

        public void Exit()
        {
        }
    }
}