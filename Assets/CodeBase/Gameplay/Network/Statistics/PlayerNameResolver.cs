using System.Collections.Generic;

namespace CodeBase.CodeBase.Gameplay.Network.Statistics
{
    public sealed class PlayerNameResolver : IPlayerNameResolver
    {
        private readonly Dictionary<int, string> _names = new();

        public void Register(int objectId, string playerName)
        {
            _names[objectId] = playerName;
        }

        public void Unregister(int objectId)
        {
            _names.Remove(objectId);
        }

        public bool TryGetName(int objectId, out string playerName)
        {
            return _names.TryGetValue(objectId, out playerName);
        }
    }
}
