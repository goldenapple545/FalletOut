using System;

namespace CodeBase.Gameplay.Car.Input
{
    [Serializable]
    public struct VehicleInput
    {
        public float Throttle;
        public float Steering;
        public bool Handbrake;

        public VehicleInput(
            float throttle,
            float steering,
            bool handbrake)
        {
            Throttle = Math.Clamp(throttle, -1f, 1f);
            Steering = Math.Clamp(steering, -1f, 1f);
            Handbrake = handbrake;
        }

        public static VehicleInput Neutral => new(0f, 0f, false);
    }
}