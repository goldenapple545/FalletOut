using CodeBase.Infrastructure.Services.GameStateMachine.States;

namespace CodeBase.Infrastructure.Services.GameStateMachine
{
    public interface IGameStateMachine
    {
        void Enter<TState>() where TState : class, IState;
    }
}