namespace CodeBase.CodeBase.Gameplay.Network.Statistics
{
    public readonly struct VehicleDamageEvent
    {
        public readonly int AttackerObjectId;
        public readonly int VictimObjectId;
        public readonly int Damage;
        public readonly bool IsCritical;
        public readonly float ServerTime;

        public VehicleDamageEvent(
            int attackerObjectId,
            int victimObjectId,
            int damage,
            bool isCritical,
            float serverTime)
        {
            AttackerObjectId = attackerObjectId;
            VictimObjectId = victimObjectId;
            Damage = damage;
            IsCritical = isCritical;
            ServerTime = serverTime;
        }
    }
}