using CodeBase.Gameplay.Car.Presentation.Effects;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleVisualStateSynchronizer : NetworkBehaviour
    {
        [SerializeField] private PrometeoCarController carController;
        [SerializeField] private DriftEffectsStateProvider driftEffectsStateProvider;
        [SerializeField] private VehicleNetworkVisualState visualState;

        private void FixedUpdate()
        {
            if (!IsServerStarted)
                return;

            if (carController == null ||
                driftEffectsStateProvider == null ||
                visualState == null)
            {
                return;
            }

            driftEffectsStateProvider.Refresh();

            visualState.SetFrontSteeringAngle(
                carController.frontLeftCollider.steerAngle);

            visualState.SetDrifting(
                driftEffectsStateProvider.IsDrifting);

            visualState.SetSkidMarksActive(
                driftEffectsStateProvider.AreSkidMarksActive);
        }
    }
}