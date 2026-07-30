using CodeBase.CodeBase.Gameplay.Car;
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
        private ICameraFollow _сameraFollow;

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
            if (!IsOwner || _сameraFollow != null)
                return;

            FindCamera();

            if (_сameraFollow == null)
            {
                Debug.LogError(
                    "[LocalPlayerCameraTarget] Gameplay scene is ready, " +
                    "but CameraFollow is absent.",
                    this);

                return;
            }

            _сameraFollow.SetTarget(
                followPoint != null
                    ? followPoint
                    : transform);
        }

        private void FindCamera()
        {
            var candidates = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (var c in candidates)
            {
                if (c is ICameraFollow follow)
                {
                    _сameraFollow = follow;
                    break;
                }
            }
        }

        private void UnbindCamera()
        {
            if (_сameraFollow == null)
                return;

            _сameraFollow.SetTarget(null);
            _сameraFollow = null;
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