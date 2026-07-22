using UnityEngine;

namespace CodeBase.Infrastructure.BootstrapSteps.StaticData.Configs
{
    [CreateAssetMenu(
        fileName = "VehicleConfig",
        menuName = "StaticData/VehicleConfig")]
    public sealed class VehicleConfig : ScriptableObject
    {
        [field: SerializeField] public float ForwardAcceleration { get; private set; } = 18f;
        [field: SerializeField] public float ReverseAcceleration { get; private set; } = 10f;
        [field: SerializeField] public float MaxForwardSpeed { get; private set; } = 20f;
        [field: SerializeField] public float MaxReverseSpeed { get; private set; } = 8f;
        [field: SerializeField] public float SteeringSpeed { get; private set; } = 120f;
        [field: SerializeField] public float Mass { get; private set; } = 1200f;
        [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    }
}