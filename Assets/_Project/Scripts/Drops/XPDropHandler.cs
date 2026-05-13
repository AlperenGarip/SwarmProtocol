using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.Factories;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Drops
{
    /// <summary>
    /// Orthogonal listener to EnemyKilledEvent. Spawns XP gems at the death position.
    /// Replaces XPGemSpawner — remove XPGemSpawner from the scene when adding this.
    /// </summary>
    public class XPDropHandler : MonoBehaviour
    {
        [SerializeField] private XPGem xpGemPrefab;

        private void OnEnable()  => Event<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        private void OnDisable() => Event<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            if (xpGemPrefab == null || e.EnemyData == null) return;
            if (e.EnemyData.xpDropAmount <= 0) return;

            Vector3 pos = e.DeathPosition;
            pos.y = 0.3f;

            var gem = PickupFactory.Spawn(xpGemPrefab, pos);
            gem?.Initialize(e.EnemyData.xpDropAmount, xpGemPrefab);
        }
    }
}
