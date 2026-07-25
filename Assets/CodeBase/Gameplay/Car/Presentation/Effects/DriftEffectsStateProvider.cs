using UnityEngine;

namespace CodeBase.Gameplay.Car.Presentation.Effects
{
    public sealed class DriftEffectsStateProvider : MonoBehaviour
    {
        [SerializeField] private PrometeoCarController carController;

        public bool IsDrifting { get; private set; }
        public bool AreSkidMarksActive { get; private set; }

        public void Refresh()
        {
            if (carController == null)
            {
                IsDrifting = false;
                AreSkidMarksActive = false;
                return;
            }

            IsDrifting = carController.isDrifting;

            AreSkidMarksActive =
                (carController.isTractionLocked ||
                 Mathf.Abs(carController.LocalVelocityX) > 5f) &&
                Mathf.Abs(carController.carSpeed) > 12f;
        }
    }
}