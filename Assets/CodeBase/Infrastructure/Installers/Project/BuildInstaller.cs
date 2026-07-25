using CodeBase.Data;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers.Project
{
    public sealed class BuildInstaller : MonoInstaller
    {
        [SerializeField] private BuildConfig config;
        
        public override void InstallBindings()
        {
            Container.Bind<BuildConfig>().FromInstance(config).AsSingle();
        }
    }
}