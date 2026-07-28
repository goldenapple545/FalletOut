using System;
using UnityEngine;

namespace CodeBase.CodeBase.Gameplay.Network.Match.Spawn
{
    public sealed class VehicleColorRegistry : MonoBehaviour
    {
        [SerializeField] private VehicleColorEntry[] colors;

        public int Count => colors?.Length ?? 0;

        public VehicleColorEntry GetEntry(int index)
        {
            if (colors == null || colors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(VehicleColorRegistry)} has no colors.");
            }

            return colors[index % colors.Length];
        }
    }
}