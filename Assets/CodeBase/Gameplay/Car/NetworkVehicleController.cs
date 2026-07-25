using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Car.Presentation;
using FishNet.Object;
using UnityEngine;

namespace CodeBase.Gameplay.Car
{
    public sealed class NetworkVehicleController : NetworkBehaviour
    {
        [SerializeField] private VehicleInputSource inputSource;
        [SerializeField] private VehiclePhysicsMotor physicsMotor;

        private VehicleInput _latestInput;

        private void Update()
        {
            if (!IsOwner)
                return;

            _latestInput = inputSource.Read();
            SendInputToServer(_latestInput);
        }

        [ServerRpc]
        private void SendInputToServer(VehicleInput input)
        {
            _latestInput = input;
        }

        private void FixedUpdate()
        {
            if (!IsServerStarted)
                return;

            SimulateVehicle(_latestInput);
        }

        private void SimulateVehicle(VehicleInput LatestInput)
        {
            physicsMotor.Simulate(LatestInput, Time.fixedDeltaTime);
        }
    }
}