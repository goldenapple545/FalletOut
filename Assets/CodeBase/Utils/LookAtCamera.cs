using UnityEngine;

namespace CodeBase.CodeBase.Utils
{
    public class LookAtCamera : MonoBehaviour
    {
        public bool yOnly = true;
        public bool inverse = true;

        private Transform _transform;

        void Awake()
        {
            _transform = transform;
        }

        void Update ()
        {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (yOnly) {
                pos = new Vector3 (MainCamera.Transform.position.x, _transform.position.y, MainCamera.Transform.position.z);
            } else {
                pos = MainCamera.Transform.position;
            }

            if (inverse) {
                rot = Quaternion.LookRotation (_transform.position - pos);
            } else {
                rot = Quaternion.LookRotation (pos - _transform.position);
            }

            _transform.rotation = rot;
        }
    }
}