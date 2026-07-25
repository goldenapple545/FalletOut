using System;

namespace CodeBase.Gameplay.Car.Input
{
    [Serializable]
    public struct VehicleInput
    {
        public float Throttle;
        public float Steering;
        public bool Handbrake;

        public VehicleInput(float throttle, float steering, bool handbrake)
        {
            Throttle = throttle;
            Steering = steering;
            Handbrake = handbrake;
        }

        public static VehicleInput Neutral =>
            new VehicleInput(0f, 0f, false);
    }
}