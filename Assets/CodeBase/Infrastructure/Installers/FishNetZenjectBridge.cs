using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CodeBase.Infrastructure.Networking
{
    /// <summary>
    /// Инжектит Zenject-зависимости в NetworkObject,
    /// создаваемые FishNet на клиенте.
    /// </summary>
    public sealed class FishNetZenjectBridge : MonoBehaviour
    {
        private readonly Dictionary<int, DiContainer> _containersBySceneHandle = new();

        private DiContainer _projectContainer;

        private void Awake()
        {
            _projectContainer = ProjectContext.Instance.Container;
        }

        private void OnEnable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.Objects.OnBeforeSpawn +=
                    InjectBeforeSpawn;
            }
        }

        private void OnDisable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.Objects.OnBeforeSpawn -=
                    InjectBeforeSpawn;
            }
        }

        public void RegisterSceneContainer(Scene scene, DiContainer container)
        {
            if (!scene.IsValid() || container == null)
                return;

            _containersBySceneHandle[scene.handle] = container;
        }

        public void UnregisterSceneContainer(Scene scene)
        {
            if (!scene.IsValid())
                return;

            _containersBySceneHandle.Remove(scene.handle);
        }

        private void InjectBeforeSpawn(NetworkObject networkObject)
        {
            if (networkObject == null)
                return;

            Scene objectScene = networkObject.gameObject.scene;

            DiContainer container = ResolveContainer(objectScene);

            if (container == null)
            {
                Debug.LogWarning(
                    $"[{nameof(FishNetZenjectBridge)}] " +
                    $"No Zenject container for spawned object " +
                    $"'{networkObject.name}' in scene '{objectScene.name}'.",
                    networkObject);

                return;
            }

            container.InjectGameObject(networkObject.gameObject);
        }

        private DiContainer ResolveContainer(Scene scene)
        {
            if (scene.IsValid() &&
                _containersBySceneHandle.TryGetValue(
                    scene.handle,
                    out DiContainer sceneContainer) &&
                sceneContainer != null)
            {
                return sceneContainer;
            }

            return _projectContainer;
        }
    }
}