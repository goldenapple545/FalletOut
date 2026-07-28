using System;
using CodeBase.Infrastructure.Services.Music;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Network.Music
{
    public class MusicLobbyController: MonoBehaviour
    {
        private IMusicService _musicService;

        [Inject]
        public void Construct(IMusicService musicService)
        {
            _musicService = musicService;
        }

        private void Start()
        {
            PlayLobbyTrack();
        }

        private void OnDestroy()
        {
            StopMusic();
        }

        private void PlayLobbyTrack()
        {
            _musicService?.PlayLobbyTrack();
        }

        private void StopMusic()
        {
            _musicService?.Stop();
        }
    }
}