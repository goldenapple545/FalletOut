using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "LevelsRegistry",
        menuName = "StaticData/LevelsRegistry")]
    public sealed class LevelsRegistry : ScriptableObject
    {
        [field: SerializeField] public List<LevelConfig> Levels { get; private set; } = new();
    }
}
