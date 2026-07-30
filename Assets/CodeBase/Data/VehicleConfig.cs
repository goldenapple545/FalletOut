using CodeBase.Gameplay.Car;
using FishNet.Object;
using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "VehicleConfig",
        menuName = "StaticData/VehicleConfig")]
    public sealed class VehicleConfig : ScriptableObject
    {
        [field: Header("General")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite PreviewImage { get; private set; }

        [field: Header("Prefab")]
        [field: SerializeField] public NetworkObject Prefab { get; private set; }

        [field: Header("Stats")]
        [field: Range(0, 100)]
        [field: SerializeField] public float Speed { get; private set; } = 90f;
        [field: Range(0, 100)]
        [field: SerializeField] public float Drift { get; private set; } = 5f;
        [field: Range(0, 100)]
        [field: SerializeField] public float Durability { get; private set; } = 100f;
        [field: Range(0, 100)]
        [field: SerializeField] public float Damage { get; private set; } = 1f;
    }
}
