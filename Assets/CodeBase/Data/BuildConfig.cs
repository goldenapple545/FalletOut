using System;
using UnityEngine;

namespace CodeBase.Data
{
    [CreateAssetMenu(
        fileName = "BuildConfig",
        menuName = "StaticData/BuildConfig")]
    public class BuildConfig: ScriptableObject
    {
        [field: SerializeField] public Platform BuildPlatform { get; private set; } = Platform.PC;
        
        public bool IsPc => BuildPlatform == Platform.PC;
        public bool IsAndroid => BuildPlatform == Platform.Android;
    }

    [Serializable]
    public enum Platform
    {
        PC = 0,
        Android = 1
    } 
}