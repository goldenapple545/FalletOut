// ZenjectObjectPool.cs — вешаешь на тот же GO что NetworkManager

using System.Collections.Generic;
using FishNet.Managing.Object;
using FishNet.Object;
using FishNet.Utility.Extension;
using FishNet.Utility.Performance;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class ZenjectObjectPool : ObjectPool
    {
        private static bool _containerReady = false;
        
        // Очередь объектов, которые ждут инжекта
        private static readonly List<(GameObject go, bool makeActive)> _pendingInject = new();
        
        public static void NotifyContainerReady()
        {
            _containerReady = true;
            FlushPending();
        }

        public static void ResetReadyState()
        {
            _containerReady = false;
            _pendingInject.Clear();
        }
        
        private DiContainer GetSceneContainer()
        {
            // Возвращает контейнер активной сцены (SceneContext текущей сцены)
            var sceneContext = GameObject.FindObjectOfType<SceneContext>();
            return sceneContext != null ? sceneContext.Container : ProjectContext.Instance.Container;
        }
    
        public override NetworkObject RetrieveObject(
            int prefabId,
            ushort collectionId,
            ObjectPoolRetrieveOption options,
            Transform parent = null,
            Vector3? nullablePosition = null,
            Quaternion? nullableRotation = null,
            Vector3? nullableScale = null,
            bool asServer = true)
        {
            NetworkObject prefab = GetPrefab(prefabId, collectionId, asServer);

            if (prefab == null)
            {
                Debug.LogError(
                    $"[{nameof(ZenjectObjectPool)}] " +
                    $"Prefab not found: prefabId={prefabId}, collectionId={collectionId}, " +
                    $"asServer={asServer}.");

                return null;
            }

            bool localSpace = options.FastContains(ObjectPoolRetrieveOption.LocalSpace);

            Vector3 position;
            Quaternion rotation;
            Vector3 scale;

            if (localSpace)
            {
                prefab.transform.OutLocalPropertyValues(
                    nullablePosition,
                    nullableRotation,
                    nullableScale,
                    out position,
                    out rotation,
                    out scale);

                if (parent != null)
                {
                    position = parent.TransformPoint(position);
                    rotation = parent.rotation * rotation;
                }
            }
            else
            {
                prefab.transform.OutWorldPropertyValues(
                    nullablePosition,
                    nullableRotation,
                    nullableScale,
                    out position,
                    out rotation,
                    out scale);
            }

            NetworkObject instance = InstantiateWithZenject(
                prefab.gameObject,
                position,
                rotation,
                scale,
                parent,
                makeActive: false);

            Debug.Log(
                $"[{nameof(ZenjectObjectPool)}] Created '{instance.name}', " +
                $"asServer={asServer}, scene={instance.gameObject.scene.name}.",
                instance);

            return instance;
        }

        private NetworkObject InstantiateWithZenject(
            GameObject prefab, Vector3 pos, Quaternion rot, Vector3 scale,
            Transform parent, bool makeActive)
        {
            var container = GetSceneContainer();
            var go = container.InstantiatePrefab(prefab, pos, rot, parent);
            go.SetActive(false);
            var nob = go.GetComponent<NetworkObject>();
            nob.transform.localScale = scale;
            if (makeActive) go.SetActive(true);
            return nob;
        }

        private static void FlushPending()
        {
            if (_pendingInject.Count == 0) return;

            var instance = FindObjectOfType<ZenjectObjectPool>();
            if (instance == null) return;

            var container = instance.GetSceneContainer();

            foreach (var (go, makeActive) in _pendingInject)
            {
                if (go == null) continue;
                try
                {
                    container.Inject(go);
                    if (makeActive) go.SetActive(true);
                }
                catch (ZenjectException e)
                {
                    Debug.LogError($"[ZenjectObjectPool] FlushPending inject failed: {e.Message}");
                }
            }

            _pendingInject.Clear();
        }

        public override NetworkObject GetPrefab(int prefabId, ushort collectionId, bool asServer)
        {
            PrefabObjects po = NetworkManager.GetPrefabObjects<PrefabObjects>(collectionId, false);
            return po?.GetObject(asServer, prefabId);
        }

        public override void StoreObject(NetworkObject instantiated, bool asServer)
        {
            Destroy(instantiated.gameObject);
        }

        public override List<NetworkObject> StorePrefabObjects(NetworkObject prefab, int count, bool asServer)
            => null;
    }
}