using System;
using UnityEngine;

namespace CodeBase.Gameplay.Car.Presentation.Wheels
{
    [Serializable]
    public sealed class WheelVisualBinding
    {
        public enum Axle
        {
            Front = 0,
            Rear = 1
        }

        [field: SerializeField]
        public WheelCollider Collider { get; private set; }

        [field: SerializeField]
        public Transform Visual { get; private set; }

        [field: SerializeField]
        public Axle WheelAxle { get; private set; }

        [field: SerializeField]
        public Vector3 RotationOffsetEuler { get; private set; }
        
        [NonSerialized]
        public Quaternion InitialLocalRotation;

        public bool HasPhysicsBinding => Collider != null && Visual != null;
        public bool HasVisual => Visual != null;
    }
}