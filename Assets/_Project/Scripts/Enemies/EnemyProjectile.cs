using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Player;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Pooled projectile fired by ranged enemies. Travels in a straight line and damages
    /// the player on contact. Mirror of ProjectileBase but targets the Player tag.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class EnemyProjectile : MonoBehaviour, IPoolable
    {
        private float _damage;
        private float _speed;
        private float _maxRange;
        private Vector3 _direction;
        private Vector3 _spawnPosition;
        private EnemyProjectile _prefabRef;
        private Rigidbody _rb;
        private bool _isActive;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void Initialize(float damage, float speed, float maxRange,
                               Vector3 direction, EnemyProjectile prefabRef)
        {
            _damage        = damage;
            _speed         = speed;
            _maxRange      = maxRange;
            _direction     = direction.normalized;
            _prefabRef     = prefabRef;
            _spawnPosition = transform.position;
            _isActive      = true;

            _rb.linearVelocity = _direction * _speed;
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Vector3.Distance(_spawnPosition, transform.position) >= _maxRange)
                ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerHealth>()?.TakeDamage(_damage);
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (!_isActive) return;
            _isActive = false;

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public void OnSpawn()
        {
            _isActive = true;
        }

        public void OnDespawn()
        {
            _isActive = false;
            if (_rb != null)
            {
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
