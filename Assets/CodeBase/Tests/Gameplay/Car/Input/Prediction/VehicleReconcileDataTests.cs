using CodeBase.Gameplay.Car.Input.Prediction;
using FishNet.Component.Prediction;
using FishNet.Object.Prediction;
using NUnit.Framework;

namespace CodeBase.Tests.Gameplay.Car.Input.Prediction
{
    [TestFixture]
    public class VehicleReconcileDataTests
    {
        [Test]
        public void Constructor_SetsAxisFieldsCorrectly()
        {
            // Arrange
            PredictionRigidbody prb = new PredictionRigidbody();
            float steeringAxis = 0.5f;
            float throttleAxis = 0.75f;
            float driftingAxis = 0f;
            bool isDrifting = true;
            bool isTractionLocked = false;

            // Act
            VehicleReconcileData data = new VehicleReconcileData(
                prb,
                steeringAxis,
                throttleAxis,
                driftingAxis,
                isDrifting,
                isTractionLocked);

            // Assert
            Assert.That(data.SteeringAxis, Is.EqualTo(steeringAxis).Within(0.0001f));
            Assert.That(data.ThrottleAxis, Is.EqualTo(throttleAxis).Within(0.0001f));
            Assert.That(data.DriftingAxis, Is.EqualTo(driftingAxis).Within(0.0001f));
            Assert.That(data.IsDrifting, Is.EqualTo(isDrifting));
            Assert.That(data.IsTractionLocked, Is.EqualTo(isTractionLocked));
            Assert.That(data.Rigidbody, Is.EqualTo(prb));
        }

        [Test]
        public void DefaultConstructor_HasZeroTick()
        {
            // Arrange & Act
            VehicleReconcileData data = default;

            // Assert
            Assert.That(data.GetTick(), Is.EqualTo(0u));
        }

        [Test]
        public void SetTick_UpdatesTickValue()
        {
            // Arrange
            VehicleReconcileData data = default;
            uint expectedTick = 100u;

            // Act
            data.SetTick(expectedTick);

            // Assert
            Assert.That(data.GetTick(), Is.EqualTo(expectedTick));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            VehicleReconcileData data = new VehicleReconcileData(
                new PredictionRigidbody(),
                0f, 0f, 0f, false, false);

            // Act & Assert - should not throw
            Assert.DoesNotThrow(() => data.Dispose());
        }

        [TestCase(0f, 0f, 0f, false, false)]
        [TestCase(1f, 1f, 1f, true, true)]
        [TestCase(-1f, -1f, 0f, false, true)]
        public void VariousAxisValues_AreStoredCorrectly(
            float steering,
            float throttle,
            float drift,
            bool drifting,
            bool tractionLocked)
        {
            // Arrange & Act
            VehicleReconcileData data = new VehicleReconcileData(
                new PredictionRigidbody(),
                steering,
                throttle,
                drift,
                drifting,
                tractionLocked);

            // Assert
            Assert.That(data.SteeringAxis, Is.EqualTo(steering).Within(0.0001f));
            Assert.That(data.ThrottleAxis, Is.EqualTo(throttle).Within(0.0001f));
            Assert.That(data.DriftingAxis, Is.EqualTo(drift).Within(0.0001f));
            Assert.That(data.IsDrifting, Is.EqualTo(drifting));
            Assert.That(data.IsTractionLocked, Is.EqualTo(tractionLocked));
        }
    }
}
