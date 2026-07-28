using System;

namespace CodeBase.Infrastructure.Services.Music
{
    public interface IMusicService
    {
        event Action TrackChanged;

        int CurrentBattleTrackIndex { get; }
        int BattleTrackCount { get; }

        void PlayLobbyTrack();
        void PlayBattleTrack(int trackIndex);
        void Stop();
        void SetVolume(float volume);
    }
}
