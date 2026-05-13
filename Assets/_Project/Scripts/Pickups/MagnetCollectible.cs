using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Pickups
{
    /// <summary>
    /// Abstract base for all magnet-attracted pickups.
    /// Handles player detection, magnet movement, idle bobbing, and trigger pickup.
    /// Subclasses implement OnCollect() for their specific reward logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class MagnetCollectible : MonoBehaviour, IPoolable
    {
        [SerializeField] protected MagnetConfigSO config;

        protected bool IsActive { get; private set; }

        private bool             _isMagneted;
        private Vector3          _basePosition;
        private Transform        _playerTransform;
        private Player.PlayerStats _playerStats;

        // ─── Subclass API ─────────────────────────────────────────

        /// <summary>Call in subclass Initialize() after setting own fields.</summary>
        protected void Activate(Vector3 spawnPosition)
        {
            _basePosition    = spawnPosition;
            IsActive         = true;
            _isMagneted      = false;
        }

        /// <summary>Called once with IsActive already false — apply reward and return to pool.</summary>
        protected abstract void OnCollect();

        /// <summary>Instantly starts magnet attraction toward the player. Used by VacuumPickup.</summary>
        public void ForceAttract() => _isMagneted = true;

        // ─── Movement ─────────────────────────────────────────────

        protected virtual void Update()
        {
            if (!IsActive || config == null) return;

            if (_playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null) return;
                _playerTransform = p.transform;
                _playerStats     = p.GetComponent<Player.PlayerStats>();
            }

            float dist  = Vector3.Distance(transform.position, _playerTransform.position);
            float range = config.usePlayerMagnetRange && _playerStats != null
                ? _playerStats.MagnetRange
                : config.magnetRange;

            if (dist <= range || _isMagneted)
            {
                _isMagneted = true;
                transform.position = Vector3.MoveTowards(
                    transform.position, _playerTransform.position, config.magnetSpeed * Time.deltaTime);

                if (dist <= config.pickupDistance)
                    Collect();
            }
            else
            {
                // Remap sin from [-1,1] to [0,1] so the pickup only bobs UP from its spawn,
                // never sinks below — prevents clipping into the ground at the trough.
                float bob = (Mathf.Sin(Time.time * config.bobSpeed) * 0.5f + 0.5f) * config.bobHeight;
                transform.position = new Vector3(
                    _basePosition.x,
                    _basePosition.y + bob,
                    _basePosition.z);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive || !other.CompareTag("Player")) return;
            Collect();
        }

        private void Collect()
        {
            if (!IsActive) return;
            IsActive    = false;
            _isMagneted = false;
            OnCollect();
        }

        // ─── IPoolable ────────────────────────────────────────────

        public virtual void OnSpawn()
        {
            IsActive    = true;
            _isMagneted = false;
        }

        public virtual void OnDespawn()
        {
            IsActive    = false;
            _isMagneted = false;
        }
    }
}
