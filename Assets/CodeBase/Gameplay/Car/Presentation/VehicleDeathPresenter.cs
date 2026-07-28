using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.Gameplay.Car;
using R3;
using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleDeathPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerMatchState playerState;
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private ParticleSystem[] fireEffects;
        [SerializeField] private GameObject[] aliveParts;
        [SerializeField] private GameObject[] damageParts;
        [SerializeField] private PrometeoCarController carController;
        [SerializeField] private Rigidbody vehicleRigidbody;

        [Header("Audio")] 
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip clip;
        
        private GameObject _explosionInstance;

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

            SetEffectsActive(isAlive);
            
            if (!isAlive)
            {
                source.clip = clip;
                source.loop = false;
                source.Play();
            }
            else
            {
                source.Stop();
            }
        }

        private void SetEffectsActive(bool isAlive)
        {
            if (_explosionInstance != null)
                Destroy(_explosionInstance);
            
            if (!isAlive)
            {
                _explosionInstance = Instantiate(explosionEffect, transform);
                
                Destroy(explosionEffect, 5f);
            }
            
            foreach (var fire in fireEffects)
            {
                fire.gameObject.SetActive(!isAlive);
            }
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
            foreach (GameObject visual in aliveParts)
            {
                if (visual != null)
                    visual.SetActive(isAlive);
            }
            
            foreach (GameObject visual in damageParts)
            {
                if (visual != null)
                    visual.SetActive(!isAlive);
            }
        }
    }
}