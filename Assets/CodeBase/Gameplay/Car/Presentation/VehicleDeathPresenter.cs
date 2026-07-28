using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.Gameplay.Car;
using R3;
using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleDeathPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerMatchState playerState;
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private GameObject[] bodyVisuals;
        [SerializeField] private PrometeoCarController carController;
        [SerializeField] private Rigidbody vehicleRigidbody;

        private void Awake()
        {
            if (playerState == null)
                playerState = GetComponentInParent<PlayerMatchState>();

            if (vehicleRigidbody == null)
                vehicleRigidbody = GetComponentInParent<Rigidbody>();
        }

        private void Start()
        {
            if (playerState == null)
            {
                Debug.LogError(
                    $"{nameof(VehicleDeathPresenter)}: " +
                    $"{nameof(PlayerMatchState)} не найден.",
                    this);

                return;
            }

            playerState.IsAlive
                .Subscribe(ApplyAliveState)
                .AddTo(this);
        }

        private void ApplyAliveState(bool isAlive)
        {
            SetInputEnabled(isAlive);
            SetVisualsAlive(isAlive);

            if (!isAlive)
                explosionEffect?.Play();
            else
                explosionEffect?.Stop();
        }

        private void SetInputEnabled(bool enabled)
        {
            carController.SetReactOnInput(enabled);

            // Машина остаётся физическим препятствием (её можно толкать),
            // но больше не разгоняется сама.
            if (vehicleRigidbody != null && !enabled)
                vehicleRigidbody.linearVelocity = Vector3.zero;
        }

        private void SetVisualsAlive(bool isAlive)
        {
            foreach (GameObject visual in bodyVisuals)
            {
                if (visual != null)
                    visual.SetActive(isAlive);
            }
        }
    }
}