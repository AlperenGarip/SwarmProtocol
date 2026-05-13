using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Pickups
{
    /// <summary>
    /// Pooled health orb. Heals the player for a percentage of their max HP on pickup.
    /// Assign a HealthOrbMagnetConfig SO (magnetRange ~3, usePlayerMagnetRange = false).
    /// </summary>
    public class HealthOrb : MagnetCollectible
    {
        [SerializeField] [Range(0f, 1f)] private float healPercent = 0.1f;

        private HealthOrb              _prefabRef;
        private Player.PlayerHealth    _playerHealth;

        public void Initialize(HealthOrb prefabRef)
        {
            _prefabRef = prefabRef;
            Activate(transform.position);
        }

        protected override void OnCollect()
        {
            if (_playerHealth == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _playerHealth = p.GetComponent<Player.PlayerHealth>();
            }

            _playerHealth?.Heal(_playerHealth.MaxHP * healPercent);

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            _playerHealth = null;
        }
    }
}
