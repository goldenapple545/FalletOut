using FishNet.Object;
using FishNet.Object.Synchronizing;
using R3;
using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public sealed class PlayerMatchState : NetworkBehaviour
    {
        [Header("Server defaults")]
        [SerializeField, Min(1)] private int defaultMaxHealth = 100;
        [SerializeField] private Color defaultVehicleColor = Color.red;

        private readonly SyncVar<int> _health = new();
        private readonly SyncVar<int> _maxHealth = new();
        private readonly SyncVar<bool> _isAlive = new();
        private readonly SyncVar<Color> _vehicleColor = new();

        private readonly ReactiveProperty<int> _healthReactive =
            new(0);

        private readonly ReactiveProperty<int> _maxHealthReactive =
            new(0);

        private readonly ReactiveProperty<bool> _isAliveReactive =
            new(false);

        private readonly ReactiveProperty<Color> _vehicleColorReactive =
            new(Color.white);

        public ReadOnlyReactiveProperty<int> Health =>
            _healthReactive;

        public ReadOnlyReactiveProperty<int> MaxHealth =>
            _maxHealthReactive;

        public ReadOnlyReactiveProperty<bool> IsAlive =>
            _isAliveReactive;

        public ReadOnlyReactiveProperty<Color> VehicleColor =>
            _vehicleColorReactive;

        public int ServerHealth => _health.Value;
        public bool ServerIsAlive => _isAlive.Value;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            _health.OnChange += OnHealthChanged;
            _maxHealth.OnChange += OnMaxHealthChanged;
            _isAlive.OnChange += OnAliveChanged;
            _vehicleColor.OnChange += OnVehicleColorChanged;

            // Важно для host и поздних observer:
            // переносим уже пришедшие SyncVar значения в R3 state.
            PublishCurrentValues();
        }

        public override void OnStopNetwork()
        {
            _health.OnChange -= OnHealthChanged;
            _maxHealth.OnChange -= OnMaxHealthChanged;
            _isAlive.OnChange -= OnAliveChanged;
            _vehicleColor.OnChange -= OnVehicleColorChanged;

            base.OnStopNetwork();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            ResetForMatchServer(
                defaultMaxHealth,
                defaultVehicleColor);
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

        [Server]
        public void SetVehicleColorServer(Color vehicleColor)
        {
            _vehicleColor.Value = vehicleColor;
        }

        private void PublishCurrentValues()
        {
            _maxHealthReactive.Value = _maxHealth.Value;
            _healthReactive.Value = _health.Value;
            _isAliveReactive.Value = _isAlive.Value;
            _vehicleColorReactive.Value = _vehicleColor.Value;
        }

        private void OnHealthChanged(
            int previous,
            int next,
            bool asServer)
        {
            _healthReactive.Value = next;
        }

        private void OnMaxHealthChanged(
            int previous,
            int next,
            bool asServer)
        {
            _maxHealthReactive.Value = next;
        }

        private void OnAliveChanged(
            bool previous,
            bool next,
            bool asServer)
        {
            _isAliveReactive.Value = next;
        }

        private void OnVehicleColorChanged(
            Color previous,
            Color next,
            bool asServer)
        {
            _vehicleColorReactive.Value = next;
        }
    }
}