using FishNet.Object;
using UnityEngine;

namespace CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleWheelVisuals : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private VehicleNetworkVisualState networkVisualState;
        [SerializeField] private WheelVisualBinding[] wheels;

        [Header("Remote visual tuning")]
        [SerializeField, Min(0.01f)] private float wheelRadius = 0.34f;

        private Vector3 _previousRootPosition;
        private bool _hasPreviousRootPosition;
        private float _spinAngle;

        private void Awake()
        {
            if (wheels == null)
                return;

            foreach (WheelVisualBinding wheel in wheels)
            {
                if (wheel?.Visual != null)
                    wheel.InitialLocalRotation = wheel.Visual.localRotation;
            }
        }
        
        private void LateUpdate()
        {
            if (IsServerStarted)
            {
                RefreshFromWheelColliders();
                return;
            }

            RefreshRemoteVisuals();
        }

        private void RefreshFromWheelColliders()
        {
            if (wheels == null)
                return;

            foreach (WheelVisualBinding wheel in wheels)
            {
                if (wheel == null || !wheel.HasPhysicsBinding)
                    continue;

                wheel.Collider.GetWorldPose(
                    out Vector3 position,
                    out Quaternion rotation);

                wheel.Visual.SetPositionAndRotation(
                    position,
                    rotation * Quaternion.Euler(wheel.RotationOffsetEuler));
            }
        }

        private void RefreshRemoteVisuals()
        {
            if (wheels == null)
                return;

            float deltaTime = Time.deltaTime;

            if (deltaTime <= Mathf.Epsilon)
                return;

            float forwardSpeed = GetRootForwardSpeed(deltaTime);
            float circumference = 2f * Mathf.PI * wheelRadius;
            float degreesPerSecond = forwardSpeed / circumference * 360f;

            _spinAngle = Mathf.Repeat(
                _spinAngle + degreesPerSecond * deltaTime,
                360f);

            float steeringAngle = networkVisualState != null
                ? networkVisualState.FrontSteeringAngle
                : 0f;

            foreach (WheelVisualBinding wheel in wheels)
            {
                if (wheel == null || !wheel.HasVisual)
                    continue;

                ApplyRemoteWheelRotation(wheel, steeringAngle);
            }
        }

        private float GetRootForwardSpeed(float deltaTime)
        {
            Vector3 currentPosition = transform.position;

            if (!_hasPreviousRootPosition)
            {
                _previousRootPosition = currentPosition;
                _hasPreviousRootPosition = true;
                return 0f;
            }

            Vector3 worldVelocity =
                (currentPosition - _previousRootPosition) / deltaTime;

            _previousRootPosition = currentPosition;

            return transform.InverseTransformDirection(worldVelocity).z;
        }

        private void ApplyRemoteWheelRotation(
            WheelVisualBinding wheel,
            float steeringAngle)
        {
            float steer = wheel.WheelAxle == WheelVisualBinding.Axle.Front
                ? steeringAngle
                : 0f;

            Quaternion baseOffset = Quaternion.Euler(
                wheel.RotationOffsetEuler);

            Quaternion steering = Quaternion.Euler(0f, steer, 0f);

            // Если ось вращения wheel mesh в asset не X,
            // измени этот rotation, а не сетевую логику.
            Quaternion spin = Quaternion.Euler(_spinAngle, 0f, 0f);

            wheel.Visual.localRotation =
                wheel.InitialLocalRotation *
                steering *
                spin *
                baseOffset;
        }
    }
}