using System.Collections.Generic;
using CodeBase.CodeBase.Gameplay.Car.Damage;
using R3;

namespace CodeBase.CodeBase.Gameplay.Network.Statistics
{
    public interface IVehicleDamageHistory
    {
        ReadOnlyReactiveProperty<VehicleDamageEvent> LastEvent { get; }

        IReadOnlyCollection<VehicleDamageEvent> Events { get; }

        void Add(VehicleDamageEvent damageEvent);

        void Clear();
    }
}