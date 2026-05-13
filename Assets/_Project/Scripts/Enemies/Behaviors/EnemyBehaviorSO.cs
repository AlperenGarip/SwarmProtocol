using UnityEngine;

namespace SwarmProtocol.Enemies.Behaviors
{
    /// <summary>
    /// Abstract base for enemy AI behaviors (Strategy-as-SO pattern).
    /// Subclass + create a SO asset; reference it from EnemyDataSO.behavior.
    /// New enemy behavior = new subclass + SO asset + EnemyDataSO — no new MonoBehaviour.
    /// </summary>
    public abstract class EnemyBehaviorSO : ScriptableObject
    {
        /// <summary>
        /// Called every frame by BehaviorDrivenEnemy.UpdateBehavior().
        /// Implement chase, attack, or any other per-frame AI logic here.
        /// </summary>
        public abstract void Execute(EnemyBase enemy, EnemyNavigation nav, Transform playerTransform);
    }
}
