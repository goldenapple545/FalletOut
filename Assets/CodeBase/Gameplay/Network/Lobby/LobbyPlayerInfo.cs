namespace CodeBase.Gameplay.Network.Lobby
{
    public readonly struct LobbyPlayerInfo
    {
        public readonly int ClientId;
        public readonly string DisplayName;
        public readonly bool IsHost;
        public readonly bool IsReady;

        public LobbyPlayerInfo(
            int clientId,
            string displayName,
            bool isHost,
            bool isReady)
        {
            ClientId = clientId;
            DisplayName = displayName;
            IsHost = isHost;
            IsReady = isReady;
        }
    }
}