namespace CodeBase.CodeBase.Gameplay.Network.Statistics
{
    public interface IPlayerNameResolver
    {
        void Register(int objectId, string playerName);
        void Unregister(int objectId);
        bool TryGetName(int objectId, out string playerName);
    }
}
