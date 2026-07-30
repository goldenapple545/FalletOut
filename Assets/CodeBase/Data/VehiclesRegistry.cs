using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "VehiclesRegistry",
        menuName = "StaticData/VehiclesRegistry")]
    public sealed class VehiclesRegistry : ScriptableObject
    {
        [field: SerializeField] public List<VehicleConfig> Vehicles { get; private set; } = new();
    }
}
