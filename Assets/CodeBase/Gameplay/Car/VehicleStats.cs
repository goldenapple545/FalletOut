using UnityEngine;

namespace CodeBase.Gameplay.Car
{
    public sealed class VehicleStats : MonoBehaviour
    {
        [Header("Speed")]
        [Range(20, 1000)]
        public int maxSpeed = 90;
        [Range(10, 400)]
        public int maxReverseSpeed = 45;

        [Header("Acceleration")]
        [Range(1, 10000)]
        public int accelerationMultiplier = 2000;

        [Header("Steering")]
        [Range(10, 70)]
        public int maxSteeringAngle = 27;
        [Range(0.1f, 1f)]
        public float steeringSpeed = 0.5f;

        [Header("Braking")]
        [Range(100, 1000)]
        public int brakeForce = 350;
        [Range(1, 50)]
        public int decelerationMultiplier = 2;

        [Header("Drift")]
        [Range(1, 50)]
        public int handbrakeDriftMultiplier = 5;
        
        [Header("Boost")]
        public float boostForce = 500f;
        public float boostCooldown = 2f;

        [Header("Physics")]
        public Vector3 bodyMassCenter = new Vector3(0f, -0.5f, 0f);

        [Header("Health")]
        public int maxHealth = 100;

        [Header("Collision Damage")]
        [SerializeField, Min(0f)] public float minimumImpactSpeed = 4f;

        [SerializeField, Min(0f)] public float damagePerSpeed = 6f;
        [SerializeField, Min(0f)] public float pairCooldownSeconds = 0.5f;
    }
}
