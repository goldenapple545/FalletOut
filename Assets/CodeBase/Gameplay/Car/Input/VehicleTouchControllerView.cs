using System;
using UnityEngine;

namespace CodeBase.Gameplay.Car.Input
{
    public sealed class VehicleTouchControlsView : MonoBehaviour
    {
        [SerializeField] private VehicleTouchButton[] buttons;

        public void Bind(VehicleInputSource inputSource)
        {
            buttons = GetComponentsInChildren<VehicleTouchButton>(true);
            
            foreach (VehicleTouchButton button in buttons)
            {
                if (button != null)
                    button.Bind(inputSource);
            }
        }
    }
}