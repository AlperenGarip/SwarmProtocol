using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Generic enemy that delegates all AI to an EnemyBehaviorSO.
    /// New enemy type = EnemyBehaviorSO subclass + SO asset + EnemyDataSO — no new MonoBehaviour.
    /// Contact damage is handled here; movement/attack logic lives in the behavior SO.
    /// </summary>
    public class BehaviorDrivenEnemy : EnemyBase
    {
        private Transform _playerTransform;
        private float     _attackCooldown;

        protected override void Awake()
        {
            base.Awake();
            var p = GameObject.FindWithTag("Player");
            if (p != null) _playerTransform = p.transform;
        }

        public override void Initialize(float difficultyMultiplier, EnemyBase prefabRef)
        {
            base.Initialize(difficultyMultiplier, prefabRef);
            _attackCooldown = 0f;
        }

        protected override void UpdateBehavior()
        {
            if (_playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _playerTransform = p.transform;
            }

            enemyData?.behavior?.Execute(this, _nav, _playerTransform);
        }

        private void OnTriggerEnter(Collider other) => TryContactDamage(other);

        // OnTriggerStay re-applies damage every physics frame while in contact, gated by
        // attackCooldown — so a player who stays glued to a tank takes 1 hit per cooldown.
        private void OnTriggerStay(Collider other) => TryContactDamage(other);

        private void TryContactDamage(Collider other)
        {
            if (IsDead || enemyData == null) return;
            if (_attackCooldown > 0f) return;

            var health = other.GetComponent<Player.PlayerHealth>();
            if (health == null) return;

            health.TakeDamage(enemyData.damage * _difficultyMultiplier);
            _attackCooldown = enemyData.attackCooldown;
        }

        protected override void Update()
        {
            base.Update();
            if (_attackCooldown > 0f)
                _attackCooldown -= Time.deltaTime;
        }
    }
}
