using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "LevelConfig",
        menuName = "StaticData/LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite PreviewImage { get; private set; }
    }
}
