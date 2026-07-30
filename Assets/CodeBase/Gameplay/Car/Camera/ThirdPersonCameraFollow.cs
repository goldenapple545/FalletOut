using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car.Camera
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

        // Оффсет в ЛОКАЛЬНОМ пространстве машины, а не в мировом.
        private Vector3 _localOffset;

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

            // Переводим текущую мировую позицию камеры
            // в локальные координаты машины — это и есть "посадка сзади/спереди".
            _localOffset = carTransform.InverseTransformPoint(transform.position);
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

            // Локальный оффсет НЕ пересчитываем — сохраняем ту же посадку камеры
            // относительно нового таргета (например, при пересадке на другую машину).
            carTransform = target;
        }

        private void LateUpdate()
        {
            if (carTransform == null)
                return;

            // Локальный оффсет переводим обратно в мир —
            // при повороте машины эта точка поворачивается вместе с ней.
            Vector3 targetPosition = carTransform.TransformPoint(_localOffset);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                positionSpeed * Time.deltaTime);

            // Вращение камеры плавно подстраивается под вращение машины,
            // а не под направление взгляда на неё — так камера "едет" вместе с кузовом.
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                carTransform.rotation,
                rotationSpeed * Time.deltaTime);
        }
    }
}