using FishNet.Object.Prediction;

namespace CodeBase.Gameplay.Car.Input.Prediction
{
    public struct VehicleReplicateData : IReplicateData
    {
        public float Throttle;
        public float Steering;
        public bool Handbrake;
        public bool Boost;

        private uint _tick;

        public VehicleReplicateData(
            float throttle,
            float steering,
            bool handbrake,
            bool boost)
        {
            Throttle = throttle;
            Steering = steering;
            Handbrake = handbrake;
            Boost = boost;
            _tick = 0;
        }

        public uint GetTick() => _tick;

        public void SetTick(uint value) => _tick = value;

        public void Dispose()
        {
        }
    }
}