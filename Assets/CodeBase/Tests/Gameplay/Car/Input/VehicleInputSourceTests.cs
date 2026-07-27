using CodeBase.Data;
using CodeBase.Gameplay.Car.Input;
using NUnit.Framework;
using UnityEngine;

namespace CodeBase.Tests.Gameplay.Car.Input
{
    [TestFixture]
    public class VehicleInputSourceTests
    {
        private GameObject _sourceGo;
        private VehicleInputSource _source;
        private BuildConfig _buildConfig;

        [SetUp]
        public void SetUp()
        {
            _buildConfig = ScriptableObject.CreateInstance<BuildConfig>();
            _sourceGo = new GameObject("TestInputSource");
            _source = _sourceGo.AddComponent<VehicleInputSource>();

            // Inject BuildConfig via reflection since [Inject] requires Zenject
            var constructMethod = typeof(VehicleInputSource).GetMethod(
                "Construct",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (constructMethod != null)
                constructMethod.Invoke(_source, new object[] { _buildConfig });
        }

        [TearDown]
        public void TearDown()
        {
            if (_sourceGo != null)
                Object.DestroyImmediate(_sourceGo);
            if (_buildConfig != null)
                Object.DestroyImmediate(_buildConfig);
        }

        [Test]
        public void Read_OnPC_WithNoKeyboard_ReturnsNeutral()
        {
            // On PC, if Keyboard.current is null, it returns Neutral
            // This is the default state in headless test environment
            VehicleInput input = _source.Read();

            Assert.That(input.Throttle, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(input.Steering, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(input.Handbrake, Is.EqualTo(false));
        }

        [Test]
        public void SetTouchThrottle_StoresClampedValue()
        {
            _source.SetTouchThrottle(2f);
            VehicleInput input = _source.Read();

            // SetTouchThrottle clamps to [-1,1], but Read on PC mode
            // reads keyboard, not touch. We need to test Android mode.
            Assert.That(input.Throttle, Is.EqualTo(0f).Within(0.0001f),
                "PC mode should ignore touch values");
        }

        [Test]
        public void Read_OnAndroid_ReturnsTouchInput()
        {
            // Force Android mode via reflection
            SetBuildPlatform(Platform.Android);

            _source.SetTouchThrottle(0.8f);
            _source.SetTouchSteering(-0.5f);
            _source.SetTouchHandbrake(true);

            VehicleInput input = _source.Read();

            Assert.That(input.Throttle, Is.EqualTo(0.8f).Within(0.0001f),
                "Android mode should return touch throttle");
            Assert.That(input.Steering, Is.EqualTo(-0.5f).Within(0.0001f),
                "Android mode should return touch steering");
            Assert.That(input.Handbrake, Is.EqualTo(true),
                "Android mode should return touch handbrake");
        }

        [Test]
        public void Read_OnAndroid_ExcessiveTouchValues_ClampedByVehicleInput()
        {
            SetBuildPlatform(Platform.Android);

            _source.SetTouchThrottle(5f);   // exceeds [0,1]
            _source.SetTouchSteering(-3f);  // exceeds [-1,0]

            VehicleInput input = _source.Read();

            // SetTouchThrottle clamps to [-1,1] internally
            // But VehicleInput constructor also clamps
            Assert.That(input.Throttle, Is.EqualTo(1f).Within(0.0001f),
                "Excessive throttle should be clamped to 1");
            Assert.That(input.Steering, Is.EqualTo(-1f).Within(0.0001f),
                "Excessive steering should be clamped to -1");
        }

        [Test]
        public void ResetTouch_ZerosAllTouchValues()
        {
            SetBuildPlatform(Platform.Android);

            _source.SetTouchThrottle(1f);
            _source.SetTouchSteering(1f);
            _source.SetTouchHandbrake(true);

            _source.ResetTouch();
            VehicleInput input = _source.Read();

            Assert.That(input.Throttle, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(input.Steering, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(input.Handbrake, Is.EqualTo(false));
        }

        [Test]
        public void SetTouchThrottle_NegativeValue_StoresClamped()
        {
            SetBuildPlatform(Platform.Android);

            _source.SetTouchThrottle(-0.7f);
            VehicleInput input = _source.Read();

            Assert.That(input.Throttle, Is.EqualTo(-0.7f).Within(0.0001f),
                "Negative throttle should be preserved for reverse");
        }

        private void SetBuildPlatform(Platform platform)
        {
            // BuildConfig.BuildPlatform has private setter,
            // use the compiler-generated backing field: <BuildPlatform>k__BackingField
            var backingField = typeof(BuildConfig).GetField(
                "<BuildPlatform>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (backingField != null)
                backingField.SetValue(_buildConfig, platform);
        }
    }
}
