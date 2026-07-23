using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public class LobbyUIClient : MonoBehaviour
    {
        [Header("Lobby List")]
        [SerializeField] private GameObject lobbyListPanel;
        [SerializeField] private Transform  lobbyListContainer;
        [SerializeField] private Button   lobbyButtonPrefab;
        
        [SerializeField] private Button   refreshButton;
        [SerializeField] private Button   createLobbyButton;
        
        [SerializeField] private TMP_Text   lobbyStatusText;

        [Header("Client Connected")]
        [SerializeField] private GameObject clientPanel;
        [SerializeField] private TMP_Text   clientStatusText;
        [SerializeField] private Button   clientBackButton;

        private LobbySessionService   _lobbyService;

        private readonly List<Button> _spawnedButtons = new();
        private string _currentServerName;

        [Inject]
        private void Construct(LobbySessionService lobbyService)
        {
            _lobbyService = lobbyService;
        }
        
        public void Init()
        {
            refreshButton.onClick.AddListener(OnRefreshClicked);
            createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
            clientBackButton.onClick.AddListener(OnBackClicked);

            Hide();
            
            if (_lobbyService == null) return;

            _lobbyService.ModeChanged += OnModeChanged;
            _lobbyService.StatusChanged += OnStatusChanged;
            _lobbyService.ServerListChanged += OnServerListChanged;
            _lobbyService.TransitionStateChanged += OnTransitionStateChanged;
            _lobbyService.ServerNameChanged += OnServerNameChanged;

            RefreshInteractable();
            OnModeChanged(_lobbyService.Mode);
        }

        private void OnDestroy()
        {
            refreshButton.onClick.RemoveListener(OnRefreshClicked);
            createLobbyButton.onClick.RemoveListener(OnCreateLobbyClicked);
            clientBackButton.onClick.RemoveListener(OnBackClicked);
            
            if (_lobbyService == null) return;

            _lobbyService.ModeChanged -= OnModeChanged;
            _lobbyService.StatusChanged -= OnStatusChanged;
            _lobbyService.ServerListChanged -= OnServerListChanged;
            _lobbyService.TransitionStateChanged -= OnTransitionStateChanged;
            _lobbyService.ServerNameChanged -= OnServerNameChanged;
        }

        private void OnRefreshClicked()
        {
            _lobbyService.RefreshLobbies();
        }

        private void OnCreateLobbyClicked()
        {
            _lobbyService.StartHost();
        }

        private void OnBackClicked()
        {
            _lobbyService.StopClient();
            _lobbyService.RefreshLobbies();
        }

        private void OnLobbySelected(IPEndPoint endPoint)
        {
            _lobbyService.ConnectToServer(endPoint);
        }

        private void OnTransitionStateChanged(bool _)
        {
            RefreshInteractable();
        }

        private void RefreshInteractable()
        {
            bool interactable = !_lobbyService.IsTransitioning;

            refreshButton.interactable = interactable;
            createLobbyButton.interactable = interactable;
            clientBackButton.interactable = interactable;
        }

        private void OnModeChanged(LobbyMode mode)
        {
            switch (mode)
            {
                case LobbyMode.Offline:
                case LobbyMode.Searching:
                    ShowLobbyList();
                    _lobbyService.RefreshLobbies();
                    break;

                case LobbyMode.Connecting:
                    ShowConnected();
                    break;

                case LobbyMode.Client:
                    ShowConnected();
                    //RegisterLocalRole();
                    break;

                case LobbyMode.StartingHost:
                case LobbyMode.Host:
                    Hide();
                    break;
            }
        }

        private void OnStatusChanged(string status)
        {
            switch (_lobbyService.Mode)
            {
                case LobbyMode.Offline:
                case LobbyMode.Searching:
                    SetLobbyStatus(status);
                    break;

                case LobbyMode.Connecting:
                case LobbyMode.Client:
                    SetConnectedStatus(status);
                    break;
            }
        }

        private void OnServerListChanged(IReadOnlyList<ServerInfo> servers)
        {
            RebuildLobbyList(servers);
        }

        private void OnServerNameChanged(string name)
        {
            _currentServerName = name;
        }

        public void ShowLobbyList()
        {
            lobbyListPanel.SetActive(true);
            clientPanel.SetActive(false);
        }

        public void ShowConnected()
        {
            lobbyListPanel.SetActive(false);
            clientPanel.SetActive(true);
        }

        public void Hide()
        {
            lobbyListPanel.SetActive(false);
            clientPanel.SetActive(false);
        }

        public void SetConnectedStatus(string status)
        {
            if (clientStatusText != null)
                clientStatusText.text = _currentServerName + " - " + status;
        }

        public void SetLobbyStatus(string status)
        {
            if (lobbyStatusText != null)
                lobbyStatusText.text = status;
        }

        public void RebuildLobbyList(IReadOnlyList<ServerInfo> servers)
        {
            ClearLobbyButtons();

            foreach (var info in servers)
            {
                var btn = Instantiate(lobbyButtonPrefab, lobbyListContainer);
                _spawnedButtons.Add(btn);

                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = info.DisplayLabel;

                var captured = info.EndPoint;
                btn.onClick.AddListener(() => OnLobbySelected(captured));
            }
        }

        private void ClearLobbyButtons()
        {
            foreach (var btn in _spawnedButtons)
                if (btn != null) Destroy(btn.gameObject);

            _spawnedButtons.Clear();
        }
    }
}