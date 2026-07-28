using CodeBase.Gameplay.Network.Lobby;
using TMPro;
using UnityEngine;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class PlayerSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private GameObject hostBadge;
        [SerializeField] private GameObject readyIndicator;
        [SerializeField] private GameObject waitingIndicator;

        public void Bind(LobbyPlayerInfo player)
        {
            if (playerNameText != null)
            {
                string hostSuffix = player.IsHost ? "  [HOST]" : string.Empty;
                playerNameText.text = $"{player.DisplayName}{hostSuffix}";
            }

            if (hostBadge != null)
                hostBadge.SetActive(player.IsHost);

            SetReady(player.IsReady);
        }

        public void SetReady(bool ready)
        {
            if (readyIndicator != null)
                readyIndicator.SetActive(ready);

            if (waitingIndicator != null)
                waitingIndicator.SetActive(!ready);
        }
    }
}