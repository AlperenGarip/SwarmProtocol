using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.Factories;
using SwarmProtocol.Pickups;

namespace SwarmProtocol.Drops
{
    /// <summary>
    /// Orthogonal listener to EnemyKilledEvent. Spawns health orbs based on EnemyDataSO.healthOrbDropChance.
    /// </summary>
    public class HealthOrbDropHandler : MonoBehaviour
    {
        [SerializeField] private HealthOrb healthOrbPrefab;

        private void OnEnable()  => Event<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        private void OnDisable() => Event<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            if (healthOrbPrefab == null || e.EnemyData == null) return;
            if (e.EnemyData.healthOrbDropChance <= 0) return;
            if (Random.Range(0, 100) >= e.EnemyData.healthOrbDropChance) return;

            Vector3 pos = e.DeathPosition;
            pos.y = 0.3f;

            var orb = PickupFactory.Spawn(healthOrbPrefab, pos);
            orb?.Initialize(healthOrbPrefab);
        }
    }
}
