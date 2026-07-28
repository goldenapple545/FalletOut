using CodeBase.Infrastructure.Services.Music;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class MusicInstaller : MonoInstaller
    {
        [SerializeField] private MusicConfig musicConfig;
        [SerializeField] private GameObject musicSourcePrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(musicConfig).AsSingle();

            Container.BindInterfacesAndSelfTo<MusicService>()
                .FromMethod(ctx => new MusicService(musicConfig, musicSourcePrefab))
                .AsSingle()
                .NonLazy();
        }
    }
}
