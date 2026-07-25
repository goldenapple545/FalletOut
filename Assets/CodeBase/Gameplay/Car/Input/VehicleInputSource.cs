using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Gameplay.Car.Input
{
    public sealed class VehicleInputSource : MonoBehaviour
    {
        private float _touchThrottle;
        private float _touchSteering;
        private bool _touchHandbrake;

        public VehicleInput ReadInput()
        {
            float throttle = _touchThrottle;
            float steering = _touchSteering;
            bool handbrake = _touchHandbrake;

            if (Keyboard.current != null)
            {
                throttle += Keyboard.current.wKey.isPressed ? 1f : 0f;
                throttle -= Keyboard.current.sKey.isPressed ? 1f : 0f;

                steering += Keyboard.current.dKey.isPressed ? 1f : 0f;
                steering -= Keyboard.current.aKey.isPressed ? 1f : 0f;

                handbrake |= Keyboard.current.spaceKey.isPressed;
            }

            return new VehicleInput(
                Mathf.Clamp(throttle, -1f, 1f),
                Mathf.Clamp(steering, -1f, 1f),
                handbrake);
        }

        public void SetThrottle(float value) =>
            _touchThrottle = Mathf.Clamp(value, -1f, 1f);

        public void SetSteering(float value) =>
            _touchSteering = Mathf.Clamp(value, -1f, 1f);

        public void SetHandbrake(bool value) =>
            _touchHandbrake = value;
    }
}