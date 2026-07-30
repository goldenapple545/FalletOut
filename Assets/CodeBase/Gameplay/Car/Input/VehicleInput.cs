using System;

namespace CodeBase.Gameplay.Car.Input
{
    [Serializable]
    public struct VehicleInput
    {
        public float Throttle;
        public float Steering;
        public bool Handbrake;
        public bool Boost;

        public VehicleInput(
            float throttle,
            float steering,
            bool handbrake,
            bool boost = false)
        {
            Throttle = Math.Clamp(throttle, -1f, 1f);
            Steering = Math.Clamp(steering, -1f, 1f);
            Handbrake = handbrake;
            Boost = boost;
        }

        public static VehicleInput Neutral => new(0f, 0f, false);
    }
}