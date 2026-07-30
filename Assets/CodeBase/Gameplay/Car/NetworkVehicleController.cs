using CodeBase.Gameplay.Car;
using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Car.Input.Prediction;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car
{
    public sealed class NetworkVehicleController : NetworkBehaviour
    {
        [SerializeField] private VehicleInputSource inputSource;
        [SerializeField] private VehiclePhysicsMotor physicsMotor;
        [SerializeField] private Rigidbody vehicleRigidbody;

        private PredictionRigidbody _predictionRigidbody;

        private void Awake()
        {
            if (vehicleRigidbody == null)
                vehicleRigidbody = GetComponent<Rigidbody>();

            if (vehicleRigidbody == null)
            {
                Debug.LogError(
                    "[NetworkVehicleController] Rigidbody is missing.",
                    this);

                enabled = false;
                return;
            }

            _predictionRigidbody = new PredictionRigidbody();
            _predictionRigidbody.Initialize(vehicleRigidbody);
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= OnTick;
            TimeManager.OnPostTick -= OnPostTick;

            base.OnStopNetwork();
        }

        private void OnTick()
        {
            VehicleReplicateData data = default;

            if (IsOwner)
            {
                VehicleInput input = inputSource.Read();

                data = new VehicleReplicateData(
                    input.Throttle,
                    input.Steering,
                    input.Handbrake,
                    input.Boost);
            }

            RunInputs(data);
        }

        private void OnPostTick()
        {
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            if (!IsServerStarted)
                return;

            PrometeoCarController prometeo = physicsMotor.Prometeo;

            VehicleReconcileData data = new(
                _predictionRigidbody,
                prometeo.SteeringAxis,
                prometeo.ThrottleAxis,
                prometeo.DriftingAxis,
                prometeo.IsDrifting,
                prometeo.IsTractionLocked);

            ReconcileState(data);
        }

        [Replicate]
        private void RunInputs(
            VehicleReplicateData data,
            ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            physicsMotor.Simulate(
                new VehicleInput(
                    data.Throttle,
                    data.Steering,
                    data.Handbrake,
                    data.Boost),
                _predictionRigidbody,
                (float)TimeManager.TickDelta,
                state);
        }

        [Reconcile]
        private void ReconcileState(
            VehicleReconcileData data,
            Channel channel = Channel.Unreliable)
        {
            _predictionRigidbody.Reconcile(data.Rigidbody);
            physicsMotor.ApplyReconcile(data);
        }
    }
}