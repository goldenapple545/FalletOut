using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using CodeBase.Infrastructure;
using CodeBase.Infrastructure.BootstrapSteps;
using Zenject;

public sealed class AppBootstrapper : IInitializable, IDisposable
{
    private readonly IReadOnlyList<IAppBootstrapStep> _steps;
    private readonly IAppReadyService _appReadyService;
    private readonly CancellationTokenSource _cts = new();

    public AppBootstrapper(
        List<IAppBootstrapStep> steps,
        IAppReadyService appReadyService)
    {
        _steps = steps;
        _appReadyService = appReadyService;
    }

    public void Initialize()
    {
        RunAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid RunAsync(CancellationToken ct)
    {
        try
        {
            foreach (var step in _steps)
                await step.ExecuteAsync(ct);

            _appReadyService.MarkReady();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}