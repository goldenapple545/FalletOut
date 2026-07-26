using FishNet.Object.Prediction;

namespace CodeBase.Gameplay.Car.Input.Prediction
{
    public struct VehicleReconcileData : IReconcileData
    {
        public PredictionRigidbody Rigidbody;
        
        public float SteeringAxis;
        public float ThrottleAxis;
        public float DriftingAxis;
        public bool IsDrifting;
        public bool IsTractionLocked;

        private uint _tick;

        public VehicleReconcileData(
            PredictionRigidbody rigidbody,
            float steeringAxis,
            float throttleAxis,
            float driftingAxis,
            bool isDrifting,
            bool isTractionLocked)
        {
            Rigidbody = rigidbody;
            SteeringAxis = steeringAxis;
            ThrottleAxis = throttleAxis;
            DriftingAxis = driftingAxis;
            IsDrifting = isDrifting;
            IsTractionLocked = isTractionLocked;
            _tick = 0;
        }

        public uint GetTick() => _tick;

        public void SetTick(uint value)
        {
            _tick = value;
        }

        public void Dispose()
        {
        }
    }
}