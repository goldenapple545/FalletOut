using CodeBase.Infrastructure.Networking;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Scene
{
    public sealed class FishNetZenjectBridgeInstaller : MonoInstaller
    {
        [SerializeField] private FishNetZenjectBridge bridge;
        
        public override void InstallBindings()
        {
            ZenjectObjectPool.ResetReadyState();
            
            if (bridge != null)
                bridge.RegisterSceneContainer(gameObject.scene, Container);
        }
        
        private void OnDestroy()
        {
            if (bridge != null)
                bridge.UnregisterSceneContainer(gameObject.scene);
        }
    }
}