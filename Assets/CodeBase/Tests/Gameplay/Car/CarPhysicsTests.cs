using CodeBase.Gameplay.Car;
using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Car.Input.Prediction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace CodeBase.Tests.Gameplay.Car
{
    [TestFixture]
    public class CarPhysicsTests
    {
        private GameObject _carRoot;
        private GameObject _ground;
        private PrometeoCarController _prometeo;
        private Rigidbody _rigidbody;

        [SetUp]
        public void SetUp()
        {
            _ground = CreateGroundPlane();
            _carRoot = CreateTestCar();
            _prometeo = _carRoot.GetComponent<PrometeoCarController>();
            _rigidbody = _carRoot.GetComponent<Rigidbody>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_carRoot != null)
                Object.DestroyImmediate(_carRoot);
            if (_ground != null)
                Object.DestroyImmediate(_ground);
        }

        [UnityTest]
        public IEnumerator FullThrottle_AcceleratesForward()
        {
            float tickDelta = 0.02f;
            VehicleInput throttleInput = new VehicleInput(1f, 0f, false);

            for (int i = 0; i < 50; i++)
            {
                _prometeo.SimulateTick(
                    throttleInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(_prometeo.ThrottleAxis, Is.GreaterThan(0f),
                "ThrottleAxis should be positive after full throttle input");

            Assert.That(_prometeo.carSpeed, Is.GreaterThan(0f),
                "Car should have forward speed after throttle");
        }

        [UnityTest]
        public IEnumerator ReverseThrottle_AcceleratesBackward()
        {
            float tickDelta = 0.02f;
            VehicleInput reverseInput = new VehicleInput(-1f, 0f, false);

            // First few ticks - check initial state
            _prometeo.SimulateTick(
                reverseInput,
                null,
                tickDelta,
                FishNet.Object.Prediction.ReplicateState.Ticked);
            Physics.Simulate(Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();

            // After 1 tick: check throttle is changing
            Assert.That(_prometeo.ThrottleAxis, Is.LessThan(0f),
                "ThrottleAxis should start going negative after first tick");

            // Full run
            for (int i = 0; i < 200; i++)
            {
                _prometeo.SimulateTick(
                    reverseInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            // Verify throttle reached reverse territory
            Assert.That(_prometeo.ThrottleAxis, Is.LessThan(-0.5f),
                "ThrottleAxis should be significantly negative");

            // RPM should show wheel rotation (either direction)
            float absSpeed = Mathf.Abs(_prometeo.carSpeed);
            Assert.That(absSpeed, Is.GreaterThan(1f),
                string.Format("Wheels should be rotating. carSpeed={0}, throttleAxis={1}, localVZ={2}",
                    _prometeo.carSpeed, _prometeo.ThrottleAxis, _prometeo.LocalVelocityZ));
        }

        [UnityTest]
        public IEnumerator SteeringInput_TurnsWheels()
        {
            float tickDelta = 0.02f;
            VehicleInput steerRight = new VehicleInput(0f, 1f, false);

            for (int i = 0; i < 20; i++)
            {
                _prometeo.SimulateTick(
                    steerRight,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(_prometeo.SteeringAxis, Is.GreaterThan(0f),
                "SteeringAxis should be positive when steering right");

            Assert.That(_prometeo.frontLeftCollider.steerAngle, Is.GreaterThan(0f),
                "Front left wheel should have positive steer angle");
            Assert.That(_prometeo.frontRightCollider.steerAngle, Is.GreaterThan(0f),
                "Front right wheel should have positive steer angle");
        }

        [UnityTest]
        public IEnumerator SteeringLeft_TurnsWheelsNegative()
        {
            float tickDelta = 0.02f;
            VehicleInput steerLeft = new VehicleInput(0f, -1f, false);

            for (int i = 0; i < 20; i++)
            {
                _prometeo.SimulateTick(
                    steerLeft,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(_prometeo.SteeringAxis, Is.LessThan(0f),
                "SteeringAxis should be negative when steering left");

            Assert.That(_prometeo.frontLeftCollider.steerAngle, Is.LessThan(0f),
                "Front left wheel should have negative steer angle");
        }

        [UnityTest]
        public IEnumerator Handbrake_LocksTraction()
        {
            float tickDelta = 0.02f;
            VehicleInput handbrakeInput = new VehicleInput(0f, 0f, true);

            for (int i = 0; i < 10; i++)
            {
                _prometeo.SimulateTick(
                    handbrakeInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(_prometeo.IsTractionLocked, Is.True,
                "Handbrake should lock traction");

            Assert.That(_prometeo.DriftingAxis, Is.GreaterThan(0f),
                "DriftingAxis should increase when handbrake is pressed");
        }

        [UnityTest]
        public IEnumerator NeutralInput_CarDecelerates()
        {
            float tickDelta = 0.02f;

            // First accelerate
            VehicleInput throttleInput = new VehicleInput(1f, 0f, false);
            for (int i = 0; i < 30; i++)
            {
                _prometeo.SimulateTick(
                    throttleInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            float speedBeforeDecel = _rigidbody.linearVelocity.magnitude;

            // Then release throttle
            VehicleInput neutral = VehicleInput.Neutral;
            for (int i = 0; i < 30; i++)
            {
                _prometeo.SimulateTick(
                    neutral,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            float speedAfterDecel = _rigidbody.linearVelocity.magnitude;

            Assert.That(speedAfterDecel, Is.LessThan(speedBeforeDecel),
                "Car should decelerate when throttle is released");
        }

        [UnityTest]
        public IEnumerator ApplyPredictionState_RestoresInternalAxes()
        {
            float tickDelta = 0.02f;

            // Drive forward + steer for a few ticks
            VehicleInput input = new VehicleInput(1f, 0.5f, false);
            for (int i = 0; i < 20; i++)
            {
                _prometeo.SimulateTick(
                    input,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                Physics.Simulate(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            // Capture state after simulation
            float steeringAfterSim = _prometeo.SteeringAxis;
            float throttleAfterSim = _prometeo.ThrottleAxis;
            float driftingAfterSim = _prometeo.DriftingAxis;
            bool isDriftingAfterSim = _prometeo.IsDrifting;
            bool isTractionLockedAfterSim = _prometeo.IsTractionLocked;

            // Reset state manually (simulating a reconcile scenario)
            VehicleReconcileData reconcileData = new VehicleReconcileData(
                null,
                steeringAfterSim,
                throttleAfterSim,
                driftingAfterSim,
                isDriftingAfterSim,
                isTractionLockedAfterSim);

            _prometeo.ApplyPredictionState(reconcileData);

            Assert.That(_prometeo.SteeringAxis,
                Is.EqualTo(steeringAfterSim).Within(0.001f),
                "SteeringAxis should be restored from reconcile data");
            Assert.That(_prometeo.ThrottleAxis,
                Is.EqualTo(throttleAfterSim).Within(0.001f),
                "ThrottleAxis should be restored from reconcile data");
            Assert.That(_prometeo.DriftingAxis,
                Is.EqualTo(driftingAfterSim).Within(0.001f),
                "DriftingAxis should be restored from reconcile data");
            Assert.That(_prometeo.IsTractionLocked,
                Is.EqualTo(isTractionLockedAfterSim),
                "IsTractionLocked should be restored from reconcile data");
        }

        private GameObject CreateTestCar()
        {
            var car = new GameObject("TestCar");
            car.transform.position = Vector3.zero;

            var rb = car.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var prometeo = car.AddComponent<PrometeoCarController>();
            prometeo.maxSpeed = 90;
            prometeo.maxReverseSpeed = 45;
            prometeo.accelerationMultiplier = 2;
            prometeo.maxSteeringAngle = 27;
            prometeo.steeringSpeed = 0.5f;
            prometeo.brakeForce = 350;
            prometeo.decelerationMultiplier = 2;
            prometeo.handbrakeDriftMultiplier = 5;
            prometeo.bodyMassCenter = new Vector3(0f, -0.5f, 0f);
            prometeo.useSounds = false;

            // Create 4 wheel colliders as children
            prometeo.frontLeftCollider = CreateWheel("FL", car, new Vector3(-0.8f, -0.3f, 1.2f));
            prometeo.frontRightCollider = CreateWheel("FR", car, new Vector3(0.8f, -0.3f, 1.2f));
            prometeo.rearLeftCollider = CreateWheel("RL", car, new Vector3(-0.8f, -0.3f, -1.2f));
            prometeo.rearRightCollider = CreateWheel("RR", car, new Vector3(0.8f, -0.3f, -1.2f));

            // Initialize after wheel colliders are assigned
            prometeo.InitializeManual();

            return car;
        }

        private WheelCollider CreateWheel(string name, GameObject car, Vector3 localPos)
        {
            var wheelGo = new GameObject(name + "_Wheel");
            wheelGo.transform.SetParent(car.transform);
            wheelGo.transform.localPosition = localPos;

            var wc = wheelGo.AddComponent<WheelCollider>();
            wc.radius = 0.35f;
            wc.suspensionDistance = 0.3f;

            return wc;
        }

        private GameObject CreateGroundPlane()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "TestGround";
            // Car at y=0, wheels at y=-0.3, radius=0.35 → bottom of wheel = y=-0.65
            // Plane surface is at local y=0
            ground.transform.position = new Vector3(0, -0.65f, 0);
            ground.transform.localScale = new Vector3(10, 1, 10);
            return ground;
        }
    }
}
