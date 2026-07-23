using System.Net;

namespace CodeBase.Gameplay.Network
{
    public readonly struct ServerInfo
    {
        public readonly IPEndPoint EndPoint;
        public readonly string     Name;
        public readonly int        CurrentPlayers;
        public readonly int        MaxPlayers;

        public ServerInfo(IPEndPoint endPoint, string name, int currentPlayers, int maxPlayers)
        {
            EndPoint       = endPoint;
            Name           = name;
            CurrentPlayers = currentPlayers;
            MaxPlayers     = maxPlayers;
        }
        
        public string DisplayLabel => $"{Name}  —  {CurrentPlayers}/{MaxPlayers}";
    }
}