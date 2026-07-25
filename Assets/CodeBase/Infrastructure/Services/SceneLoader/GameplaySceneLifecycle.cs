using System;
using CodeBase.Infrastructure.Services.GameStateMachine.States;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.SceneLoader
{
    public sealed class GameplaySceneLifecycle : IGameplaySceneLifecycle
    {
        public bool IsGameplaySceneReady { get; private set; }

        public event Action GameplaySceneReady;
        public event Action GameplaySceneUnloading;

        public void NotifyGameplaySceneReady()
        {
            if (IsGameplaySceneReady)
                return;

            IsGameplaySceneReady = true;

            Debug.Log("[SceneLifecycle] Gameplay scene is ready.");

            GameplaySceneReady?.Invoke();
        }

        public void NotifyGameplaySceneUnloading()
        {
            if (!IsGameplaySceneReady)
                return;

            IsGameplaySceneReady = false;

            Debug.Log("[SceneLifecycle] Gameplay scene is unloading.");

            GameplaySceneUnloading?.Invoke();
        }
    }
}