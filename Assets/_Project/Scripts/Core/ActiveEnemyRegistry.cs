using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using SwarmProtocol.Enemies;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// Centralized list of all currently alive enemies.
    /// Replaces scattered FindGameObjectsWithTag and Physics.OverlapSphere calls.
    ///
    /// Register on spawn, Unregister in EnemyBase before returning to pool.
    /// GetEnemiesInRange uses SqrMagnitude — no Physics broadphase overhead.
    /// </summary>
    public class ActiveEnemyRegistry : MonoBehaviour
    {
        public static ActiveEnemyRegistry Instance { get; private set; }

        private readonly List<EnemyBase> _activeEnemies = new();

        public IReadOnlyList<EnemyBase> All => _activeEnemies;
        public int Count => _activeEnemies.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Register(EnemyBase enemy)
        {
            if (!_activeEnemies.Contains(enemy))
                _activeEnemies.Add(enemy);
        }

        public void Unregister(EnemyBase enemy)
        {
            _activeEnemies.Remove(enemy);
        }

        /// <summary>
        /// Fills <paramref name="results"/> with all enemies within <paramref name="sqrRange"/>
        /// of <paramref name="center"/>. Uses squared distance — no sqrt, no Physics calls.
        /// </summary>
        private static readonly ProfilerMarker _markerInRange = new("ActiveEnemyRegistry.GetEnemiesInRange");

        public void GetEnemiesInRange(Vector3 center, float sqrRange, List<EnemyBase> results)
        {
            using (_markerInRange.Auto())
            {
                results.Clear();
                for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                {
                    var enemy = _activeEnemies[i];
                    if (enemy == null) { _activeEnemies.RemoveAt(i); continue; }
                    if ((enemy.transform.position - center).sqrMagnitude <= sqrRange)
                        results.Add(enemy);
                }
            }
        }

        /// <summary>
        /// Returns the closest enemy to <paramref name="origin"/>, or null if none alive.
        /// </summary>
        public EnemyBase GetNearest(Vector3 origin)
        {
            EnemyBase nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null) { _activeEnemies.RemoveAt(i); continue; }
                float sqr = (enemy.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; nearest = enemy; }
            }
            return nearest;
        }

        /// <summary>
        /// Freezes all alive enemies for <paramref name="duration"/> seconds.
        /// Used by OrologionPickup.
        /// </summary>
        public void FreezeAll(float duration)
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null)
                    enemy.Freeze(duration);
            }
        }

        /// <summary>Clears registry on scene transition / game restart.</summary>
        public void Clear() => _activeEnemies.Clear();
    }
}
