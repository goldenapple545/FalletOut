using CodeBase.CodeBase.Gameplay.Network.Match;
using R3;
using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car.Presentation
{
    public sealed class VehicleColorPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerMatchState playerState;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private string colorProperty = "_BaseColor";

        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            if (playerState == null)
                playerState = GetComponentInParent<PlayerMatchState>();
        }

        private void Start()
        {
            if (playerState == null)
            {
                Debug.LogError(
                    $"{nameof(VehicleColorPresenter)}: " +
                    $"{nameof(PlayerMatchState)} не найден.",
                    this);

                return;
            }

            playerState.VehicleColor
                .Subscribe(ApplyColor)
                .AddTo(this);
        }

        private void ApplyColor(Color color)
        {
            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(colorProperty, color);
                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}