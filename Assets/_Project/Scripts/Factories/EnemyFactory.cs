using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Factories
{
    /// <summary>
    /// Centralizes enemy creation: pool Get + Initialize in one call.
    /// Used by EnemySpawner, BossSpawnDriver, EliteBurstDriver — no duplicate spawn logic.
    /// </summary>
    public static class EnemyFactory
    {
        public static float GlobalDifficultyBonus { get; set; } = 0f;

        public static Enemies.EnemyBase Spawn(EnemyDataSO data, Vector3 position, float difficultyMultiplier = 1f)
        {
            if (data == null || data.prefab == null) return null;

            var prefabBase = data.prefab.GetComponent<Enemies.EnemyBase>();
            if (prefabBase == null)
            {
                Debug.LogWarning($"[EnemyFactory] {data.prefab.name} is missing an EnemyBase component.");
                return null;
            }

            var enemy = ObjectPoolManager.Instance.Get(prefabBase, position, Quaternion.identity);
            enemy.Initialize(difficultyMultiplier + GlobalDifficultyBonus, prefabBase);
            enemy.GetComponent<Enemies.EnemyNavigation>()?.WarpTo(position);
            return enemy;
        }
    }
}
