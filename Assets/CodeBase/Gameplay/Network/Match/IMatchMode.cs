using System.Collections.Generic;

namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public interface IMatchMode
    {
        void OnMatchStarted();
        void OnPlayerEliminated(PlayerMatchState player);
        bool TryGetWinner(
            IReadOnlyCollection<PlayerMatchState> players,
            out PlayerMatchState winner);
    }
}