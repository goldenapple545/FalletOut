using System;
using System.Collections.Generic;
using R3;

namespace CodeBase.CodeBase.Gameplay.Network.Statistics
{
    public sealed class VehicleDamageHistory : IVehicleDamageHistory,
        IDisposable
    {
        private readonly int _capacity;
        private readonly Queue<VehicleDamageEvent> _events = new();

        private readonly ReactiveProperty<VehicleDamageEvent> _lastEvent =
            new();

        public ReadOnlyReactiveProperty<VehicleDamageEvent> LastEvent =>
            _lastEvent;

        public IReadOnlyCollection<VehicleDamageEvent> Events =>
            _events;

        public VehicleDamageHistory(int capacity)
        {
            _capacity = Math.Max(1, capacity);
        }

        public void Add(VehicleDamageEvent damageEvent)
        {
            while (_events.Count >= _capacity)
                _events.Dequeue();

            _events.Enqueue(damageEvent);

            // R3: оповещаем локальных подписчиков сцены,
            // например damage feed, дебаг-панель или статистику.
            _lastEvent.Value = damageEvent;
        }

        public void Clear()
        {
            _events.Clear();
        }

        public void Dispose()
        {
            _events.Clear();
            _lastEvent.Dispose();
        }
    }
}