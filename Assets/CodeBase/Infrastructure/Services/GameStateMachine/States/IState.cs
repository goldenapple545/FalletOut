namespace CodeBase.Infrastructure.Services.GameStateMachine.States
{
    public interface IState
    {
        void Enter();
        void Exit();
    }
}