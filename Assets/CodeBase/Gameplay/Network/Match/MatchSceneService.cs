using System;
using CodeBase.Gameplay.Network;
using CodeBase.Gameplay.Network.Match;
using CodeBase.Infrastructure.Services.Session;
using FishNet.Managing;
using FishNet.Managing.Scened;

namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public sealed class MatchSceneService : IMatchSceneService
    {
        private readonly ISessionService _sessionService;
        private readonly NetworkManager _networkManager;
        private readonly LobbySessionService _lobbyService;

        public MatchSceneService(
            ISessionService sessionService,
            NetworkRuntimeRoot networkRoot,
            LobbySessionService lobbyService)
        {
            _sessionService = sessionService;
            _networkManager = networkRoot.NetworkManager;
            _lobbyService = lobbyService;
        }

        public void StartMatch()
        {
            if (!_sessionService.IsHostStarted)
            {
                throw new InvalidOperationException(
                    "Only an active host can start a match.");
            }

            string sceneName = _lobbyService.SelectedLevel != null
                ? _lobbyService.SelectedLevel.Id
                : "Parking Zone";

            SceneLoadData sceneLoadData = new SceneLoadData(sceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };

            _networkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        }
    }
}