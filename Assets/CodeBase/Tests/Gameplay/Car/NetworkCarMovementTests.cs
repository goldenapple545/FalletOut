using CodeBase.Gameplay.Car;
using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Network;
using CodeBase.Infrastructure.Services.Session;
using FishNet.Managing;
using FishNet.Object;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace CodeBase.Tests.Gameplay.Car
{
    [TestFixture]
    public class NetworkCarMovementTests
    {
        private NetworkRuntimeRoot _networkRoot;
        private SessionService _sessionService;
        private bool _clientAuthenticated;

        [SetUp]
        public void SetUp()
        {
            _clientAuthenticated = false;

            var prefab = Resources.Load<NetworkRuntimeRoot>("NetworkRoot");
            Assert.That(prefab, Is.Not.Null,
                "NetworkRoot prefab must exist in Resources folder");

            _networkRoot = Object.Instantiate(prefab);
            Assert.That(_networkRoot.NetworkManager, Is.Not.Null,
                "NetworkManager must be set on NetworkRuntimeRoot prefab");

            _sessionService = new SessionService(_networkRoot);
            _sessionService.Initialize();

            _sessionService.ClientAuthenticated += OnClientAuthenticated;
        }

        [TearDown]
        public void TearDown()
        {
            if (_sessionService != null)
            {
                _sessionService.ClientAuthenticated -= OnClientAuthenticated;
                _sessionService.Stop();
                _sessionService.Dispose();
            }

            if (_networkRoot != null)
                Object.DestroyImmediate(_networkRoot.gameObject);
        }

        [UnityTest]
        public IEnumerator StartHost_BecomesHost()
        {
            _sessionService.StartHost();

            // Wait up to 3 seconds for host to authenticate
            float timeout = 3f;
            while (!_clientAuthenticated && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.That(_sessionService.IsHostStarted, Is.True,
                "Host should be started after StartHost and authentication");
            Assert.That(_sessionService.IsServerStarted, Is.True,
                "Server should be running when host is started");
            Assert.That(_sessionService.IsClientStarted, Is.True,
                "Client should be connected when host is started");
        }

        [UnityTest]
        public IEnumerator HostVehicle_RespondsToThrottleInput()
        {
            _sessionService.StartHost();

            yield return WaitForHostAuthenticated(3f);

            Assert.That(_sessionService.IsHostStarted, Is.True,
                "Must be host to test vehicle movement");

            // Spawn a test vehicle for the host client
            var vehiclePrefab = Resources.Load<NetworkObject>("NetworkRoot");
            GameObject testVehicle = CreateTestVehicleOnHost(
                _networkRoot.NetworkManager);

            yield return new WaitForSeconds(0.1f);

            // Find the PrometeoCarController on spawned vehicle
            var prometeo = testVehicle.GetComponent<PrometeoCarController>();
            var motor = testVehicle.GetComponent<VehiclePhysicsMotor>();
            var rb = testVehicle.GetComponent<Rigidbody>();

            Assert.That(prometeo, Is.Not.Null,
                "Spawned vehicle must have PrometeoCarController");

            // Simulate ticks with throttle input directly through Prometeo
            float tickDelta = (float)_networkRoot.NetworkManager.TimeManager.TickDelta;
            VehicleInput throttleInput = new VehicleInput(1f, 0f, false);

            for (int i = 0; i < 50; i++)
            {
                prometeo.SimulateTick(
                    throttleInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                yield return new WaitForSeconds(tickDelta);
            }

            Assert.That(prometeo.ThrottleAxis, Is.GreaterThan(0f),
                "ThrottleAxis should be positive after throttle input on host vehicle");
            Assert.That(prometeo.carSpeed, Is.GreaterThan(0f),
                "Host vehicle should have forward speed after throttle");

            Object.DestroyImmediate(testVehicle);
        }

        [UnityTest]
        public IEnumerator HostVehicle_SteeringChangesWheelAngle()
        {
            _sessionService.StartHost();

            yield return WaitForHostAuthenticated(3f);

            GameObject testVehicle = CreateTestVehicleOnHost(
                _networkRoot.NetworkManager);
            var prometeo = testVehicle.GetComponent<PrometeoCarController>();

            float tickDelta = (float)_networkRoot.NetworkManager.TimeManager.TickDelta;
            VehicleInput steerInput = new VehicleInput(0f, 1f, false);

            for (int i = 0; i < 20; i++)
            {
                prometeo.SimulateTick(
                    steerInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                yield return new WaitForSeconds(tickDelta);
            }

            Assert.That(prometeo.SteeringAxis, Is.GreaterThan(0f),
                "SteeringAxis should be positive after steering right on host vehicle");

            Assert.That(prometeo.frontLeftCollider.steerAngle,
                Is.GreaterThan(0f),
                "Front wheel steer angle should be positive");

            Object.DestroyImmediate(testVehicle);
        }

        [UnityTest]
        public IEnumerator HostVehicle_HandbrakeLocksTraction()
        {
            _sessionService.StartHost();

            yield return WaitForHostAuthenticated(3f);

            GameObject testVehicle = CreateTestVehicleOnHost(
                _networkRoot.NetworkManager);
            var prometeo = testVehicle.GetComponent<PrometeoCarController>();

            float tickDelta = (float)_networkRoot.NetworkManager.TimeManager.TickDelta;
            VehicleInput handbrakeInput = new VehicleInput(0f, 0f, true);

            for (int i = 0; i < 10; i++)
            {
                prometeo.SimulateTick(
                    handbrakeInput,
                    null,
                    tickDelta,
                    FishNet.Object.Prediction.ReplicateState.Ticked);

                yield return new WaitForSeconds(tickDelta);
            }

            Assert.That(prometeo.IsTractionLocked, Is.True,
                "Handbrake should lock traction on host vehicle");
            Assert.That(prometeo.DriftingAxis, Is.GreaterThan(0f),
                "DriftingAxis should increase when handbrake is pressed");

            Object.DestroyImmediate(testVehicle);
        }

        private void OnClientAuthenticated()
        {
            _clientAuthenticated = true;
        }

        private IEnumerator WaitForHostAuthenticated(float maxWait)
        {
            float remaining = maxWait;
            while (!_clientAuthenticated && remaining > 0f)
            {
                remaining -= Time.deltaTime;
                yield return null;
            }
        }

        private GameObject CreateTestVehicleOnHost(NetworkManager networkManager)
        {
            var vehicleGo = new GameObject("HostTestVehicle");
            vehicleGo.transform.position = Vector3.zero;

            var rb = vehicleGo.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var prometeo = vehicleGo.AddComponent<PrometeoCarController>();
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

            prometeo.frontLeftCollider = CreateWheel("FL", vehicleGo, new Vector3(-0.8f, -0.3f, 1.2f));
            prometeo.frontRightCollider = CreateWheel("FR", vehicleGo, new Vector3(0.8f, -0.3f, 1.2f));
            prometeo.rearLeftCollider = CreateWheel("RL", vehicleGo, new Vector3(-0.8f, -0.3f, -1.2f));
            prometeo.rearRightCollider = CreateWheel("RR", vehicleGo, new Vector3(0.8f, -0.3f, -1.2f));

            prometeo.InitializeManual();

            var motor = vehicleGo.AddComponent<VehiclePhysicsMotor>();

            // Use SerializedField setting via reflection is fragile,
            // so we add the component and set the reference via code
            var motorType = typeof(VehiclePhysicsMotor);
            var prometeoField = motorType.GetField("prometeo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prometeoField != null)
                prometeoField.SetValue(motor, prometeo);

            return vehicleGo;
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
    }
}
