using System;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class LobbyServerItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text playersText;
        [SerializeField] private Button joinButton;

        private IPEndPoint _endpoint;

        public void Bind(ServerInfo info, Action<IPEndPoint> onSelected)
        {
            _endpoint = info.EndPoint;

            if (nameText != null)
                nameText.text = info.Name;

            if (playersText != null)
                playersText.text = $"{info.CurrentPlayers}/{info.MaxPlayers}";

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => onSelected?.Invoke(_endpoint));
        }

        private void OnDestroy()
        {
            joinButton.onClick.RemoveAllListeners();
        }
    }
}