using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Factories;
using SwarmProtocol.Player;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Manages up to 6 simultaneous weapons, all auto-firing on their own cooldowns.
    /// Picking a weapon already held levels it up instead of adding a duplicate.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("Starting Weapon")]
        [SerializeField] private WeaponBase startingWeapon;

        [Header("References")]
        [SerializeField] private PlayerStats  playerStats;
        [SerializeField] private Transform    weaponContainer;
        [SerializeField] private GameConfigSO config;

        private int MaxSlots => config != null ? config.maxWeaponSlots : 6;

        // Active weapon instances keyed by their WeaponDataSO for quick lookup
        private readonly List<WeaponBase> _activeWeapons = new();

        private bool _isActive;

        public IReadOnlyList<WeaponBase> ActiveWeapons => _activeWeapons;

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            if (startingWeapon != null)
                _activeWeapons.Add(startingWeapon);
        }

        private void HandleGameStateChanged(GameState newState)
        {
            _isActive = newState == GameState.Playing;
        }

        public bool IsOverrideActive { get; set; } = false;

        private void Update()
        {
            if (!_isActive || IsOverrideActive) return;

            // All weapons fire independently on their own cooldowns
            foreach (var weapon in _activeWeapons)
            {
                if (weapon == null) continue;

                if (playerStats != null)
                    weapon.SetFireRateMultiplier(playerStats.FireRateMultiplier * OverchargeFireRateMultiplier);

                weapon.TryFire();
            }
        }

        /// <summary>
        /// Adds a new weapon or levels up an existing one.
        /// Called by UpgradeManager when the player picks a weapon upgrade.
        /// </summary>
        public bool AddOrLevelUpWeapon(WeaponDataSO weaponData)
        {
            var existing = FindWeapon(weaponData);
            if (existing != null)
            {
                existing.LevelUp();
                return true;
            }

            if (_activeWeapons.Count >= MaxSlots)
            {
                Debug.LogWarning("[WeaponManager] All weapon slots full.");
                return false;
            }

            var parent = weaponContainer != null ? weaponContainer : transform;
            var weapon = WeaponFactory.Create(weaponData, parent);
            if (weapon == null) return false;

            _activeWeapons.Add(weapon);
            return true;
        }

        /// <summary>
        /// Returns the WeaponBase instance for a given WeaponDataSO, or null if not held.
        /// </summary>
        public WeaponBase FindWeapon(WeaponDataSO weaponData)
        {
            foreach (var w in _activeWeapons)
                if (w != null && w.WeaponData == weaponData) return w;
            return null;
        }

        public bool HasWeapon(WeaponDataSO weaponData) => FindWeapon(weaponData) != null;

        public bool HasFreeSlot => _activeWeapons.Count < MaxSlots;

        public float OverchargeFireRateMultiplier { get; set; } = 1f;

        public void ReplaceWeapon(WeaponDataSO from, WeaponDataSO to)
        {
            var existing = FindWeapon(from);
            if (existing == null) return;
            _activeWeapons.Remove(existing);
            Destroy(existing.gameObject);
            AddOrLevelUpWeapon(to);
        }
    }
}
