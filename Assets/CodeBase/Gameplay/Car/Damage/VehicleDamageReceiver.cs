using System.Collections.Generic;
using CodeBase.CodeBase.Gameplay.Network.Match;
using CodeBase.CodeBase.Gameplay.Network.Statistics;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Car.Damage
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerMatchState))]
    public sealed class VehicleDamageReceiver : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody vehicleRigidbody;
        [SerializeField] private PlayerMatchState playerState;

        [Header("Damage formula")]
        [SerializeField, Min(0f)] private float minimumImpactSpeed = 4f;

        [SerializeField, Min(0f)] private float damagePerSpeed = 6f;
        [SerializeField, Min(0f)] private float pairCooldownSeconds = 0.5f;

        private VehicleDamageHistory _damageHistory;

        private readonly Dictionary<int, float> _nextDamageTimeByAttacker =
            new();

        [Inject]
        private void Construct(VehicleDamageHistory VehicleDamageHistory)
        {
            _damageHistory = VehicleDamageHistory;
        } 
        
        private void Awake()
        {
            if (vehicleRigidbody == null)
                vehicleRigidbody = GetComponent<Rigidbody>();

            if (playerState == null)
                playerState = GetComponent<PlayerMatchState>();
        }

        /// <summary>
        /// Вызывается зоной жертвы при пересечении с зоной атакующего.
        /// Изменение HP разрешено только на сервере.
        /// </summary>
        public void TryReceiveDamageServer(
            VehicleDamageZone victimZone,
            VehicleDamageZone attackerZone)
        {
            if (!IsServerStarted ||
                victimZone == null ||
                attackerZone == null ||
                playerState == null ||
                vehicleRigidbody == null)
            {
                return;
            }

            if (!playerState.ServerIsAlive)
                return;

            VehicleDamageReceiver attacker =
                attackerZone.Owner;

            if (attacker == null || attacker == this)
                return;

            if (attacker.VehicleRigidbody == null)
                return;

            int attackerObjectId =
                attacker.NetworkObject.ObjectId;

            if (_nextDamageTimeByAttacker.TryGetValue(
                    attackerObjectId,
                    out float nextDamageTime) &&
                Time.time < nextDamageTime)
            {
                return;
            }

            Vector3 directionToVictim =
                vehicleRigidbody.worldCenterOfMass -
                attacker.VehicleRigidbody.worldCenterOfMass;

            directionToVictim.y = 0f;

            if (directionToVictim.sqrMagnitude < 0.0001f)
                return;

            directionToVictim.Normalize();

            Vector3 relativeVelocity =
                attacker.VehicleRigidbody.linearVelocity -
                vehicleRigidbody.linearVelocity;

            float impactSpeed = Vector3.Dot(
                relativeVelocity,
                directionToVictim);

            if (impactSpeed < minimumImpactSpeed)
                return;

            int baseDamage = Mathf.CeilToInt(
                (impactSpeed - minimumImpactSpeed) *
                damagePerSpeed);

            if (baseDamage <= 0)
                return;

            float multiplier = victimZone.DamageMultiplier;

            int finalDamage = Mathf.Max(
                1,
                Mathf.CeilToInt(baseDamage * multiplier));

            playerState.ApplyDamageServer(finalDamage);

            bool isCritical =
                victimZone.ZoneType ==
                VehicleDamageZoneType.CriticalDamageReceiver;

            _damageHistory?.Add(
                new VehicleDamageEvent(
                    attackerObjectId,
                    NetworkObject.ObjectId,
                    finalDamage,
                    isCritical,
                    Time.time));

            _nextDamageTimeByAttacker[attackerObjectId] =
                Time.time + pairCooldownSeconds;

            Debug.Log(
                $"[VehicleDamage] {attacker.name} -> {name}; " +
                $"speed={impactSpeed:F2}; " +
                $"damage={finalDamage}; " +
                $"critical={isCritical}.",
                this);
        }

        public Rigidbody VehicleRigidbody => vehicleRigidbody;
    }
}