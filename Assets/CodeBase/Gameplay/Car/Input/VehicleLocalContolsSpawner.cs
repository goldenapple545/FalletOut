using CodeBase.Data;
using CodeBase.Infrastructure.Services.SceneLoader;
using FishNet.Object;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;

namespace CodeBase.Gameplay.Car.Input
{
    public sealed class VehicleLocalControlsSpawner : NetworkBehaviour
    {
        [SerializeField] private VehicleInputSource inputSource;
        [SerializeField] private VehicleTouchControlsView touchControlsPrefab;

        private BuildConfig _buildConfig;
        private VehicleTouchControlsView _spawnedControls;
        private IGameplaySceneLifecycle _sceneLifecycle;
        private bool _isLocalOwner;

        [Inject]
        private void Construct(BuildConfig buildConfig, IGameplaySceneLifecycle sceneLifecycle)
        {
            _buildConfig = buildConfig;
            _sceneLifecycle = sceneLifecycle;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner || !_buildConfig.IsAndroid)
                return;
            
            _isLocalOwner = true;
            
            _sceneLifecycle.GameplaySceneReady += OnGameplaySceneReady;
            _sceneLifecycle.GameplaySceneUnloading += DestroyControls;

            if (_sceneLifecycle.IsGameplaySceneReady)
                OnGameplaySceneReady();
        }

        public override void OnStopClient()
        {
            Unsubscribe();
            DestroyControls();
            
            base.OnStopClient();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            DestroyControls();
        }

        private void OnGameplaySceneReady()
        {
            if (!_isLocalOwner || _spawnedControls != null)
                return;

            if (inputSource == null || touchControlsPrefab == null)
            {
                Debug.LogError(
                    "[VehicleControls] Input source or touch-controls prefab is missing.",
                    this);

                return;
            }

            _spawnedControls = Instantiate(touchControlsPrefab);
            _spawnedControls.Bind(inputSource);

            Debug.Log(
                "[VehicleControls] Local mobile controls were created.",
                _spawnedControls);
        }
        
        private void Unsubscribe()
        {
            if (_sceneLifecycle == null)
                return;

            _sceneLifecycle.GameplaySceneReady -= OnGameplaySceneReady;
            _sceneLifecycle.GameplaySceneUnloading -= DestroyControls;
        }

        private void DestroyControls()
        {
            if (_spawnedControls == null)
                return;

            inputSource?.ResetTouch();

            Destroy(_spawnedControls.gameObject);
            _spawnedControls = null;
        }
    }
}