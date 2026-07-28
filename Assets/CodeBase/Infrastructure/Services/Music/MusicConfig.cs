using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Music
{
    [CreateAssetMenu(
        fileName = "MusicConfig",
        menuName = "StaticData/MusicConfig")]
    public sealed class MusicConfig : ScriptableObject
    {
        [field: SerializeField] public AudioClip LobbyTrack { get; private set; }
        [field: SerializeField] public List<AudioClip> BattleTracks { get; private set; } = new();
    }
}
