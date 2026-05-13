using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.Factories;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Drops
{
    /// <summary>
    /// Orthogonal listener to EnemyKilledEvent. Spawns gold coins based on EnemyDataSO.goldDropChance.
    /// Replaces GoldCoinSpawner — remove GoldCoinSpawner from the scene when adding this.
    /// </summary>
    public class GoldDropHandler : MonoBehaviour
    {
        [SerializeField] private GoldCoin goldCoinPrefab;
        [SerializeField] private int baseGoldValue      = 5;
        [SerializeField] private int eliteGoldMultiplier = 3;
        [SerializeField] private int bossGoldMultiplier  = 10;

        private void OnEnable()  => Event<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        private void OnDisable() => Event<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            if (goldCoinPrefab == null || e.EnemyData == null) return;
            if (Random.Range(0, 100) >= e.EnemyData.goldDropChance) return;

            int value = baseGoldValue;
            if      (e.EnemyData.isBoss)  value *= bossGoldMultiplier;
            else if (e.EnemyData.isElite) value *= eliteGoldMultiplier;

            Vector3 pos = e.DeathPosition;
            pos.y = 0.3f;

            var coin = PickupFactory.Spawn(goldCoinPrefab, pos);
            coin?.Initialize(value, goldCoinPrefab);
        }
    }
}
