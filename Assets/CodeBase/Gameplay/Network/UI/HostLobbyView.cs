using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class HostLobbyView : LobbyPanelView
    {
        [SerializeField] private TMP_Text serverNameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text playersCountText;
        [SerializeField] private Button leaveHostButton;
        [SerializeField] private Button startMatchButton;

        private LobbySessionService _lobbyService;

        [Inject]
        private void Construct(LobbySessionService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        private void OnEnable()
        {
            leaveHostButton.onClick.AddListener(LeaveHost);
            startMatchButton.onClick.AddListener(StartMatch);

            if (_lobbyService == null)
                return;

            _lobbyService.ModeChanged += OnModeChanged;
            _lobbyService.StatusChanged += OnStatusChanged;
            _lobbyService.HostPlayersChanged += OnPlayersChanged;
            _lobbyService.ServerNameChanged += OnServerNameChanged;
            _lobbyService.TransitionStateChanged += OnTransitionChanged;

            ApplyMode(_lobbyService.Mode);
            OnServerNameChanged(_lobbyService.CurrentServerName);
            RefreshButtons();
        }

        private void OnDisable()
        {
            leaveHostButton.onClick.RemoveListener(LeaveHost);
            startMatchButton.onClick.RemoveListener(StartMatch);

            if (_lobbyService == null)
                return;

            _lobbyService.ModeChanged -= OnModeChanged;
            _lobbyService.StatusChanged -= OnStatusChanged;
            _lobbyService.HostPlayersChanged -= OnPlayersChanged;
            _lobbyService.ServerNameChanged -= OnServerNameChanged;
            _lobbyService.TransitionStateChanged -= OnTransitionChanged;
        }

        private void LeaveHost()
        {
            _lobbyService.StopHost();
        }

        private void StartMatch()
        {
            // На данном этапе кнопка только подтверждает flow.
            // Дальше здесь будет _matchStartService.RequestStartMatch().
            SetStatus("Подготовка матча...");
            startMatchButton.interactable = false;
        }

        private void OnModeChanged(LobbyMode mode)
        {
            ApplyMode(mode);
            RefreshButtons();
        }

        private void ApplyMode(LobbyMode mode)
        {
            bool visible = mode is LobbyMode.StartingHost or LobbyMode.Host;

            if (visible)
                Show();
            else 
                Hide();
        }

        private void OnStatusChanged(string status)
        {
            if (_lobbyService.Mode is LobbyMode.StartingHost or LobbyMode.Host)
                SetStatus(status);
        }

        private void OnPlayersChanged(int players)
        {
            if (playersCountText != null)
                playersCountText.text = $"Игроки: {players}";
        }

        private void OnServerNameChanged(string serverName)
        {
            if (serverNameText != null)
                serverNameText.text = serverName;
        }

        private void OnTransitionChanged(bool _)
        {
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            bool isHost = _lobbyService.Mode == LobbyMode.Host;
            bool canInteract = isHost && !_lobbyService.IsTransitioning;

            leaveHostButton.interactable = canInteract;
            startMatchButton.interactable = canInteract;

            // Позже:
            // startMatchButton.interactable =
            //     canInteract && _lobbyRosterService.CanHostStartMatch;
        }

        private void SetStatus(string status)
        {
            if (statusText != null)
                statusText.text = status;
        }
    }
}