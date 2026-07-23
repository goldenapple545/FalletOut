using FishNet.Managing;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Network
{
    public sealed class NetworkRuntimeRoot : MonoBehaviour
    {
        [field: SerializeField] public NetworkManager NetworkManager { get; private set; }

        public sealed class Factory : PlaceholderFactory<NetworkRuntimeRoot>
        {
        }
    }
}