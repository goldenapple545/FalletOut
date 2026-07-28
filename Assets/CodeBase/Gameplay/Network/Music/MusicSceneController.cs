using CodeBase.Infrastructure.Services.Music;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Network.Music
{
    /// <summary>
    /// Размещается на сцене (например, на отдельном GameObject или MatchManager).
    /// Сервер выбирает случайный трек и синхронизирует его всем клиентам через SyncVar.
    /// Клиенты получают индекс и просят MusicService воспроизвести этот же трек.
    /// </summary>
    public sealed class MusicSceneController : NetworkBehaviour
    {
        private readonly SyncVar<int> _syncedBattleTrackIndex = new SyncVar<int>(-1);

        private IMusicService _musicService;
        private int _lastReceivedIndex = -1;

        [Inject]
        public void Construct(IMusicService musicService)
        {
            _musicService = musicService;
        }

        private void Update()
        {
            if (_musicService == null)
                return;

            // На сервере: если трек ещё не выбран и есть треки — выбираем
            if (IsServer && _syncedBattleTrackIndex.Value == -1 && _musicService.BattleTrackCount > 0)
            {
                int randomIndex = Random.Range(0, _musicService.BattleTrackCount);
                _syncedBattleTrackIndex.Value = randomIndex;
                _musicService.PlayBattleTrack(randomIndex);

                Debug.Log($"[MusicSceneController] Server selected battle track #{randomIndex}");
            }

            // На клиенте: если сервер прислал новый индекс — воспроизводим
            if (IsClient && _syncedBattleTrackIndex.Value >= 0 && _syncedBattleTrackIndex.Value != _lastReceivedIndex)
            {
                _lastReceivedIndex = _syncedBattleTrackIndex.Value;
                _musicService.PlayBattleTrack(_syncedBattleTrackIndex.Value);

                Debug.Log($"[MusicSceneController] Client received battle track #{_syncedBattleTrackIndex}");
            }
        }
    }
}
