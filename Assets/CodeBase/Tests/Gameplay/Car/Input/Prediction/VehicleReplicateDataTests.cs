using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Car.Input.Prediction;
using NUnit.Framework;

namespace CodeBase.Tests.Gameplay.Car.Input.Prediction
{
    [TestFixture]
    public class VehicleReplicateDataTests
    {
        [Test]
        public void Constructor_SetsAllFieldsCorrectly()
        {
            // Arrange
            float throttle = 0.75f;
            float steering = -0.5f;
            bool handbrake = true;

            // Act
            VehicleReplicateData data = new VehicleReplicateData(
                throttle,
                steering,
                handbrake);

            // Assert
            Assert.That(data.Throttle, Is.EqualTo(throttle).Within(0.0001f));
            Assert.That(data.Steering, Is.EqualTo(steering).Within(0.0001f));
            Assert.That(data.Handbrake, Is.EqualTo(handbrake));
        }

        [Test]
        public void DefaultConstructor_HasZeroTick()
        {
            // Arrange & Act
            VehicleReplicateData data = default;

            // Assert
            Assert.That(data.GetTick(), Is.EqualTo(0u));
        }

        [Test]
        public void SetTick_UpdatesTickValue()
        {
            // Arrange
            VehicleReplicateData data = default;
            uint expectedTick = 42u;

            // Act
            data.SetTick(expectedTick);

            // Assert
            Assert.That(data.GetTick(), Is.EqualTo(expectedTick));
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            VehicleReplicateData data = new VehicleReplicateData(1f, 0f, false);

            // Act & Assert - should not throw
            Assert.DoesNotThrow(() => data.Dispose());
        }

        [Test]
        public void Throttle_StoresAnyValue()
        {
            // Arrange & Act
            VehicleReplicateData dataMax = new VehicleReplicateData(2f, 0f, false);
            VehicleReplicateData dataMin = new VehicleReplicateData(-2f, 0f, false);
            VehicleReplicateData dataNormal = new VehicleReplicateData(0.5f, 0f, false);

            // Assert - data stores values as-is, clamping is done by input source
            Assert.That(dataMax.Throttle, Is.EqualTo(2f));
            Assert.That(dataMin.Throttle, Is.EqualTo(-2f));
            Assert.That(dataNormal.Throttle, Is.EqualTo(0.5f));
        }

        [Test]
        public void Steering_AllowsFullRange()
        {
            // Arrange & Act
            VehicleReplicateData dataLeft = new VehicleReplicateData(0f, -1f, false);
            VehicleReplicateData dataRight = new VehicleReplicateData(0f, 1f, false);
            VehicleReplicateData dataCenter = new VehicleReplicateData(0f, 0f, false);

            // Assert
            Assert.That(dataLeft.Steering, Is.EqualTo(-1f));
            Assert.That(dataRight.Steering, Is.EqualTo(1f));
            Assert.That(dataCenter.Steering, Is.EqualTo(0f));
        }
    }
}
