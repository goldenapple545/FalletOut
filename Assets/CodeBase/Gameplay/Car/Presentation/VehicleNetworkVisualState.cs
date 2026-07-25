using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

namespace CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleNetworkVisualState : NetworkBehaviour
    {
        private readonly SyncVar<float> _frontSteeringAngle =
            new SyncVar<float>(new SyncTypeSettings(0.05f, Channel.Unreliable));

        public float FrontSteeringAngle => _frontSteeringAngle.Value;

        [Server]
        public void SetFrontSteeringAngle(float value)
        {
            _frontSteeringAngle.Value = value;
        }
    }
}