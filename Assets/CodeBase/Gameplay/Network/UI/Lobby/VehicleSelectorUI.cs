using System.Collections.Generic;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Data;
using CodeBase.Gameplay.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class VehicleSelectorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Dropdown vehicleDropdown;
        [SerializeField] private Image vehiclePreviewImage;
        [SerializeField] private TMP_Text vehicleNameText;
        [SerializeField] private StatBar speedBar;
        [SerializeField] private StatBar driftBar;
        [SerializeField] private StatBar durabilityBar;
        [SerializeField] private StatBar damageBar;

        private LobbySessionService _lobbyService;
        private IStaticDataService _staticDataService;

        [Inject]
        private void Construct(
            LobbySessionService lobbyService,
            IStaticDataService staticDataService)
        {
            _lobbyService = lobbyService;
            _staticDataService = staticDataService;
        }

        public void Start()
        {
            var vehicles = _staticDataService.VehiclesRegistry.Vehicles;

            vehicleDropdown.ClearOptions();

            foreach (var vehicle in vehicles)
            {
                vehicleDropdown.options.Add(new TMP_Dropdown.OptionData(vehicle.DisplayName));
            }

            int defaultIndex = vehicles.IndexOf(_staticDataService.VehiclesRegistry.Vehicles[0]);
            if (defaultIndex < 0) defaultIndex = 0;

            vehicleDropdown.value = defaultIndex;
            vehicleDropdown.RefreshShownValue();
            vehicleDropdown.onValueChanged.AddListener(OnVehicleSelected);
            OnVehicleSelected(defaultIndex);

            UpdatePreview(_staticDataService.VehiclesRegistry.Vehicles[0]);
        }

        private void OnDestroy()
        {
            if (vehicleDropdown != null)
                vehicleDropdown.onValueChanged.RemoveListener(OnVehicleSelected);
        }

        private void OnVehicleSelected(int index)
        {
            // if (_staticDataService.VehiclesRegistry == null || index < 0 || index >= _staticDataService.VehiclesRegistry.Vehicles.Count)
            //     return;

            var vehicle = _staticDataService.VehiclesRegistry.Vehicles[index];
            _lobbyService.SetSelectedVehicle(vehicle);
            UpdatePreview(vehicle);
        }

        private void UpdatePreview(VehicleConfig vehicle)
        {
            if (vehiclePreviewImage != null)
            {
                vehiclePreviewImage.sprite = vehicle.PreviewImage;
                vehiclePreviewImage.enabled = vehicle.PreviewImage != null;
            }

            if (vehicleNameText != null)
                vehicleNameText.text = vehicle.DisplayName;

            if (speedBar != null)
                speedBar.SetValue(vehicle.Speed);

            if (driftBar != null)
                driftBar.SetValue(vehicle.Drift);

            if (durabilityBar != null)
                durabilityBar.SetValue(vehicle.Durability);

            if (damageBar != null)
                damageBar.SetValue(vehicle.Damage);
        }
    }
}
