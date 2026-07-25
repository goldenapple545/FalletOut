using UnityEngine;

namespace CodeBase.Gameplay.Car
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Temporary target for scene setup")]
        [SerializeField] private Transform carTransform;

        [Header("Follow")]
        [SerializeField, Range(1f, 10f)]
        private float followSpeed = 5f;

        [SerializeField, Range(1f, 10f)]
        private float lookSpeed = 5f;

        private Vector3 _offset;

        private void Awake()
        {
            if (carTransform == null)
            {
                Debug.LogError(
                    "[CameraFollow] Assign CameraAnchor as initial target.",
                    this);

                enabled = false;
                return;
            }

            // Камера стоит в сцене относительно CameraAnchor.
            _offset = transform.position - carTransform.position;
        }

        public void SetTarget(Transform target)
        {
            if (target == null)
            {
                Debug.LogError(
                    "[CameraFollow] Target is null.",
                    this);

                return;
            }

            // Offset НЕ пересчитываем.
            // Именно поэтому anchor можно заменить настоящим игроком.
            carTransform = target;
        }

        private void LateUpdate()
        {
            if (carTransform == null)
                return;

            Vector3 targetPosition = carTransform.position + _offset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime);

            Vector3 lookDirection = carTransform.position - transform.position;

            if (lookDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(
                lookDirection,
                Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                lookSpeed * Time.deltaTime);
        }
    }
}