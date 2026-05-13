using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Player;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Shotgun weapon: fires multiple projectiles in a random spread pattern per shot.
    /// Configure pellet count via WeaponDataSO.projectilesPerShot and spread via spreadAngle.
    /// </summary>
    public class ShotgunWeapon : WeaponBase
    {
        private PlayerController _playerController;
        private PlayerStats      _playerStats;
        private float _levelDamageBonus;

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
            _playerStats      = GetComponentInParent<PlayerStats>();
        }

        protected override void OnLevelUp(int newLevel)
        {
            if (weaponData != null)
                _levelDamageBonus = weaponData.damagePerLevel * (newLevel - 1);
        }

        protected override void Fire()
        {
            if (weaponData?.projectilePrefab == null) return;

            var projectilePrefab = weaponData.projectilePrefab.GetComponent<ProjectileBase>();
            if (projectilePrefab == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector3 baseDir  = _playerController != null
                ? _playerController.AimDirection
                : transform.forward;

            float halfSpread = weaponData.spreadAngle * 0.5f;
            int   pellets    = Mathf.Max(1, weaponData.projectilesPerShot);

            float damageMult = _playerStats != null ? _playerStats.DamageMultiplier : 1f;
            float critChance = _playerStats != null ? _playerStats.CritChance       : 0f;
            float areaMult   = _playerStats != null ? _playerStats.AreaMultiplier   : 1f;

            for (int i = 0; i < pellets; i++)
            {
                float angle = UnityEngine.Random.Range(-halfSpread, halfSpread);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;

                var proj = ObjectPoolManager.Instance.Get(projectilePrefab, spawnPos, Quaternion.identity);
                proj.Initialize(
                    weaponData.baseDamage + _levelDamageBonus,
                    weaponData.projectileSpeed,
                    weaponData.range * areaMult,
                    weaponData.piercing,
                    dir,
                    projectilePrefab,
                    weaponData.knockbackForce,
                    weaponData.knockbackDuration,
                    damageMult,
                    critChance);
            }
        }
    }
}
