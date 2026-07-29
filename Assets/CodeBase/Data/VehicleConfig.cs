using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "VehicleConfig",
        menuName = "StaticData/VehicleConfig")]
    public sealed class VehicleConfig : ScriptableObject
    {
        [Header("Speed")]
        [field: SerializeField] public float MaxForwardSpeed { get; private set; } = 90f;
        [field: SerializeField] public float MaxReverseSpeed { get; private set; } = 45f;

        [Header("Acceleration")]
        [field: SerializeField] public float AccelerationMultiplier { get; private set; } = 2000f;
        [field: SerializeField] public float ForwardAcceleration { get; private set; } = 18f;
        [field: SerializeField] public float ReverseAcceleration { get; private set; } = 10f;

        [Header("Steering")]
        [field: SerializeField] public float MaxSteeringAngle { get; private set; } = 27f;
        [field: SerializeField] public float SteeringSpeed { get; private set; } = 0.5f;

        [Header("Braking")]
        [field: SerializeField] public float BrakeForce { get; private set; } = 350f;
        [field: SerializeField] public float DecelerationMultiplier { get; private set; } = 2f;

        [Header("Drift")]
        [field: SerializeField] public float HandbrakeDriftMultiplier { get; private set; } = 5f;

        [Header("Physics")]
        [field: SerializeField] public float Mass { get; private set; } = 1200f;
        [field: SerializeField] public Vector3 BodyMassCenter { get; private set; } = new Vector3(0f, -0.5f, 0f);

        [Header("Health")]
        [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    }
}