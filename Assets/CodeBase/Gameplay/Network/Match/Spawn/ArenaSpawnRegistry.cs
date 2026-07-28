using System;
using UnityEngine;

namespace CodeBase.Gameplay.Spawn
{
    public sealed class ArenaSpawnRegistry : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;

        public int Count => spawnPoints?.Length ?? 0;

        public Transform GetSpawnPoint(int index)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ArenaSpawnRegistry)} has no spawn points.");
            }

            return spawnPoints[index % spawnPoints.Length];
        }
    }
}