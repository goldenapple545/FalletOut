using CodeBase.Data;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Car.Input
{
    public sealed class VehicleLocalControlsSpawner : NetworkBehaviour
    {
        [SerializeField] private VehicleInputSource inputSource;
        [SerializeField] private VehicleTouchControlsView touchControlsPrefab;

        private BuildConfig _buildConfig;
        private VehicleTouchControlsView _spawnedControls;

        [Inject]
        private void Construct(BuildConfig buildConfig)
        {
            _buildConfig = buildConfig;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner || !_buildConfig.IsAndroid)
                return;

            if (inputSource == null)
            {
                Debug.LogError(
                    $"[{nameof(VehicleLocalControlsSpawner)}] " +
                    $"Input source is not assigned.",
                    this);

                return;
            }

            if (touchControlsPrefab == null)
            {
                Debug.LogError(
                    $"[{nameof(VehicleLocalControlsSpawner)}] " +
                    $"Touch controls prefab is not assigned.",
                    this);

                return;
            }

            _spawnedControls = Instantiate(touchControlsPrefab);
            _spawnedControls.Bind(inputSource);
        }

        public override void OnStopClient()
        {
            DestroyControls();
            base.OnStopClient();
        }

        private void OnDestroy()
        {
            DestroyControls();
        }

        private void DestroyControls()
        {
            if (_spawnedControls == null)
                return;

            _spawnedControls.gameObject.SetActive(false);
            Destroy(_spawnedControls.gameObject);
            _spawnedControls = null;
            
            inputSource?.ResetTouch();
        }
    }
}