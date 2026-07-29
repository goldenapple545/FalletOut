using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.Gameplay.Car;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using R3;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public sealed class PlayerMatchState : NetworkBehaviour
    {
        [Header("Server defaults")] 
        [SerializeField] private VehicleStats config;
        [SerializeField] private Color defaultVehicleColor = Color.white;
        [SerializeField] private string defaultPlayerName = "Player";

        private MatchManager _matchManager;

        private readonly SyncVar<int> _health = new();
        private readonly SyncVar<int> _maxHealth = new();
        private readonly SyncVar<bool> _isAlive = new();
        private readonly SyncVar<Color> _vehicleColor = new();
        private readonly SyncVar<string> _playerName = new();

        private readonly ReactiveProperty<int> _healthReactive =
            new(0);

        private readonly ReactiveProperty<int> _maxHealthReactive =
            new(0);

        private readonly ReactiveProperty<bool> _isAliveReactive =
            new(false);

        private readonly ReactiveProperty<Color> _vehicleColorReactive =
            new(Color.white);

        private readonly ReactiveProperty<string> _playerNameReactive =
            new(string.Empty);

        public ReadOnlyReactiveProperty<int> Health =>
            _healthReactive;

        public ReadOnlyReactiveProperty<int> MaxHealth =>
            _maxHealthReactive;

        public ReadOnlyReactiveProperty<bool> IsAlive =>
            _isAliveReactive;

        public ReadOnlyReactiveProperty<Color> VehicleColor =>
            _vehicleColorReactive;

        public ReadOnlyReactiveProperty<string> PlayerName =>
            _playerNameReactive;

        public int ServerHealth => _health.Value;
        public bool ServerIsAlive => _isAlive.Value;

        // Значения, которые спавнер выставляет ДО ServerManager.Spawn.
        // Обычные поля, не SyncVar — объект ещё не в сети.
        private Color _pendingColor = Color.white;
        private string _pendingName;
        private bool _hasPendingIdentity;

        [Inject]
        private void Construct(MatchManager matchManager)
        {
            _matchManager = matchManager;
        }

        /// <summary>
        /// Server-only. Вызывается MatchVehicleSpawner-ом сразу после
        /// Instantiate, но ДО ServerManager.Spawn.
        /// </summary>
        public void SetPendingIdentity(Color color, string playerName)
        {
            _pendingColor = color;
            _pendingName = playerName;
            _hasPendingIdentity = true;
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            _health.OnChange += OnHealthChanged;
            _maxHealth.OnChange += OnMaxHealthChanged;
            _isAlive.OnChange += OnAliveChanged;
            _vehicleColor.OnChange += OnVehicleColorChanged;
            _playerName.OnChange += OnPlayerNameChanged;

            PublishCurrentValues();
        }

        public override void OnStopNetwork()
        {
            _health.OnChange -= OnHealthChanged;
            _maxHealth.OnChange -= OnMaxHealthChanged;
            _isAlive.OnChange -= OnAliveChanged;
            _vehicleColor.OnChange -= OnVehicleColorChanged;
            _playerName.OnChange -= OnPlayerNameChanged;

            _matchManager.UnregisterPlayerServer(this);

            base.OnStopNetwork();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            Color color = _hasPendingIdentity
                ? _pendingColor
                : defaultVehicleColor;

            string playerName = _hasPendingIdentity && !string.IsNullOrEmpty(_pendingName)
                ? _pendingName
                : defaultPlayerName;

            _playerName.Value = playerName;

            ResetForMatchServer(config.maxHealth, color);

            _matchManager.RegisterPlayerServer(this);
        }

        [Server]
        public void ResetForMatchServer(
            int maxHealth,
            Color vehicleColor)
        {
            int safeMaxHealth = Mathf.Max(1, maxHealth);

            _maxHealth.Value = safeMaxHealth;
            _health.Value = safeMaxHealth;
            _isAlive.Value = true;
            _vehicleColor.Value = vehicleColor;
        }

        [Server]
        public void ApplyDamageServer(int damage)
        {
            if (!_isAlive.Value)
                return;

            int safeDamage = Mathf.Max(0, damage);

            if (safeDamage == 0)
                return;

            _health.Value = Mathf.Max(
                0,
                _health.Value - safeDamage);

            if (_health.Value == 0)
                _isAlive.Value = false;
        }

        private void PublishCurrentValues()
        {
            _maxHealthReactive.Value = _maxHealth.Value;
            _healthReactive.Value = _health.Value;
            _isAliveReactive.Value = _isAlive.Value;
            _vehicleColorReactive.Value = _vehicleColor.Value;
            _playerNameReactive.Value = _playerName.Value;
        }

        private void OnHealthChanged(int previous, int next, bool asServer)
        {
            _healthReactive.Value = next;
        }

        private void OnMaxHealthChanged(int previous, int next, bool asServer)
        {
            _maxHealthReactive.Value = next;
        }

        private void OnAliveChanged(bool previous, bool next, bool asServer)
        {
            _isAliveReactive.Value = next;
        }

        private void OnVehicleColorChanged(Color previous, Color next, bool asServer)
        {
            _vehicleColorReactive.Value = next;
        }

        private void OnPlayerNameChanged(string previous, string next, bool asServer)
        {
            _playerNameReactive.Value = next;
        }
    }
}