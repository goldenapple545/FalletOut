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
        [SerializeField] private TMP_Text historyText;
        [SerializeField] private Button restartButton;

        private MatchManager _matchManager;
        private ISessionService _sessionService;

        [Inject]
        private void Construct(
            MatchManager matchManager,
            ISessionService sessionService)
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

            if (isRoundEnded)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            winnerText.text =
                $"Winner: {_matchManager.WinnerName}";

            historyText.text = _matchManager.DamageHistoryText;

            resultsRoot.SetActive(true);
        }

        private void Hide()
        {
            resultsRoot.SetActive(false);
        }
    }
}