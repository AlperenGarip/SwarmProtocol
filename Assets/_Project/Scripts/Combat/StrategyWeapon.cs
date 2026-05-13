using UnityEngine;
using SwarmProtocol.Combat.Strategies;
using SwarmProtocol.Player;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Generic WeaponBase implementation that delegates Fire() to a FireStrategySO.
    /// Use this for any new weapon type — create a FireStrategySO subclass + SO asset,
    /// set it on WeaponDataSO.fireStrategy, and reference this prefab from weaponPrefab.
    /// No custom MonoBehaviour needed per weapon type.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class StrategyWeapon : WeaponBase
    {
        private PlayerController _playerController;
        private PlayerStats      _playerStats;
        private float            _levelDamageBonus;

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
            if (weaponData?.fireStrategy == null)
            {
                Debug.LogWarning($"[StrategyWeapon] No FireStrategySO on {weaponData?.weaponName}.");
                return;
            }

            var ctx = new FireContext
            {
                FirePoint                  = firePoint != null ? firePoint : transform,
                AimDirection               = _playerController != null ? _playerController.AimDirection : transform.forward,
                WeaponData                 = weaponData,
                Level                      = CurrentLevel,
                LevelDamageBonus           = _levelDamageBonus,
                OverchargeDamageMultiplier = OverchargeDamageMultiplier,
                DamageMultiplier           = _playerStats != null ? _playerStats.DamageMultiplier : 1f,
                CritChance                 = _playerStats != null ? _playerStats.CritChance       : 0f,
                AreaMultiplier             = _playerStats != null ? _playerStats.AreaMultiplier   : 1f,
            };

            weaponData.fireStrategy.Execute(ctx);
        }
    }
}
