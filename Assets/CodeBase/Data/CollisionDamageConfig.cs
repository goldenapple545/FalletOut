using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "CollisionDamageConfig",
        menuName = "StaticData/CollisionDamageConfig")]
    public sealed class CollisionDamageConfig : ScriptableObject
    {
        [field: SerializeField] public float MinRelativeSpeedForDamage { get; private set; } = 4f;
        [field: SerializeField] public float BaseDamage { get; private set; } = 20f;
        [field: SerializeField] public float SideMultiplier { get; private set; } = 1f;
        [field: SerializeField] public float RearMultiplier { get; private set; } = 1.5f;
        [field: SerializeField] public float RepeatHitCooldown { get; private set; } = 0.75f;
    }
}