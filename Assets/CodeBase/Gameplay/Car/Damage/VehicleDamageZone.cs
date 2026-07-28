using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Car.Damage
{
    [RequireComponent(typeof(Collider))]
    public sealed class VehicleDamageZone : MonoBehaviour
    {
        [SerializeField] private VehicleDamageZoneType zoneType;

        [Header("Receiver only")]
        [SerializeField, Min(1f)] private float damageMultiplier = 1f;

        public VehicleDamageZoneType ZoneType => zoneType;

        public float DamageMultiplier =>
            zoneType == VehicleDamageZoneType.CriticalDamageReceiver
                ? damageMultiplier
                : 1f;

        public VehicleDamageReceiver Owner { get; private set; }

        private void Awake()
        {
            Owner = GetComponentInParent<VehicleDamageReceiver>();

            Collider targetCollider = GetComponent<Collider>();
            targetCollider.isTrigger = true;

            if (Owner == null)
            {
                Debug.LogError(
                    $"{nameof(VehicleDamageZone)} на {name}: " +
                    $"{nameof(VehicleDamageReceiver)} не найден " +
                    "на родительской машине.",
                    this);
            }
        }
    }
}