using CodeBase.CodeBase.Gameplay.Network.UI.Lobby;
using CodeBase.Gameplay.Network.UI;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Scene
{
    public sealed class LobbySceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<LobbyBrowserView>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container.Bind<HostLobbyView>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container.Bind<ClientLobbyView>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<LobbyScreenController>()
                .AsSingle()
                .NonLazy();
        }
    }
}