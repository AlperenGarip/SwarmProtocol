using UnityEngine;
using Unity.Profiling;
using SwarmProtocol.Audio;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Abstract base class for all weapons. Handles cooldown timing.
    /// Concrete weapons implement Fire() with their specific behavior.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Data")]
        [SerializeField] protected WeaponDataSO weaponData;

        [Header("References")]
        [SerializeField] protected Transform firePoint; // where projectiles spawn

        public WeaponDataSO WeaponData    => weaponData;
        public bool IsOnCooldown          => _cooldownTimer > 0f;
        public int  CurrentLevel          { get; private set; } = 1;
        public bool IsMaxLevel            => weaponData != null && CurrentLevel >= weaponData.maxLevel;

        private float _cooldownTimer;
        private float _fireRateMultiplier        = 1f;
        private float _overchargeDamageMultiplier = 1f;

        public float OverchargeDamageMultiplier => _overchargeDamageMultiplier;

        public void SetOverchargeDamageMultiplier(float multiplier) =>
            _overchargeDamageMultiplier = Mathf.Max(multiplier, 1f);

        public void ResetOvercharge() => _overchargeDamageMultiplier = 1f;

        protected virtual void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Attempts to fire the weapon. Returns true if fired, false if on cooldown.
        /// </summary>
        private static readonly ProfilerMarker _markerTryFire = new("WeaponBase.TryFire");

        public bool TryFire()
        {
            if (IsOnCooldown || weaponData == null) return false;

            using (_markerTryFire.Auto())
            {
                Fire();
                // Lower volume than other SFX since auto-fire can be very rapid.
                AudioService.Instance?.PlaySfx(SfxId.WeaponFire, 0.5f);
                _cooldownTimer = weaponData.FireInterval / _fireRateMultiplier;
            }
            return true;
        }

        /// <summary>
        /// Override to implement weapon-specific firing behavior.
        /// Called when the weapon is off cooldown.
        /// </summary>
        protected abstract void Fire();

        /// <summary>
        /// Increments weapon level and fires the WeaponLeveledUp event.
        /// Concrete weapons override OnLevelUp() to apply per-level stat changes.
        /// </summary>
        public void LevelUp()
        {
            if (IsMaxLevel) return;
            CurrentLevel++;
            OnLevelUp(CurrentLevel);
            Core.EventBus.WeaponLeveledUp(weaponData, CurrentLevel);
        }

        /// <summary>
        /// Override to apply level-specific stat changes (damage, fire rate, etc.).
        /// </summary>
        protected virtual void OnLevelUp(int newLevel) { }

        /// <summary>
        /// Updates the fire rate multiplier from PlayerStats.
        /// </summary>
        public void SetFireRateMultiplier(float multiplier)
        {
            _fireRateMultiplier = Mathf.Max(multiplier, 0.1f);
        }

        /// <summary>
        /// Cooldown progress from 0 (just fired) to 1 (ready).
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                if (weaponData == null) return 1f;
                float interval = weaponData.FireInterval / _fireRateMultiplier;
                return interval > 0f ? 1f - Mathf.Clamp01(_cooldownTimer / interval) : 1f;
            }
        }
    }
}
