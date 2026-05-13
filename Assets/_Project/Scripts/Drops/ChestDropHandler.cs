using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.Factories;
using SwarmProtocol.Progression;
using SwarmProtocol.Stage;

namespace SwarmProtocol.Drops
{
    /// <summary>
    /// Orthogonal listener to EnemyKilledEvent. Spawns a TreasureChest when an elite or boss dies.
    /// EnemyBase.Die() never touches chest logic — this handler subscribes independently.
    /// </summary>
    public class ChestDropHandler : MonoBehaviour
    {
        [SerializeField] private TreasureChest chestPrefab;
        [SerializeField] private StageTimer    stageTimer;

        private void OnEnable()  => Event<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        private void OnDisable() => Event<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            if (chestPrefab == null || e.EnemyData == null) return;
            if (!e.EnemyData.isElite && !e.EnemyData.isBoss) return;

            float elapsed = stageTimer != null ? stageTimer.ElapsedTime : 0f;
            var chest = PickupFactory.Spawn(chestPrefab, e.DeathPosition);
            chest?.Initialize(chestPrefab, elapsed);
        }
    }
}
