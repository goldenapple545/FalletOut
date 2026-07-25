using System;
using CodeBase.Gameplay.Network;
using CodeBase.Infrastructure.Services.GameStateMachine;
using CodeBase.Infrastructure.Services.GameStateMachine.States;
using FishNet.Managing;
using FishNet.Managing.Scened;
using Zenject;

namespace CodeBase.Infrastructure.Services.SceneLoader
{
    public sealed class FishNetSceneFlowAdapter : IInitializable, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly IGameStateMachine _gameStateMachine;

        public FishNetSceneFlowAdapter(
            NetworkRuntimeRoot networkRoot,
            IGameStateMachine gameStateMachine)
        {
            _networkManager = networkRoot.NetworkManager;
            _gameStateMachine = gameStateMachine;
        }

        public void Initialize()
        {
            _networkManager.SceneManager.OnLoadEnd += OnLoadEnd;
            _networkManager.SceneManager.OnUnloadStart += OnUnloadStart;
        }

        public void Dispose()
        {
            if (_networkManager == null)
                return;

            _networkManager.SceneManager.OnLoadEnd -= OnLoadEnd;
            _networkManager.SceneManager.OnUnloadStart -= OnUnloadStart;
        }

        private void OnLoadEnd(SceneLoadEndEventArgs args)
        {
            _gameStateMachine.Enter<GameLoopState>();
        }

        private void OnUnloadStart(SceneUnloadStartEventArgs args)
        {
            _gameStateMachine.Enter<LoadGameplaySceneState>();
        }
    }
}