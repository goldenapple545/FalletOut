using CodeBase.CodeBase.Gameplay.Network.Statistics;
using Zenject;

namespace CodeBase.CodeBase.Infrastructure.Installers.Scene
{
    public sealed class MatchSceneInstaller : MonoInstaller
    {
        [UnityEngine.SerializeField, UnityEngine.Min(1)]
        private int damageHistoryCapacity = 256;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VehicleDamageHistory>()
                .AsSingle()
                .WithArguments(damageHistoryCapacity)
                .NonLazy();
        }
    }
}