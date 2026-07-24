using System;
using CodeBase.Infrastructure.Services.Session;
using FishNet.Managing;
using FishNet.Managing.Scened;

namespace CodeBase.Gameplay.Network.Match
{
    public sealed class MatchSceneService : IMatchSceneService
    {
        private const string ArenaSceneName = "Arena_01";

        private readonly ISessionService _sessionService;
        private readonly NetworkManager _networkManager;

        public MatchSceneService(
            ISessionService sessionService,
            NetworkRuntimeRoot networkRoot)
        {
            _sessionService = sessionService;
            _networkManager = networkRoot.NetworkManager;
        }

        public void StartMatch()
        {
            if (!_sessionService.IsHostStarted)
            {
                throw new InvalidOperationException(
                    "Only an active host can start a match.");
            }

            SceneLoadData sceneLoadData = new SceneLoadData(ArenaSceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };

            _networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }
    }
}