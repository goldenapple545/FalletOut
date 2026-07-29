// using CodeBase.Gameplay.Car;
// using CodeBase.Gameplay.Car.Input;
// using CodeBase.Gameplay.Car.Input.Prediction;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using System.Collections;
//
// namespace CodeBase.Tests.Gameplay.Car
// {
//     /// <summary>
//     /// Тесты симуляции машины через VehiclePhysicsMotor — 
//     /// тот же путь, что проходит input в NetworkVehicleController.RunInputs().
//     /// Не требует запущенного FishNet-сервера.
//     /// </summary>
//     [TestFixture]
//     public class VehiclePhysicsMotorTests
//     {
//         private GameObject _vehicleRoot;
//         private GameObject _ground;
//         private PrometeoCarController _prometeo;
//         private VehiclePhysicsMotor _motor;
//         private Rigidbody _rigidbody;
//
//         [SetUp]
//         public void SetUp()
//         {
//             _ground = CreateGroundPlane();
//             _vehicleRoot = CreateTestVehicle();
//             _prometeo = _vehicleRoot.GetComponent<PrometeoCarController>();
//             _motor = _vehicleRoot.GetComponent<VehiclePhysicsMotor>();
//             _rigidbody = _vehicleRoot.GetComponent<Rigidbody>();
//         }
//
//         [TearDown]
//         public void TearDown()
//         {
//             if (_vehicleRoot != null)
//                 Object.DestroyImmediate(_vehicleRoot);
//             if (_ground != null)
//                 Object.DestroyImmediate(_ground);
//         }
//
//         [UnityTest]
//         public IEnumerator Motor_SimulateFullThrottle_Accelerates()
//         {
//             float tickDelta = 0.02f;
//             VehicleInput input = new VehicleInput(1f, 0f, false);
//
//             for (int i = 0; i < 50; i++)
//             {
//                 _motor.Simulate(input, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             Assert.That(_prometeo.ThrottleAxis, Is.GreaterThan(0f),
//                 "ThrottleAxis should increase after full throttle");
//             Assert.That(_prometeo.carSpeed, Is.GreaterThan(0f),
//                 "Car should gain speed with full throttle");
//         }
//
//         [UnityTest]
//         public IEnumerator Motor_SimulateReverse_MovesBackward()
//         {
//             float tickDelta = 0.02f;
//             VehicleInput input = new VehicleInput(-1f, 0f, false);
//
//             for (int i = 0; i < 200; i++)
//             {
//                 _motor.Simulate(input, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             Assert.That(_prometeo.ThrottleAxis, Is.LessThan(-0.5f),
//                 "ThrottleAxis should be negative with reverse input");
//
//             // carSpeed from RPM should be non-zero (wheels rotating)
//             float absSpeed = Mathf.Abs(_prometeo.carSpeed);
//             Assert.That(absSpeed, Is.GreaterThan(1f),
//                 string.Format("Wheels should rotate. carSpeed={0}, throttleAxis={1}, localVZ={2}",
//                     _prometeo.carSpeed, _prometeo.ThrottleAxis, _prometeo.LocalVelocityZ));
//         }
//
//         [UnityTest]
//         public IEnumerator Motor_SimulateSteering_TurnsWheels()
//         {
//             float tickDelta = 0.02f;
//             VehicleInput input = new VehicleInput(0f, 1f, false);
//
//             for (int i = 0; i < 20; i++)
//             {
//                 _motor.Simulate(input, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             Assert.That(_prometeo.SteeringAxis, Is.GreaterThan(0f),
//                 "SteeringAxis should be positive with right input");
//             Assert.That(_prometeo.frontLeftCollider.steerAngle, Is.GreaterThan(0f),
//                 "Front wheels should turn right");
//         }
//
//         [UnityTest]
//         public IEnumerator Motor_SimulateHandbrake_LocksTraction()
//         {
//             float tickDelta = 0.02f;
//             VehicleInput input = new VehicleInput(0f, 0f, true);
//
//             for (int i = 0; i < 10; i++)
//             {
//                 _motor.Simulate(input, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             Assert.That(_prometeo.IsTractionLocked, Is.True,
//                 "Handbrake should lock traction via motor simulation");
//             Assert.That(_prometeo.DriftingAxis, Is.GreaterThan(0f),
//                 "DriftingAxis should increase with handbrake");
//         }
//
//         [UnityTest]
//         public IEnumerator Motor_SimulateThrottleThenNeutral_Decelerates()
//         {
//             float tickDelta = 0.02f;
//
//             // Accelerate first
//             VehicleInput throttleInput = new VehicleInput(1f, 0f, false);
//             for (int i = 0; i < 30; i++)
//             {
//                 _motor.Simulate(throttleInput, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             float speedBeforeDecel = _rigidbody.linearVelocity.magnitude;
//
//             // Release throttle
//             VehicleInput neutral = VehicleInput.Neutral;
//             for (int i = 0; i < 30; i++)
//             {
//                 _motor.Simulate(neutral, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             float speedAfterDecel = _rigidbody.linearVelocity.magnitude;
//
//             Assert.That(speedAfterDecel, Is.LessThan(speedBeforeDecel),
//                 "Car should decelerate when neutral input is applied after throttle");
//         }
//
//         [UnityTest]
//         public IEnumerator Motor_ApplyReconcile_RestoresState()
//         {
//             float tickDelta = 0.02f;
//             VehicleInput input = new VehicleInput(1f, 0.5f, false);
//
//             // Simulate some ticks
//             for (int i = 0; i < 20; i++)
//             {
//                 _motor.Simulate(input, null, tickDelta,
//                     FishNet.Object.Prediction.ReplicateState.Ticked);
//                 Physics.Simulate(Time.fixedDeltaTime);
//                 yield return new WaitForFixedUpdate();
//             }
//
//             // Capture state
//             var reconcileData = new VehicleReconcileData(
//                 null,
//                 _prometeo.SteeringAxis,
//                 _prometeo.ThrottleAxis,
//                 _prometeo.DriftingAxis,
//                 _prometeo.IsDrifting,
//                 _prometeo.IsTractionLocked);
//
//             // Apply reconcile
//             _motor.ApplyReconcile(reconcileData);
//
//             Assert.That(_prometeo.SteeringAxis,
//                 Is.EqualTo(reconcileData.SteeringAxis).Within(0.001f));
//             Assert.That(_prometeo.ThrottleAxis,
//                 Is.EqualTo(reconcileData.ThrottleAxis).Within(0.001f));
//             Assert.That(_prometeo.DriftingAxis,
//                 Is.EqualTo(reconcileData.DriftingAxis).Within(0.001f));
//             Assert.That(_prometeo.IsTractionLocked,
//                 Is.EqualTo(reconcileData.IsTractionLocked));
//         }
//
//         private GameObject CreateTestVehicle()
//         {
//             var go = new GameObject("TestVehicle");
//             go.transform.position = Vector3.zero;
//
//             var rb = go.AddComponent<Rigidbody>();
//             rb.mass = 1200f;
//             rb.interpolation = RigidbodyInterpolation.Interpolate;
//             rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
//
//             var prometeo = go.AddComponent<PrometeoCarController>();
//             prometeo.maxSpeed = 90;
//             prometeo.maxReverseSpeed = 45;
//             prometeo.accelerationMultiplier = 2;
//             prometeo.maxSteeringAngle = 27;
//             prometeo.steeringSpeed = 0.5f;
//             prometeo.brakeForce = 350;
//             prometeo.decelerationMultiplier = 2;
//             prometeo.handbrakeDriftMultiplier = 5;
//             prometeo.bodyMassCenter = new Vector3(0f, -0.5f, 0f);
//             prometeo.useSounds = false;
//
//             prometeo.frontLeftCollider = CreateWheel("FL", go, new Vector3(-0.8f, -0.3f, 1.2f));
//             prometeo.frontRightCollider = CreateWheel("FR", go, new Vector3(0.8f, -0.3f, 1.2f));
//             prometeo.rearLeftCollider = CreateWheel("RL", go, new Vector3(-0.8f, -0.3f, -1.2f));
//             prometeo.rearRightCollider = CreateWheel("RR", go, new Vector3(0.8f, -0.3f, -1.2f));
//
//             prometeo.InitializeManual();
//
//             var motor = go.AddComponent<VehiclePhysicsMotor>();
//             var motorType = typeof(VehiclePhysicsMotor);
//             var prometeoField = motorType.GetField("prometeo",
//                 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//             if (prometeoField != null)
//                 prometeoField.SetValue(motor, prometeo);
//
//             return go;
//         }
//
//         private WheelCollider CreateWheel(string name, GameObject car, Vector3 localPos)
//         {
//             var wheelGo = new GameObject(name + "_Wheel");
//             wheelGo.transform.SetParent(car.transform);
//             wheelGo.transform.localPosition = localPos;
//
//             var wc = wheelGo.AddComponent<WheelCollider>();
//             wc.radius = 0.35f;
//             wc.suspensionDistance = 0.3f;
//
//             return wc;
//         }
//
//         private GameObject CreateGroundPlane()
//         {
//             var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
//             ground.name = "TestGround";
//             ground.transform.position = new Vector3(0, -0.65f, 0);
//             ground.transform.localScale = new Vector3(10, 1, 10);
//             return ground;
//         }
//     }
// }
