using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class ClientLobbyView : LobbyPanelView
    {
        [SerializeField] private TMP_Text serverNameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button leaveButton;

        private LobbySessionService _lobbyService;

        [Inject]
        private void Construct(LobbySessionService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        private void OnEnable()
        {
            leaveButton.onClick.AddListener(Leave);

            if (_lobbyService == null)
                return;

            _lobbyService.ModeChanged += OnModeChanged;
            _lobbyService.StatusChanged += OnStatusChanged;
            _lobbyService.ServerNameChanged += OnServerNameChanged;
            _lobbyService.TransitionStateChanged += OnTransitionChanged;

            ApplyMode(_lobbyService.Mode);
            OnServerNameChanged(_lobbyService.CurrentServerName);
            RefreshInteractable();
        }

        private void OnDisable()
        {
            leaveButton.onClick.RemoveListener(Leave);

            if (_lobbyService == null)
                return;

            _lobbyService.ModeChanged -= OnModeChanged;
            _lobbyService.StatusChanged -= OnStatusChanged;
            _lobbyService.ServerNameChanged -= OnServerNameChanged;
            _lobbyService.TransitionStateChanged -= OnTransitionChanged;
        }

        private void Leave()
        {
            _lobbyService.StopClient();
        }

        private void OnModeChanged(LobbyMode mode)
        {
            ApplyMode(mode);
            RefreshInteractable();
        }

        private void ApplyMode(LobbyMode mode)
        {
            bool visible = mode is LobbyMode.Connecting or LobbyMode.Client;

            if (visible)
                Show();
            else 
                Hide();
        }

        private void OnStatusChanged(string status)
        {
            if (_lobbyService.Mode is LobbyMode.Connecting or LobbyMode.Client)
            {
                if (statusText != null)
                    statusText.text = status;
            }
        }

        private void OnServerNameChanged(string serverName)
        {
            if (serverNameText != null)
                serverNameText.text = serverName;
        }

        private void OnTransitionChanged(bool _)
        {
            RefreshInteractable();
        }

        private void RefreshInteractable()
        {
            leaveButton.interactable = !_lobbyService.IsTransitioning;
        }
    }
}