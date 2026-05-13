using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Elite enemy: inherits swarmer behaviour but drops a TreasureChest on death.
    /// Boost stats via EnemyDataSO (higher HP/damage/speed) rather than code.
    /// </summary>
    public class EnemyElite : EnemySwarmer
    {
        [Header("Elite Drop")]
        [SerializeField] private TreasureChest chestPrefab;

        protected override void Die()
        {
            // Spawn chest at death position before base returns this object to pool
            if (chestPrefab != null && ObjectPoolManager.Instance != null)
            {
                var chest = ObjectPoolManager.Instance.Get(chestPrefab, transform.position, Quaternion.identity);
                chest.Initialize(chestPrefab);
            }

            base.Die();
        }
    }
}
