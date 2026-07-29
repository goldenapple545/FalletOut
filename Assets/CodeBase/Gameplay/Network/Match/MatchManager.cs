using System;
using System.Collections.Generic;
using CodeBase.CodeBase.Data;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using R3;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public sealed class MatchManager : NetworkBehaviour
    {
        private readonly SyncVar<MatchPhase> _phase = new();
        private readonly SyncVar<int> _winnerObjectId = new(-1);
        private readonly SyncVar<string> _winnerName = new(string.Empty);


        private readonly List<PlayerMatchState> _players = new();
        private IMatchMode _mode;

        private IStaticDataService _staticDataService;
        private MatchRulesConfig _rules;

        private int _expectedPlayerCount;
        private bool _roundStarted;

        [Inject]
        private void Construct(IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
            _rules = staticDataService.MatchRulesConfig;
        }

        public MatchPhase Phase => _phase.Value;
        public int WinnerObjectId => _winnerObjectId.Value;
        public string WinnerName => _winnerName.Value;

        public event Action<MatchPhase> OnPhaseChanged;
        
        public event Action<IReadOnlyList<PlayerMatchState>>
            OnRoundStartingServer;

        private void Awake()
        {
            _mode = new LastManStandingMode();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            _phase.Value = MatchPhase.WaitingForPlayers;
            _winnerObjectId.Value = -1;
            _roundStarted = false;
            _players.Clear();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            _phase.OnChange += HandlePhaseChanged;
        }

        public override void OnStopNetwork()
        {
            _phase.OnChange -= HandlePhaseChanged;

            base.OnStopNetwork();
        }

        private void HandlePhaseChanged(
            MatchPhase previous,
            MatchPhase next,
            bool asServer)
        {
            OnPhaseChanged?.Invoke(next);
        }

        /// <summary>
        /// Вызывается MatchVehicleSpawner-ом после каждого успешного spawn.
        /// </summary>
        [Server]
        public void SetExpectedPlayerCountServer(int count)
        {
            _expectedPlayerCount = Mathf.Max(1, count);
        }

        [Server]
        public void RegisterPlayerServer(PlayerMatchState player)
        {
            if (player == null || _players.Contains(player))
                return;

            int maxPlayers = _rules != null ? _rules.MaxPlayers : 6;
            if (_players.Count >= maxPlayers)
                return;

            _players.Add(player);

            player.IsAlive
                .Subscribe(isAlive =>
                    HandlePlayerAliveChangedServer(player, isAlive));

            TryAutoStartRoundServer();
        }

        [Server]
        public void UnregisterPlayerServer(PlayerMatchState player)
        {
            _players.Remove(player);
        }

        private void TryAutoStartRoundServer()
        {
            if (_roundStarted)
                return;

            if (_expectedPlayerCount <= 0)
                return;

            if (_players.Count < _expectedPlayerCount)
                return;

            StartRoundServer();
        }

        [Server]
        public void StartRoundServer()
        {
            if (_roundStarted)
                return;

            _roundStarted = true;
            _winnerObjectId.Value = -1;

            OnRoundStartingServer?.Invoke(_players);

            float countdownSeconds = _rules != null
                ? _rules.MatchStartCountdownSeconds
                : 3f;

            Invoke(nameof(FinishRoundStartServer), countdownSeconds);
        }

        [Server]
        private void FinishRoundStartServer()
        {
            foreach (PlayerMatchState player in _players)
            {
                player.ResetForMatchServer(
                    player.MaxHealth.CurrentValue > 0
                        ? player.MaxHealth.CurrentValue
                        : 100,
                    player.VehicleColor.CurrentValue);
            }

            _mode.OnMatchStarted();

            _phase.Value = MatchPhase.RoundInProgress;
        }

        private void HandlePlayerAliveChangedServer(
            PlayerMatchState player,
            bool isAlive)
        {
            if (!IsServerStarted)
                return;

            if (_phase.Value != MatchPhase.RoundInProgress)
                return;

            if (isAlive)
                return;

            _mode.OnPlayerEliminated(player);

            if (_mode.TryGetWinner(_players, out PlayerMatchState winner))
                EndRoundServer(winner);
        }

        [Server]
        private void EndRoundServer(PlayerMatchState winner)
        {
            _winnerObjectId.Value =
                winner != null ? winner.NetworkObject.ObjectId : -1;

            _winnerName.Value =
                winner != null ? winner.PlayerName.CurrentValue : "Draw";
            
            _phase.Value = MatchPhase.RoundEnded;

            _roundStarted = false;
        }

        /// <summary>
        /// Вызывается только host-клиентом из UI кнопки "Начать заново".
        /// Host физически исполняется как server, поэтому серверный код
        /// вызывается напрямую, без ServerRpc.
        /// </summary>
        [Server]
        public void RestartServer()
        {
            if (_phase.Value != MatchPhase.RoundEnded)
                return;

            StartRoundServer();
        }
    }
}