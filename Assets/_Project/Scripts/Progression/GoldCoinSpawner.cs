using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Enemies;

namespace SwarmProtocol.Progression
{
    /// <summary>
    /// Listens for enemy kills and spawns GoldCoin pickups based on each enemy's goldDropChance.
    /// Follows the same pattern as XPGemSpawner.
    /// </summary>
    public class GoldCoinSpawner : MonoBehaviour
    {
        [Header("Gold Coin")]
        [SerializeField] private GoldCoin goldCoinPrefab;
        [SerializeField] private int coinValue = 5;

        private void OnEnable()  { EventBus.OnEnemyKilled += HandleEnemyKilled; }
        private void OnDisable() { EventBus.OnEnemyKilled -= HandleEnemyKilled; }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            if (goldCoinPrefab == null || enemy?.EnemyData == null) return;
            if (UnityEngine.Random.Range(0, 100) >= enemy.EnemyData.goldDropChance) return;

            Vector3 spawnPos = enemy.transform.position;
            spawnPos.y = 0.3f;

            var coin = ObjectPoolManager.Instance.Get(goldCoinPrefab, spawnPos, Quaternion.identity);
            coin.Initialize(coinValue, goldCoinPrefab);
        }
    }
}
