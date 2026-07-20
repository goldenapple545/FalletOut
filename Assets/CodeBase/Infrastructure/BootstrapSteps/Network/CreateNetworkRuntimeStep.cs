using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.BootstrapSteps.Network
{
    public sealed class CreateNetworkRuntimeStep : IAppBootstrapStep
    {
        private readonly NetworkRuntimeRoot.Factory _factory;
        private NetworkRuntimeRoot _instance;

        public CreateNetworkRuntimeStep(NetworkRuntimeRoot.Factory factory)
        {
            _factory = factory;
        }

        public UniTask ExecuteAsync(CancellationToken ct)
        {
            if (_instance == null)
            {
                _instance = _factory.Create();
                Object.DontDestroyOnLoad(_instance.gameObject);
            }

            return UniTask.CompletedTask;
        }
    }
}