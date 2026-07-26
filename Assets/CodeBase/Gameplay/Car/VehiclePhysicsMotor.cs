using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Car.Input.Prediction;
using FishNet.Component.Prediction;
using FishNet.Object.Prediction;
using UnityEngine;

namespace CodeBase.Gameplay.Car
{
    public sealed class VehiclePhysicsMotor : MonoBehaviour
    {
        [SerializeField] private PrometeoCarController prometeo;
        
        public PrometeoCarController Prometeo => prometeo;

        public void Simulate(
            VehicleInput input,
            PredictionRigidbody predictionRigidbody,
            float tickDelta,
            ReplicateState state)
        {
            if (prometeo == null)
                return;

            prometeo.SimulateTick(
                input,
                predictionRigidbody,
                tickDelta,
                state);
        }

        public void ApplyReconcile(VehicleReconcileData data)
        {
            if (prometeo == null)
                return;

            prometeo.ApplyPredictionState(data);
        }
    }
}