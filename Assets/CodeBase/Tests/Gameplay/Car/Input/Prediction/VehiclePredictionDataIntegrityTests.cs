// using CodeBase.Gameplay.Car.Input.Prediction;
// using FishNet.Component.Prediction;
// using FishNet.Object.Prediction;
// using NUnit.Framework;
//
// namespace CodeBase.Tests.Gameplay.Car.Input.Prediction
// {
//     /// <summary>
//     /// Tests for prediction data integrity - verifies that replicate and reconcile data
//     /// can be correctly created, transmitted, and applied without corruption.
//     /// </summary>
//     [TestFixture]
//     public class VehiclePredictionDataIntegrityTests
//     {
//         [Test]
//         public void ReplicateDataRoundtrip_PreservesAllValues()
//         {
//             // Arrange
//             float throttle = 0.8f;
//             float steering = -0.3f;
//             bool handbrake = true;
//
//             // Act - create data
//             VehicleReplicateData original = new VehicleReplicateData(
//                 throttle,
//                 steering,
//                 handbrake);
//
//             // Simulate transmission (copy to new struct)
//             VehicleReplicateData received = original;
//
//             // Assert
//             Assert.That(received.Throttle, Is.EqualTo(original.Throttle).Within(0.0001f));
//             Assert.That(received.Steering, Is.EqualTo(original.Steering).Within(0.0001f));
//             Assert.That(received.Handbrake, Is.EqualTo(original.Handbrake));
//         }
//
//         [Test]
//         public void ReconcileDataRoundtrip_PreservesAllValues()
//         {
//             // Arrange
//             PredictionRigidbody prb = new PredictionRigidbody();
//
//             // Act - create data
//             VehicleReconcileData original = new VehicleReconcileData(
//                 prb,
//                 0.6f,  // steeringAxis
//                 0.9f,  // throttleAxis
//                 0f,    // driftingAxis
//                 true,  // isDrifting
//                 false  // isTractionLocked
//             );
//
//             // Simulate transmission (copy to new struct)
//             VehicleReconcileData received = original;
//
//             // Assert
//             Assert.That(received.SteeringAxis, Is.EqualTo(0.6f).Within(0.0001f));
//             Assert.That(received.ThrottleAxis, Is.EqualTo(0.9f).Within(0.0001f));
//             Assert.That(received.DriftingAxis, Is.EqualTo(0f).Within(0.0001f));
//             Assert.That(received.IsDrifting, Is.True);
//             Assert.That(received.IsTractionLocked, Is.False);
//             Assert.That(received.Rigidbody, Is.EqualTo(prb));
//         }
//
//         [Test]
//         public void ReplicateData_DefaultValues_AreValid()
//         {
//             // Arrange & Act
//             VehicleReplicateData data = default;
//
//             // Assert - default struct should have zero values
//             Assert.That(data.Throttle, Is.EqualTo(0f));
//             Assert.That(data.Steering, Is.EqualTo(0f));
//             Assert.That(data.Handbrake, Is.False);
//             Assert.That(data.GetTick(), Is.EqualTo(0u));
//         }
//
//         [Test]
//         public void ReconcileData_DefaultValues_AreValid()
//         {
//             // Arrange & Act
//             VehicleReconcileData data = default;
//
//             // Assert - default struct should have zero values
//             Assert.That(data.SteeringAxis, Is.EqualTo(0f));
//             Assert.That(data.ThrottleAxis, Is.EqualTo(0f));
//             Assert.That(data.DriftingAxis, Is.EqualTo(0f));
//             Assert.That(data.IsDrifting, Is.False);
//             Assert.That(data.IsTractionLocked, Is.False);
//             Assert.That(data.GetTick(), Is.EqualTo(0u));
//         }
//
//         [Test]
//         public void MultipleReplicateData_CanBeCreatedIndependently()
//         {
//             // Arrange & Act
//             VehicleReplicateData data1 = new VehicleReplicateData(0.5f, 0f, false);
//             VehicleReplicateData data2 = new VehicleReplicateData(-0.5f, 1f, true);
//
//             // Set different ticks
//             data1.SetTick(10u);
//             data2.SetTick(20u);
//
//             // Assert - they should be independent
//             Assert.That(data1.Throttle, Is.EqualTo(0.5f));
//             Assert.That(data2.Throttle, Is.EqualTo(-0.5f));
//             Assert.That(data1.GetTick(), Is.EqualTo(10u));
//             Assert.That(data2.GetTick(), Is.EqualTo(20u));
//         }
//
//         [Test]
//         public void ReconcileData_TickManagement_WorksCorrectly()
//         {
//             // Arrange
//             VehicleReconcileData[] dataArray = new VehicleReconcileData[5];
//
//             // Act - create array with incrementing ticks
//             for (int i = 0; i < dataArray.Length; i++)
//             {
//                 dataArray[i] = new VehicleReconcileData(
//                     new PredictionRigidbody(),
//                     0f, 0f, 0f, false, false);
//                 dataArray[i].SetTick((uint)(i * 10));
//             }
//
//             // Assert
//             for (int i = 0; i < dataArray.Length; i++)
//             {
//                 Assert.That(dataArray[i].GetTick(), Is.EqualTo((uint)(i * 10)));
//             }
//         }
//
//         [Test]
//         public void ReplicateData_ExtremeValues_AreStoredWithoutLoss()
//         {
//             // Arrange & Act
//             VehicleReplicateData maxValues = new VehicleReplicateData(
//                 float.MaxValue,
//                 float.MaxValue,
//                 true);
//
//             VehicleReplicateData minValues = new VehicleReplicateData(
//                 float.MinValue,
//                 float.MinValue,
//                 false);
//
//             // Assert
//             Assert.That(maxValues.Throttle, Is.EqualTo(float.MaxValue));
//             Assert.That(maxValues.Steering, Is.EqualTo(float.MaxValue));
//             Assert.That(minValues.Throttle, Is.EqualTo(float.MinValue));
//             Assert.That(minValues.Steering, Is.EqualTo(float.MinValue));
//         }
//     }
// }
