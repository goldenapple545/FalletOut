using UnityEngine;

namespace CodeBase.CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "MatchRulesConfig",
        menuName = "StaticData/MatchRulesConfig")]
    public sealed class MatchRulesConfig : ScriptableObject
    {
        [field: SerializeField] public int MaxPlayers { get; private set; } = 6;
        [field: SerializeField] public float MatchStartCountdownSeconds { get; private set; } = 3f;
    }
}