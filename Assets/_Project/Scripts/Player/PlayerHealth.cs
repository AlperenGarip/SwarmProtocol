using UnityEngine;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;
using SwarmProtocol.Events;

namespace SwarmProtocol.Player
{
    /// <summary>
    /// Manages player health: taking damage, i-frames, and death.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;

        public float CurrentHP { get; private set; }
        public float MaxHP => playerStats != null ? playerStats.MaxHP : 100f;
        public float HealthPercent => MaxHP > 0 ? CurrentHP / MaxHP : 0f;
        public bool IsDead { get; private set; }

        private float _iFrameTimer;
        private bool _isInvulnerable;
        private float _lastMaxHP;

        private void OnEnable()  => Event<StatsChangedEvent>.Subscribe(OnStatsChanged);
        private void OnDisable() => Event<StatsChangedEvent>.Unsubscribe(OnStatsChanged);

        private void OnStatsChanged(StatsChangedEvent _)
        {
            float newMaxHP = MaxHP;
            float delta = newMaxHP - _lastMaxHP;
            if (delta > 0f) CurrentHP = Mathf.Min(CurrentHP + delta, newMaxHP);
            _lastMaxHP = newMaxHP;
            EventBus.PlayerDamaged(0f); // refresh HUD
        }

        private void Start()
        {
            _lastMaxHP = MaxHP;
            CurrentHP  = MaxHP;
            EventBus.PlayerDamaged(0f);
        }

        private void Update()
        {
            if (_isInvulnerable)
            {
                _iFrameTimer -= Time.deltaTime;
                if (_iFrameTimer <= 0f)
                {
                    _isInvulnerable = false;
                }
            }

            // HP regen from Recovery passive (Candy Box, etc.)
            if (!IsDead && playerStats != null && CurrentHP < MaxHP)
            {
                float regen = playerStats.RecoveryPerSecond;
                if (regen > 0f)
                {
                    float before = CurrentHP;
                    CurrentHP = Mathf.Min(CurrentHP + regen * Time.deltaTime, MaxHP);
                    // Refresh HUD so the bar fills as we heal, not just on next damage tick.
                    if (CurrentHP > before) EventBus.PlayerDamaged(0f);
                }
            }
        }

        /// <summary>
        /// Applies damage to the player. Respects i-frames.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (IsDead || _isInvulnerable) return;
            if (amount <= 0f) return;

            // Armor: flat damage reduction (Leather Armor, etc.). Always at least 1 damage gets through.
            if (playerStats != null)
            {
                float armor = playerStats.Armor;
                if (armor > 0f) amount = Mathf.Max(amount - armor, 1f);
            }

            CurrentHP = Mathf.Max(CurrentHP - amount, 0f);
            EventBus.PlayerDamaged(amount);
            AudioService.Instance?.PlaySfx(SfxId.PlayerDamage);

            // Activate i-frames
            _isInvulnerable = true;
            _iFrameTimer = playerStats != null ? playerStats.IFrameDuration : 0.5f;

            if (CurrentHP <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heals the player by the specified amount, clamped to MaxHP.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead) return;
            float before = CurrentHP;
            CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
            if (CurrentHP > before) EventBus.PlayerDamaged(0f);
        }

        /// <summary>
        /// Grants temporary invulnerability. Used after picking a level-up upgrade.
        /// </summary>
        public void GrantIFrames(float duration)
        {
            _isInvulnerable = true;
            _iFrameTimer = Mathf.Max(_iFrameTimer, duration);
        }

        /// <summary>
        /// Resets health to full. Called on game restart.
        /// </summary>
        public void ResetHealth()
        {
            CurrentHP = MaxHP;
            IsDead = false;
            _isInvulnerable = false;
            _iFrameTimer = 0f;
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            AudioService.Instance?.PlaySfx(SfxId.PlayerDeath);
            EventBus.PlayerDied();
        }

        /// <summary>
        /// Whether the player is currently in i-frames.
        /// useful for visual feedback (flashing effect).
        /// </summary>
        public bool IsInvulnerable => _isInvulnerable;
    }
}
