// using System.Collections.Generic;
// using FishNet;
// using FishNet.Managing;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using Zenject;
//
// namespace CodeBase.Gameplay.Network.UI
// {
//     public class LobbyUIHost : MonoBehaviour
//     {
//         [Header("Host UI")]
//         [SerializeField] private GameObject hostPanel;
//         [SerializeField] private TMP_Text   serverNameText;
//         [SerializeField] private TMP_Text   hostPlayersText;
//         [SerializeField] private TMP_Text   waitPlayersText;
//         [SerializeField] private Button   hostBackButton;
//
//         [Header("Player Slots")]
//         [SerializeField] private PlayerSlotView slotPrefab;
//         [SerializeField] private Transform      slotsContainer;
//         [SerializeField] private int            maxPlayers = 3;
//
//         [Header("Battle")]
//         [SerializeField] private Button startBattleButton;
//
//         private LobbySessionService _lobbyService;
//
//         private readonly List<PlayerSlotView> _slots = new();
//         private NetworkManager _networkManager;
//         private string _serverName;
//         private bool _rolesSubscribed;
//
//         [Inject]
//         private void Construct(LobbySessionService lobbyService)
//         {
//             _lobbyService = lobbyService;
//         }
//         
//         public void Init()
//         {
//             hostBackButton.onClick.AddListener(OnBackClicked);
//
//             if (startBattleButton != null)
//                 startBattleButton.onClick.AddListener(OnStartBattleClicked);
//
//             SpawnSlots();
//             Hide();
//             
//             if (_lobbyService == null) return;
//
//             _lobbyService.ModeChanged += OnModeChanged;
//             _lobbyService.StatusChanged += OnStatusChanged;
//             _lobbyService.HostPlayersChanged += OnHostPlayersChanged;
//             _lobbyService.TransitionStateChanged += OnTransitionStateChanged;
//             _lobbyService.ServerNameChanged += OnServerNameChanged;
//
//             TrySubscribeToRoles();
//             RefreshInteractable();
//             OnModeChanged(_lobbyService.Mode);
//         }
//
//         private void OnDestroy()
//         {
//             hostBackButton.onClick.RemoveListener(OnBackClicked);
//             startBattleButton?.onClick.RemoveListener(OnStartBattleClicked);
//             
//             if (_lobbyService != null)
//             {
//                 _lobbyService.ModeChanged -= OnModeChanged;
//                 _lobbyService.StatusChanged -= OnStatusChanged;
//                 _lobbyService.HostPlayersChanged -= OnHostPlayersChanged;
//                 _lobbyService.TransitionStateChanged -= OnTransitionStateChanged;
//                 _lobbyService.ServerNameChanged -= OnServerNameChanged;
//             }
//
//             UnsubscribeFromRoles();
//         }
//
//         private void OnModeChanged(LobbyMode mode)
//         {
//             switch (mode)
//             {
//                 case LobbyMode.StartingHost:
//                     Show();
//                     break;
//
//                 case LobbyMode.Host:
//                     Show();
//                     //RefreshPlayerSlots(RoleAssigner.Instance?.Players ?? new Dictionary<int, PlayerLobbyEntry>());
//                     UpdateStartBattleInteractable();
//                     break;
//
//                 default:
//                     Hide();
//                     break;
//             }
//         }
//
//         private void OnStatusChanged(string status)
//         {
//             if (_lobbyService.Mode == LobbyMode.StartingHost || _lobbyService.Mode == LobbyMode.Host)
//                 SetStatus(status);
//         }
//
//         private void OnHostPlayersChanged(int count)
//         {
//             SetPlayersCount(count);
//         }
//
//         private void OnTransitionStateChanged(bool _)
//         {
//             RefreshInteractable();
//         }
//
//         private void OnServerNameChanged(string name)
//         {
//             _serverName = name;
//         }
//
//         private void RefreshInteractable()
//         {
//             bool interactable = !_lobbyService.IsTransitioning;
//             hostBackButton.interactable = interactable;
//             if (startBattleButton != null)
//                 startBattleButton.interactable = interactable && CanStartBattle();
//         }
//
//         private void OnBackClicked()
//         {
//             _lobbyService.StopHost();
//             //NetworkSceneService.Instance.ReturnToMain();
//         }
//
//         private void TrySubscribeToRoles()
//         {
//             // if (_rolesSubscribed || RoleAssigner.Instance == null)
//             //     return;
//
//             //RoleAssigner.Instance.PlayersChanged += OnRolesChanged;
//             _rolesSubscribed = true;
//         }
//
//         private void UnsubscribeFromRoles()
//         {
//             // if (!_rolesSubscribed || RoleAssigner.Instance == null)
//             //     return;
//
//             //RoleAssigner.Instance.PlayersChanged -= OnRolesChanged;
//             _rolesSubscribed = false;
//         }
//
//         private void OnRolesChanged()
//         {
//             //RefreshPlayerSlots(RoleAssigner.Instance.Players);
//             UpdateStartBattleInteractable();
//         }
//
//         private bool CanStartBattle()
//         {
//             _networkManager ??= InstanceFinder.NetworkManager;
//
//             return _networkManager != null
//                    && _networkManager.IsServerStarted;
//             // && RoleAssigner.Instance != null
//             // && RoleAssigner.Instance.HasMinimumPlayers();
//         }
//
//         private void UpdateStartBattleInteractable()
//         {
//             bool canStart = CanStartBattle();
//             SetStartBattleInteractable(canStart);
//         }
//
//         private void OnStartBattleClicked()
//         {
//             _networkManager ??= InstanceFinder.NetworkManager;
//             if (_networkManager == null || !_networkManager.IsServerStarted)
//                 return;
//
//             // if (RoleAssigner.Instance == null || !RoleAssigner.Instance.HasMinimumPlayers())
//             // {
//             //     SetStatus("Недостаточно игроков!");
//             //     return;
//             // }
//
//             SetStatus("Загрузка боевой сцены...");
//             SetStartBattleInteractable(false);
//             UnsubscribeFromRoles();
//         }
//
//         private void SpawnSlots()
//         {
//             _slots.Clear();
//
//             for (int i = 0; i < maxPlayers; i++)
//             {
//                 var slot = Instantiate(slotPrefab, slotsContainer);
//                 _slots.Add(slot);
//             }
//
//             InitSlotLabels();
//             ResetSlots();
//         }
//
//         private void InitSlotLabels()
//         {
//             if (_slots.Count == 0) return;
//
//             _slots[0].SetInfo("Игрок 1 Стрелок :");
//
//             for (int i = 1; i < _slots.Count; i++)
//                 _slots[i].SetInfo($"Игрок {i + 1} Оператор {i} :");
//         }
//
//         private void ResetSlots()
//         {
//             foreach (var slot in _slots)
//                 slot.SetConnected(false);
//         }
//
//         public void Show()
//         {
//             hostPanel.SetActive(true);
//         }
//
//         public void Hide()
//         {
//             hostPanel.SetActive(false);
//         }
//
//         public void SetStatus(string status)
//         {
//             if (serverNameText != null)
//                 serverNameText.text = $"{_serverName}: {status}";
//         }
//
//         public void SetPlayersCount(int count)
//         {
//             if (hostPlayersText != null)
//                 hostPlayersText.text = $"Игроков: {count}";
//         }
//
//         public void SetStartBattleInteractable(bool interactable)
//         {
//             if (startBattleButton != null)
//                 startBattleButton.interactable = interactable;
//
//             if (waitPlayersText != null)
//                 waitPlayersText.gameObject.SetActive(!interactable);
//         }
//
//         // public void RefreshPlayerSlots(IReadOnlyDictionary<int, PlayerLobbyEntry> assignedRoles)
//         // {
//         //     ResetSlots();
//         //
//         //     int nextOperatorSlot = 1;
//         //
//         //     foreach (var kvp in assignedRoles)
//         //     {
//         //         switch (kvp.Value.Role)
//         //         {
//         //             case PlayerRole.Shooter:
//         //                 SetSlotConnected(0, true);
//         //                 break;
//         //
//         //             case PlayerRole.DroneOperator:
//         //                 if (nextOperatorSlot < _slots.Count)
//         //                     SetSlotConnected(nextOperatorSlot, true);
//         //                 nextOperatorSlot++;
//         //                 break;
//         //         }
//         //     }
//         // }
//
//         private void SetSlotConnected(int index, bool connected)
//         {
//             if (index >= 0 && index < _slots.Count)
//                 _slots[index].SetConnected(connected);
//         }
//     }
// }