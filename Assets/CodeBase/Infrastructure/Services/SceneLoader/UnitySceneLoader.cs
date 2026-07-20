using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.Services.SceneLoader
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        public async UniTask LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            CancellationToken ct = default)
        {
            await SceneManager.LoadSceneAsync(sceneName, mode)
                .ToUniTask(cancellationToken: ct);
        }
    }
}