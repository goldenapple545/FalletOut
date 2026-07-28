using System.Collections.Generic;
using System.Linq;

namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public sealed class LastManStandingMode : IMatchMode
    {
        public void OnMatchStarted()
        {
        }

        public void OnPlayerEliminated(PlayerMatchState player)
        {
        }

        public bool TryGetWinner(
            IReadOnlyCollection<PlayerMatchState> players,
            out PlayerMatchState winner)
        {
            List<PlayerMatchState> alivePlayers = players
                .Where(player => player.IsAlive.CurrentValue)
                .ToList();

            winner = alivePlayers.Count == 1
                ? alivePlayers[0]
                : null;

            return winner != null;
        }
    }
}