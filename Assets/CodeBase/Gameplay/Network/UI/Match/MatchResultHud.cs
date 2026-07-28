using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.Infrastructure.Services.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Network.UI.Match
{
    public sealed class MatchResultsHud : MonoBehaviour
    {
        [SerializeField] private GameObject resultsRoot;
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private Button restartButton;

        private ISessionService _sessionService;
        private MatchManager _matchManager;

        [Inject]
        private void Construct(MatchManager matchManager, ISessionService sessionService)
        {
            _matchManager = matchManager;
            _sessionService = sessionService;
            
            Init();
        }
        
        public void Init()
        {
            _matchManager.OnPhaseChanged += HandlePhaseChanged;

            restartButton.gameObject.SetActive(
                _sessionService.IsHostStarted);

            restartButton.onClick.AddListener(() =>
                _matchManager.RestartServer());

            resultsRoot.SetActive(
                _matchManager.Phase == MatchPhase.RoundEnded);
        }

        private void HandlePhaseChanged(MatchPhase phase)
        {
            bool isRoundEnded = phase == MatchPhase.RoundEnded;

            resultsRoot.SetActive(isRoundEnded);

            if (isRoundEnded)
            {
                winnerText.text =
                    $"Winner: {_matchManager.WinnerName}";
            }
        }
    }
}