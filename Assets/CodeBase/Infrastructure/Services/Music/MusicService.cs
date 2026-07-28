using System;
using CodeBase.Data;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Services.Music
{
    public sealed class MusicService : IMusicService, IInitializable, IDisposable
    {
        private readonly MusicConfig _config;
        private readonly AudioSource _audioSource;

        private int _currentBattleTrackIndex = -1;

        public event Action TrackChanged;
        public int CurrentBattleTrackIndex => _currentBattleTrackIndex;
        public int BattleTrackCount => _config.BattleTracks?.Count ?? 0;

        public MusicService(MusicConfig config, GameObject musicSourcePrefab)
        {
            _config = config;

            var go = UnityEngine.Object.Instantiate(musicSourcePrefab);
            go.name = "[MusicService]";
            go.hideFlags = HideFlags.HideInHierarchy;
            UnityEngine.Object.DontDestroyOnLoad(go);

            _audioSource = go.GetComponent<AudioSource>();
            if (_audioSource == null)
                throw new Exception("[MusicService] AudioSource not found on the provided prefab.");
        }

        public void Initialize()
        {
        }

        public void PlayLobbyTrack()
        {
            if (_config.LobbyTrack == null)
            {
                Debug.LogWarning("[MusicService] Lobby track is not assigned.");
                return;
            }

            _audioSource.clip = _config.LobbyTrack;
            _audioSource.Play();
            TrackChanged?.Invoke();
        }

        public void PlayBattleTrack(int trackIndex)
        {
            if (_config.BattleTracks == null || trackIndex < 0 || trackIndex >= _config.BattleTracks.Count)
            {
                Debug.LogWarning($"[MusicService] Battle track index {trackIndex} is out of range.");
                return;
            }

            _currentBattleTrackIndex = trackIndex;
            _audioSource.clip = _config.BattleTracks[trackIndex];
            _audioSource.Play();
            TrackChanged?.Invoke();
        }

        public void Stop()
        {
            _audioSource.Stop();
            _currentBattleTrackIndex = -1;
            TrackChanged?.Invoke();
        }

        public void SetVolume(float volume)
        {
            _audioSource.volume = Mathf.Clamp01(volume);
        }

        public void Dispose()
        {
            if (_audioSource != null)
                UnityEngine.Object.Destroy(_audioSource.gameObject);
        }
    }
}
