using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.GameStateMachine.States;
using CodeBase.Infrastructure.Services.SceneLoader;

namespace CodeBase.Infrastructure.Services.GameStateMachine
{
    public sealed class GameStateMachine : IGameStateMachine
    {
        private readonly Dictionary<Type, IState> _states;
        private IState _currentState;

        public GameStateMachine(GameplaySceneLifecycle SceneLifecycle)
        {
            _states = new Dictionary<Type, IState>();

            _states[typeof(BootstrapState)] = new BootstrapState(EnterByType);
            _states[typeof(LoadGameplaySceneState)] = new LoadGameplaySceneState(SceneLifecycle);
            _states[typeof(GameLoopState)] = new GameLoopState(SceneLifecycle);
        }

        public void Enter<TState>() where TState : class, IState
        {
            EnterByType(typeof(TState));
        }

        private void EnterByType(Type stateType)
        {
            _currentState?.Exit();

            IState state = _states[stateType];
            _currentState = state;
            state.Enter();
        }
    }
}