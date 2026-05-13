using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Player;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Rapid-fire weapon: spawns pooled projectiles in aim direction at configured fire rate.
    /// </summary>
    public class RapidFireWeapon : WeaponBase
    {
        private PlayerController _playerController;
        private PlayerStats      _playerStats;

        private float _levelDamageBonus;
        private float _levelFireRateBonus;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
            _playerStats      = GetComponentInParent<PlayerStats>();
        }

        protected override void OnLevelUp(int newLevel)
        {
            if (weaponData == null) return;
            _levelDamageBonus   = weaponData.damagePerLevel  * (newLevel - 1);
            _levelFireRateBonus = weaponData.fireRatePerLevel * (newLevel - 1);
        }

        protected override void Fire()
        {
            if (weaponData.projectilePrefab == null)
            {
                Debug.LogWarning("[RapidFireWeapon] No projectile prefab assigned.");
                return;
            }

            var projectilePrefab = weaponData.projectilePrefab.GetComponent<ProjectileBase>();
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[RapidFireWeapon] Projectile prefab missing ProjectileBase component.");
                return;
            }

            // Get a projectile from the pool
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Quaternion spawnRot = firePoint != null ? firePoint.rotation : transform.rotation;

            var projectile = ObjectPoolManager.Instance.Get(projectilePrefab, spawnPos, spawnRot);

            Vector3 fireDir = _playerController != null ? _playerController.AimDirection : transform.forward;

            float damageMult = _playerStats != null ? _playerStats.DamageMultiplier : 1f;
            float critChance = _playerStats != null ? _playerStats.CritChance       : 0f;
            float areaMult   = _playerStats != null ? _playerStats.AreaMultiplier   : 1f;

            // Configure the projectile
            projectile.Initialize(
                weaponData.baseDamage + _levelDamageBonus,
                weaponData.projectileSpeed,
                weaponData.range * areaMult,
                weaponData.piercing,
                fireDir,
                projectilePrefab,
                weaponData.knockbackForce,
                weaponData.knockbackDuration,
                damageMult,
                critChance
            );
        }
    }
}
