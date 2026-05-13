using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Enemies;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Pooled projectile that moves forward, detects hits with enemies, applies damage,
    /// and returns to pool after exceeding lifetime/range.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ProjectileBase : MonoBehaviour, IPoolable
    {
        private float _damage;
        private float _damageMultiplier;
        private float _critChance;
        private float _speed;
        private float _maxRange;
        private bool  _piercing;
        private float _knockbackForce;
        private float _knockbackDuration;
        private Vector3 _direction;
        private Vector3 _spawnPosition;
        private ProjectileBase _prefabRef;
        private Rigidbody _rb;
        private bool _isActive;
        private bool _destroyOnFinish; // benchmark: true = Destroy() at end of life instead of returning to pool

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        /// <summary>
        /// Configures the projectile after being spawned from the pool.
        /// </summary>
        public void Initialize(float damage, float speed, float maxRange, bool piercing,
                               Vector3 direction, ProjectileBase prefabRef,
                               float knockbackForce = 0f, float knockbackDuration = 0f,
                               float damageMultiplier = 1f, float critChance = 0f,
                               bool destroyOnFinish = false)
        {
            _damage            = damage;
            _damageMultiplier  = damageMultiplier;
            _critChance        = critChance;
            _speed             = speed;
            _maxRange          = maxRange;
            _piercing          = piercing;
            _knockbackForce    = knockbackForce;
            _knockbackDuration = knockbackDuration;
            _direction         = direction.normalized;
            _prefabRef         = prefabRef;
            _spawnPosition     = transform.position;
            _isActive          = true;
            _destroyOnFinish   = destroyOnFinish;

            _rb.linearVelocity = _direction * _speed;
        }

        private void Update()
        {
            if (!_isActive) return;

            // Check if projectile has exceeded its max range
            float distanceTraveled = Vector3.Distance(_spawnPosition, transform.position);
            if (distanceTraveled >= _maxRange)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            // Check if we hit an enemy
            var enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // Pass spawn position as knockback source so enemy flies away from origin
                DamageSystem.ApplyDamage(enemy, _damage,
                    damageMultiplier: _damageMultiplier,
                    critChance:       _critChance,
                    sourcePosition:   transform.position,
                    knockbackForce:   _knockbackForce,
                    knockbackDuration: _knockbackDuration);

                if (!_piercing)
                    ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (!_isActive) return;
            _isActive = false;

            if (_destroyOnFinish)
            {
                // Benchmark mode: deliberately destroy + GC so we can compare against pooling
                Destroy(gameObject);
                return;
            }

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // ─── IPoolable ──────────────────────────────────────────

        public void OnSpawn()
        {
            _isActive = true;
        }

        public void OnDespawn()
        {
            _isActive = false;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
