using CodeBase.CodeBase.Gameplay.Network.Match;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.CodeBase.Gameplay.Car.Presentation
{
    public sealed class HealthHudPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerMatchState playerState;
        [SerializeField] private Image healthBar;
        [SerializeField] private TMP_Text healthText;

        private DisposableBag _disposables;

        public void Start()
        {
            _disposables.Dispose();
            _disposables = new DisposableBag();

            if (playerState == null)
            {
                Debug.LogError("PlayerMatchState is null.", this);
                return;
            }

            playerState.Health
                .CombineLatest(
                    playerState.MaxHealth,
                    (health, maxHealth) =>
                        new HealthViewData(health, maxHealth))
                .Subscribe(ApplyHealth)
                .AddTo(ref _disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void ApplyHealth(HealthViewData data)
        {
            int safeMaxHealth = Mathf.Max(1, data.MaxHealth);
            int clampedHealth = Mathf.Clamp(
                data.Health,
                0,
                safeMaxHealth);

            if (healthBar != null)
            {
                healthBar.fillAmount =
                    (float)clampedHealth / safeMaxHealth;
            }

            if (healthText != null)
            {
                healthText.text =
                    $"{clampedHealth} / {safeMaxHealth}";
            }
        }

        private readonly struct HealthViewData
        {
            public readonly int Health;
            public readonly int MaxHealth;

            public HealthViewData(int health, int maxHealth)
            {
                Health = health;
                MaxHealth = maxHealth;
            }
        }
    }
}