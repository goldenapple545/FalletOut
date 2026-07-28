using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class LobbyScreenController : IInitializable, System.IDisposable
    {
        private readonly LobbySessionService _lobbyService;
        private readonly LobbyBrowserView _browserView;
        private readonly HostLobbyView _hostView;
        private readonly ClientLobbyView _clientView;

        public LobbyScreenController(
            LobbySessionService lobbyService,
            LobbyBrowserView browserView,
            HostLobbyView hostView,
            ClientLobbyView clientView)
        {
            Debug.Log($"[LobbyScreenController]: constrcu");
            _lobbyService = lobbyService;
            _browserView = browserView;
            _hostView = hostView;
            _clientView = clientView;
        }

        public void Initialize()
        {
            Debug.Log($"[LobbyScreenController]: init");
            _lobbyService.ModeChanged += OnLobbyModeChanged;
            ApplyMode(_lobbyService.Mode);
        }

        public void Dispose()
        {
            _lobbyService.ModeChanged -= OnLobbyModeChanged;
        }

        private void OnLobbyModeChanged(LobbyMode mode) =>
            ApplyMode(mode);

        private void ApplyMode(LobbyMode mode)
        {
            Debug.Log($"[LobbyScreenController]: Apply mode {mode}");
            _browserView.Hide();
            _hostView.Hide();
            _clientView.Hide();

            switch (mode)
            {
                case LobbyMode.Offline:
                case LobbyMode.Searching:
                    _browserView.Show();
                    break;

                case LobbyMode.Connecting:
                case LobbyMode.Client:
                    _clientView.Show();
                    break;

                case LobbyMode.StartingHost:
                case LobbyMode.Host:
                    _hostView.Show();
                    break;
            }
        }
    }
}