using UnityEngine;
using SwarmProtocol.Enemies;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Melee weapon: on each swing damages all enemies within a sphere radius around the player.
    /// Configure reach via WeaponDataSO.range. Set enemyLayerMask to the Enemy layer.
    /// </summary>
    public class MeleeWeapon : WeaponBase
    {
        [SerializeField] private LayerMask enemyLayerMask;

        private SwarmProtocol.Player.PlayerStats _playerStats;
        private float _levelDamageBonus;
        private readonly Collider[] _hitBuffer = new Collider[32];

        private void Awake()
        {
            _playerStats = GetComponentInParent<SwarmProtocol.Player.PlayerStats>();
        }

        protected override void OnLevelUp(int newLevel)
        {
            if (weaponData != null)
                _levelDamageBonus = weaponData.damagePerLevel * (newLevel - 1);
        }

        protected override void Fire()
        {
            if (weaponData == null) return;

            float damageMult = _playerStats != null ? _playerStats.DamageMultiplier : 1f;
            float critChance = _playerStats != null ? _playerStats.CritChance       : 0f;
            float areaMult   = _playerStats != null ? _playerStats.AreaMultiplier   : 1f;

            float damage = weaponData.baseDamage + _levelDamageBonus;
            float radius = weaponData.range * areaMult;
            Vector3 source = transform.position;
            int count = Physics.OverlapSphereNonAlloc(source, radius,
                                                      _hitBuffer, enemyLayerMask);
            for (int i = 0; i < count; i++)
            {
                var enemy = _hitBuffer[i].GetComponent<EnemyBase>();
                if (enemy != null)
                    DamageSystem.ApplyDamage(enemy, damage,
                        damageMultiplier: damageMult,
                        critChance:       critChance,
                        sourcePosition:   source,
                        knockbackForce:   weaponData.knockbackForce,
                        knockbackDuration: weaponData.knockbackDuration);
            }
        }
    }
}
