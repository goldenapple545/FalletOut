using CodeBase.Gameplay.Car.Input;
using UnityEngine;

namespace CodeBase.Gameplay.Car
{
    public sealed class VehiclePhysicsMotor : MonoBehaviour
    {
        [SerializeField] private PrometeoCarController prometeo;

        public void Simulate(VehicleInput input, float deltaTime)
        {
            if (prometeo == null)
                return;

            if (input.Throttle > 0f)
                prometeo.GoForward();
            else if (input.Throttle < 0f)
                prometeo.GoReverse();
            else
                prometeo.ThrottleOff();

            if (input.Steering < 0f)
                prometeo.TurnLeft();
            else if (input.Steering > 0f)
                prometeo.TurnRight();
            else
                prometeo.ResetSteeringAngle();

            if (input.Handbrake)
                prometeo.Handbrake();
            else
                prometeo.RecoverTraction();
        }
    }
}