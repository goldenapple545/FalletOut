using UnityEngine;

namespace CodeBase.Gameplay.Car.Presentation.Effects
{
    public sealed class VehicleDriftEffectsPresenter : MonoBehaviour
    {
        [SerializeField] private VehicleNetworkVisualState visualState;

        [Header("Smoke")]
        [SerializeField] private ParticleSystem rearLeftSmoke;
        [SerializeField] private ParticleSystem rearRightSmoke;

        [Header("Skid trails")]
        [SerializeField] private TrailRenderer rearLeftSkid;
        [SerializeField] private TrailRenderer rearRightSkid;

        private bool _wasDrifting;
        private bool _wereSkidMarksActive;

        private void Awake()
        {
            ApplySmoke(enabled: false, clear: true);
            ApplySkids(enabled: false);
        }

        private void LateUpdate()
        {
            if (visualState == null)
                return;

            bool isDrifting = visualState.IsDrifting;

            if (_wasDrifting != isDrifting)
            {
                _wasDrifting = isDrifting;
                ApplySmoke(isDrifting, clear: false);
            }

            bool areSkidMarksActive = visualState.AreSkidMarksActive;

            if (_wereSkidMarksActive != areSkidMarksActive)
            {
                _wereSkidMarksActive = areSkidMarksActive;
                ApplySkids(areSkidMarksActive);
            }
        }

        private void ApplySmoke(bool enabled, bool clear)
        {
            SetSmoke(rearLeftSmoke, enabled, clear);
            SetSmoke(rearRightSmoke, enabled, clear);
        }

        private void ApplySkids(bool enabled)
        {
            if (rearLeftSkid != null)
                rearLeftSkid.emitting = enabled;

            if (rearRightSkid != null)
                rearRightSkid.emitting = enabled;
        }

        private static void SetSmoke(
            ParticleSystem particleSystem,
            bool enabled,
            bool clear)
        {
            if (particleSystem == null)
                return;

            if (enabled)
            {
                if (!particleSystem.isPlaying)
                    particleSystem.Play(true);

                return;
            }

            ParticleSystemStopBehavior stopBehavior = clear
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting;

            particleSystem.Stop(
                withChildren: true,
                stopBehavior);
        }
    }
}