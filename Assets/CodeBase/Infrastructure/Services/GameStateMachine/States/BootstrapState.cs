using System;

namespace CodeBase.Infrastructure.Services.GameStateMachine.States
{
    public sealed class BootstrapState : IState
    {
        private readonly Action<Type> _enter;

        public BootstrapState(Action<Type> enter)
        {
            _enter = enter;
        }

        public void Enter()
        {
            _enter(typeof(GameLoopState));
        }

        public void Exit()
        {
        }
    }
}