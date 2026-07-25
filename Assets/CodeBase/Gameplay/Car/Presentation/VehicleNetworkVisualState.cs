using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

namespace CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleNetworkVisualState : NetworkBehaviour
    {
        private readonly SyncVar<float> _frontSteeringAngle =
            new(new SyncTypeSettings(0.05f, Channel.Unreliable));

        private readonly SyncVar<bool> _isDrifting =
            new(new SyncTypeSettings(0.05f, Channel.Unreliable));

        private readonly SyncVar<bool> _areSkidMarksActive =
            new(new SyncTypeSettings(0.05f, Channel.Unreliable));

        public float FrontSteeringAngle => _frontSteeringAngle.Value;
        public bool IsDrifting => _isDrifting.Value;
        public bool AreSkidMarksActive => _areSkidMarksActive.Value;

        [Server]
        public void SetFrontSteeringAngle(float value)
        {
            _frontSteeringAngle.Value = value;
        }

        [Server]
        public void SetDrifting(bool value)
        {
            _isDrifting.Value = value;
        }

        [Server]
        public void SetSkidMarksActive(bool value)
        {
            _areSkidMarksActive.Value = value;
        }
    }
}