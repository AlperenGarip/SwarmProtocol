using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Ranged enemy: chases until within attackRange, then stops and fires projectiles.
    /// Self-contained FSM inside UpdateBehavior() — no separate EnemyAI component needed.
    /// Knockback and freeze are handled by EnemyBase.Update() before UpdateBehavior() is called.
    /// </summary>
    public class EnemyRanged : EnemyBase
    {
        [Header("Ranged Attack")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float projectileRange = 20f;

        private Transform _playerTransform;
        private float _attackTimer;
        private bool _isActive;

        private void OnEnable()  { EventBus.OnGameStateChanged += HandleGameStateChanged; }
        private void OnDisable() { EventBus.OnGameStateChanged -= HandleGameStateChanged; }

        private void HandleGameStateChanged(GameState state)
        {
            _isActive = state == GameState.Playing;
            if (!_isActive) _nav?.Stop();
        }

        protected override void Awake()
        {
            base.Awake();  // caches _nav
        }

        public override void Initialize(float difficultyMultiplier, EnemyBase prefabRef)
        {
            base.Initialize(difficultyMultiplier, prefabRef);
            // base.Initialize already calls _nav.Initialize()
            _playerTransform = null;
            _attackTimer = 0f;
            _isActive = GameManager.Instance?.CurrentState == GameState.Playing;
        }

        protected override void UpdateBehavior()
        {
            if (!_isActive) return;

            if (_playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _playerTransform = p.transform;
                else return;
            }

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            _attackTimer -= Time.deltaTime;

            if (dist <= enemyData.attackRange)
            {
                _nav.Stop();

                // Face the player (horizontal only)
                Vector3 lookDir = _playerTransform.position - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                if (_attackTimer <= 0f)
                {
                    FireProjectile();
                    _attackTimer = enemyData.attackCooldown;
                }
            }
            else
            {
                _nav.MoveTo(_playerTransform.position);
            }
        }

        private void FireProjectile()
        {
            if (projectilePrefab == null || _playerTransform == null) return;

            Vector3 spawnPos = firePoint != null
                ? firePoint.position
                : transform.position + Vector3.up * 0.5f;

            Vector3 dir = _playerTransform.position - spawnPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            dir.Normalize();

            var proj = ObjectPoolManager.Instance.Get(projectilePrefab, spawnPos, Quaternion.identity);
            proj.Initialize(
                damage:    enemyData.damage * _difficultyMultiplier,
                speed:     projectileSpeed,
                maxRange:  projectileRange,
                direction: dir,
                prefabRef: projectilePrefab);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            _playerTransform = null;
            _isActive = false;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            _nav?.Stop();
        }
    }
}
