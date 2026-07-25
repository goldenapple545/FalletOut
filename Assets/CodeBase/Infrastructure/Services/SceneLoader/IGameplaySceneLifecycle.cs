using System;

namespace CodeBase.Infrastructure.Services.SceneLoader
{
    public interface IGameplaySceneLifecycle
    {
        bool IsGameplaySceneReady { get; }

        event Action GameplaySceneReady;
        event Action GameplaySceneUnloading;
    }
}