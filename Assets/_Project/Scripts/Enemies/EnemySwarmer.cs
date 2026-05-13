using UnityEngine;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Swarmer enemy: fast chase, contact damage, low HP.
    /// Behavior is driven by EnemyAI (FSM component).
    /// EnemyBase.Update() owns knockback + freeze; EnemyAI.Update() owns movement/attack
    /// and early-exits when IsMovementBlocked is true.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))]
    public class EnemySwarmer : EnemyBase
    {
        private EnemyAI _ai;

        protected override void Awake()
        {
            base.Awake();  // caches _nav
            _ai = GetComponent<EnemyAI>();
        }

        public override void Initialize(float difficultyMultiplier, EnemyBase prefabRef)
        {
            base.Initialize(difficultyMultiplier, prefabRef);
            // base.Initialize already called _nav.Initialize(). Configure AI on top.
            _ai.Initialize(
                attackRangeOverride:    enemyData.attackRange,
                attackCooldownOverride: enemyData.attackCooldown,
                contactDamageOverride:  enemyData.damage,
                difficultyMultiplier:   difficultyMultiplier
            );
        }

        /// <summary>
        /// EnemyAI runs its own Update(). UpdateBehavior() is not used for swarmer.
        /// This exists to satisfy the abstract contract; future EnemyBehaviorSO migration
        /// will move all logic here.
        /// </summary>
        protected override void UpdateBehavior() { }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            _nav?.Stop();
        }
    }
}
