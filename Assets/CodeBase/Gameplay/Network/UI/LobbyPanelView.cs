using UnityEngine;

namespace CodeBase.Gameplay.Network.UI
{
    public abstract class LobbyPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        protected virtual void Awake()
        {
            if (root == null)
                root = gameObject;
        }

        public void Show() => root.SetActive(true);
        public void Hide() => root.SetActive(false);
    }
}