using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class LobbyBrowserView : LobbyPanelView
    {
        [Header("Lobby list")]
        [SerializeField] private Transform serverListContainer;
        [SerializeField] private LobbyServerItemView serverItemPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button createHostButton;
        [SerializeField] private TMP_Text statusText;

        private readonly List<LobbyServerItemView> _items = new();
        private LobbySessionService _lobbyService;

        [Inject]
        private void Construct(LobbySessionService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        private void OnEnable()
        {
            refreshButton.onClick.AddListener(Refresh);
            createHostButton.onClick.AddListener(CreateHost);

            if (_lobbyService == null)
                return;

            _lobbyService.ModeChanged += OnModeChanged;
            _lobbyService.StatusChanged += SetStatus;
            _lobbyService.ServerListChanged += RebuildServerList;
            _lobbyService.TransitionStateChanged += RefreshInteractable;

            ApplyMode(_lobbyService.Mode);
            RebuildServerList(_lobbyService.FoundServers);
            RefreshInteractable(_lobbyService.IsTransitioning);
        }

        private void OnDisable()
        {
            refreshButton.onClick.RemoveListener(Refresh);
            createHostButton.onClick.RemoveListener(CreateHost);

            if (_lobbyService == null)
                return;

            _lobbyService.ModeChanged -= OnModeChanged;
            _lobbyService.StatusChanged -= SetStatus;
            _lobbyService.ServerListChanged -= RebuildServerList;
            _lobbyService.TransitionStateChanged -= RefreshInteractable;
        }

        private void Refresh()
        {
            _lobbyService.RefreshLobbies();
        }

        private void CreateHost()
        {
            _lobbyService.StartHost();
        }

        private void OnModeChanged(LobbyMode mode)
        {
            ApplyMode(mode);
        }

        private void ApplyMode(LobbyMode mode)
        {
            bool visible = mode is LobbyMode.Offline or LobbyMode.Searching;

            if (visible)
                Show();
            else 
                Hide();

            if (visible && mode == LobbyMode.Offline)
                _lobbyService.RefreshLobbies();
        }

        private void RefreshInteractable(bool isTransitioning)
        {
            bool canInteract = !isTransitioning;

            refreshButton.interactable = canInteract;
            createHostButton.interactable = canInteract;
        }

        private void SetStatus(string status)
        {
            if (statusText != null)
                statusText.text = status;
        }

        private void RebuildServerList(IReadOnlyList<ServerInfo> servers)
        {
            ClearItems();

            foreach (ServerInfo info in servers)
            {
                LobbyServerItemView item =
                    Instantiate(serverItemPrefab, serverListContainer);

                item.Bind(info, OnServerSelected);
                _items.Add(item);
            }
        }

        private void OnServerSelected(IPEndPoint endpoint)
        {
            _lobbyService.ConnectToServer(endpoint);
        }

        private void ClearItems()
        {
            foreach (LobbyServerItemView item in _items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _items.Clear();
        }
    }
}