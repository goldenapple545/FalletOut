using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.GameStateMachine.States;

namespace CodeBase.Infrastructure.Services.GameStateMachine
{
    public sealed class GameStateMachine : IGameStateMachine
    {
        private readonly Dictionary<Type, IState> _states;
        private IState _currentState;

        public GameStateMachine(
            BootstrapState bootstrapState,
            GameLoopState GameLoopState)
        {
            _states = new Dictionary<Type, IState>
            {
                [typeof(BootstrapState)] = bootstrapState,
                [typeof(GameLoopState)] = GameLoopState
            };
        }

        public void Enter<TState>() where TState : class, IState
        {
            if (_currentState != null)
                _currentState.Exit();

            var state = (TState)_states[typeof(TState)];
            _currentState = state;
            state.Enter();
        }
    }
}