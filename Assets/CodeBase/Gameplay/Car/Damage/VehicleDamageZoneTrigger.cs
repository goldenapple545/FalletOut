using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car.Damage
{
    [RequireComponent(typeof(VehicleDamageZone))]
    public sealed class VehicleDamageZoneTrigger : MonoBehaviour
    {
        private VehicleDamageZone _zone;

        private void Awake()
        {
            _zone = GetComponent<VehicleDamageZone>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_zone == null)
                return;

            VehicleDamageZone otherZone =
                other.GetComponent<VehicleDamageZone>();

            if (otherZone == null)
                return;

            // Рассматриваем только:
            // атакующая front-zone → receiving zone другой машины.
            if (_zone.ZoneType !=
                VehicleDamageZoneType.DamageDealer)
            {
                return;
            }

            if (otherZone.ZoneType ==
                VehicleDamageZoneType.DamageDealer)
            {
                return;
            }

            VehicleDamageReceiver victim =
                otherZone.Owner;

            victim?.TryReceiveDamageServer(
                otherZone,
                _zone);
        }
    }
}