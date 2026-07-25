using CodeBase.Infrastructure.Services.SceneLoader;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Car
{
    public sealed class LocalVehicleCameraBinder : NetworkBehaviour
    {
        [SerializeField] private Transform followPoint;

        private IGameplaySceneLifecycle _sceneLifecycle;
        private CameraFollow _cameraFollow;

        [Inject]
        private void Construct(IGameplaySceneLifecycle sceneLifecycle)
        {
            _sceneLifecycle = sceneLifecycle;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                return;

            _sceneLifecycle.GameplaySceneReady += BindCamera;
            _sceneLifecycle.GameplaySceneUnloading += UnbindCamera;

            if (_sceneLifecycle.IsGameplaySceneReady)
                BindCamera();
        }

        public override void OnStopClient()
        {
            Unsubscribe();
            UnbindCamera();

            base.OnStopClient();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void BindCamera()
        {
            if (!IsOwner || _cameraFollow != null)
                return;

            _cameraFollow = FindFirstObjectByType<CameraFollow>();

            if (_cameraFollow == null)
            {
                Debug.LogError(
                    "[LocalPlayerCameraTarget] Gameplay scene is ready, " +
                    "but CameraFollow is absent.",
                    this);

                return;
            }

            _cameraFollow.SetTarget(
                followPoint != null
                    ? followPoint
                    : transform);
        }

        private void UnbindCamera()
        {
            if (_cameraFollow == null)
                return;

            _cameraFollow.SetTarget(null);
            _cameraFollow = null;
        }

        private void Unsubscribe()
        {
            if (_sceneLifecycle == null)
                return;

            _sceneLifecycle.GameplaySceneReady -= BindCamera;
            _sceneLifecycle.GameplaySceneUnloading -= UnbindCamera;
        }
    }
}