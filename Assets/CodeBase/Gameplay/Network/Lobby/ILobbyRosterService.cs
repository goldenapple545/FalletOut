using System;
using System.Collections.Generic;

namespace CodeBase.Gameplay.Network.Lobby
{
    public interface ILobbyRosterService
    {
        IReadOnlyList<LobbyPlayerInfo> Players { get; }
        bool CanHostStartMatch { get; }

        event Action<IReadOnlyList<LobbyPlayerInfo>> PlayersChanged;
        event Action<bool> StartAvailabilityChanged;

        void SetLocalReady(bool ready);
        void StartMatch();
    }
}