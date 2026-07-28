namespace CodeBase.CodeBase.Gameplay.Network.Match
{
    public enum MatchPhase : byte
    {
        WaitingForPlayers = 0,
        Countdown = 1,
        Playing = 2,
        Finished = 3
    }
}