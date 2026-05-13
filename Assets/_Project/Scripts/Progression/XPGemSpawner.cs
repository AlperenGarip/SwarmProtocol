using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Enemies;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Spawning
{
    /// <summary>
    /// Listens for enemy kill events and spawns XP gems at the enemy's death position.
    /// Keeps XP gem spawning decoupled from enemy logic.
    /// </summary>
    public class XPGemSpawner : MonoBehaviour
    {
        [Header("XP Gem")]
        [SerializeField] private XPGem xpGemPrefab;

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            if (xpGemPrefab == null || enemy == null || enemy.EnemyData == null) return;

            Vector3 spawnPos = enemy.transform.position;
            spawnPos.y = 0.3f; // slightly above ground

            var gem = ObjectPoolManager.Instance.Get(xpGemPrefab, spawnPos, Quaternion.identity);
            gem.Initialize(enemy.EnemyData.xpDropAmount, xpGemPrefab);
        }
    }
}
