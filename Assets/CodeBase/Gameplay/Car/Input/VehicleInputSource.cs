using CodeBase.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace CodeBase.Gameplay.Car.Input
{
    public sealed class VehicleInputSource : MonoBehaviour
    {
        private float _touchThrottle;
        private float _touchSteering;
        private bool _touchHandbrake;
        private bool _touchBoost;

        private BuildConfig _buildConfig;

        [Inject]
        private void Construct(BuildConfig buildConfig)
        {
            _buildConfig = buildConfig;
        }

        public VehicleInput Read()
        {
            if (_buildConfig.IsAndroid)
            {
                return new VehicleInput(
                    _touchThrottle,
                    _touchSteering,
                    _touchHandbrake,
                    _touchBoost);
            }

            return ReadKeyboard();
        }

        public void SetTouchThrottle(float value) =>
            _touchThrottle = Mathf.Clamp(value, -1f, 1f);

        public void SetTouchSteering(float value) =>
            _touchSteering = Mathf.Clamp(value, -1f, 1f);

        public void SetTouchHandbrake(bool value) =>
            _touchHandbrake = value;

        public void SetTouchBoost(bool value) =>
            _touchBoost = value;

        public void ResetTouch()
        {
            _touchThrottle = 0f;
            _touchSteering = 0f;
            _touchHandbrake = false;
            _touchBoost = false;
        }

        private VehicleInput ReadKeyboard()
        {
            if (Keyboard.current == null)
                return VehicleInput.Neutral;

            float throttle =
                (Keyboard.current.wKey.isPressed ? 1f : 0f) -
                (Keyboard.current.sKey.isPressed ? 1f : 0f);

            float steering =
                (Keyboard.current.dKey.isPressed ? 1f : 0f) -
                (Keyboard.current.aKey.isPressed ? 1f : 0f);

            bool handbrake = Keyboard.current.spaceKey.isPressed;
            bool boost = Keyboard.current.eKey.isPressed;

            return new VehicleInput(throttle, steering, handbrake, boost);
        }
    }
}