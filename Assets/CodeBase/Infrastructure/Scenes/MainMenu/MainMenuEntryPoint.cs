using CodeBase.Infrastructure;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public sealed class MainMenuEntryPoint : MonoBehaviour
{
    private IAppReadyService _appReadyService;

    [Inject]
    public void Construct(IAppReadyService appReadyService)
    {
        _appReadyService = appReadyService;
    }

    private async UniTaskVoid Start()
    {
        await _appReadyService.WaitUntilReadyAsync();
        UnityEngine.Debug.Log("MainMenu ready");
    }
}