using R3;
using UnityEngine;

namespace CodeBase.CodeBase.Infrastructure
{
    public static class R3Initializer
    {
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            ObservableSystem.RegisterUnhandledExceptionHandler(
                static exception => Debug.LogException(exception));

            ObservableSystem.DefaultTimeProvider =
                UnityTimeProvider.Update;

            ObservableSystem.DefaultFrameProvider =
                UnityFrameProvider.Update;
        }
    }
}