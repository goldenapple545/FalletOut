using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car
{
    public sealed class ThirdPersonCameraFollow : MonoBehaviour, ICameraFollow
    {
        [Header("Temporary target for scene setup")]
        [SerializeField] private Transform carTransform;

        [Header("Follow")]
        [SerializeField, Range(1f, 15f)]
        private float positionSpeed = 8f;

        [SerializeField, Range(1f, 15f)]
        private float rotationSpeed = 8f;

        private Vector3 _localOffset;
        private Quaternion _rotationOffset; // разница между поворотом камеры и машины

        private void Awake()
        {
            if (carTransform == null)
            {
                Debug.LogError(
                    "[ThirdPersonCameraFollow] Assign CameraAnchor as initial target.",
                    this);

                enabled = false;
                return;
            }

            _localOffset = carTransform.InverseTransformPoint(transform.position);

            // Запоминаем, насколько камера повернута ОТНОСИТЕЛЬНО машины изначально
            // (например, наклон вниз на 15 градусов).
            _rotationOffset = Quaternion.Inverse(carTransform.rotation) * transform.rotation;
        }

        public void SetTarget(Transform target)
        {
            if (target == null)
            {
                Debug.LogError(
                    "[ThirdPersonCameraFollow] Target is null.",
                    this);

                return;
            }

            carTransform = target;
        }

        private void LateUpdate()
        {
            if (carTransform == null)
                return;

            Vector3 targetPosition = carTransform.TransformPoint(_localOffset);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                positionSpeed * Time.deltaTime);

            // Целевой поворот = поворот машины + изначальный оффсет камеры.
            Quaternion targetRotation = carTransform.rotation * _rotationOffset;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }
}