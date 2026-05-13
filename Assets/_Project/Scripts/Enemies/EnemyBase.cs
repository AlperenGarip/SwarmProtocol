using UnityEngine;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Vfx;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Abstract base class for all enemies.
    /// - Manages health, damage, death.
    /// - Registers/unregisters with ActiveEnemyRegistry.
    /// - Supports knockback via agent.isStopped + agent.Move() (NEVER disable NavMeshAgent).
    /// - Supports Orologion freeze.
    /// - Die() fires ONLY EnemyKilledEvent — all drops are handled by independent drop handlers.
    /// </summary>
    [RequireComponent(typeof(EnemyNavigation))]
    public abstract class EnemyBase : MonoBehaviour, IPoolable
    {
        [Header("Enemy Data")]
        [SerializeField] protected EnemyDataSO enemyData;

        public EnemyDataSO EnemyData           => enemyData;
        public float       DifficultyMultiplier => _difficultyMultiplier;
        public float CurrentHealth { get; protected set; }
        public float MaxHealth     { get; protected set; }
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        public bool  IsDead        { get; protected set; }

        /// <summary>General-purpose per-instance timer for EnemyBehaviorSO subclasses (e.g. attack cooldown).</summary>
        public float BehaviorTimer { get; set; }

        // ─── Knockback ────────────────────────────────────────────────────────
        private Vector3 _knockbackVelocity;
        private float   _stunTimer;

        // ─── Freeze (Orologion) ────────────────────────────────────────────────
        private float _freezeTimer;
        public  bool  IsFrozen          => _freezeTimer > 0f;
        /// <summary>True while knockback stun OR Orologion freeze prevents normal movement.</summary>
        public  bool  IsMovementBlocked => _stunTimer > 0f || _freezeTimer > 0f;

        // ─── References ───────────────────────────────────────────────────────
        protected EnemyNavigation _nav;
        private   EnemyBase       _prefabRef;
        protected float           _difficultyMultiplier = 1f;

        // ─── Hit flash ────────────────────────────────────────────────────────
        private Renderer[] _renderers;
        private Color[]    _originalColors;
        private float      _flashTimer;
        private const float HitFlashDuration = 0.08f;
        private static readonly Color HitFlashColor = Color.white;

        protected virtual void Awake()
        {
            _nav = GetComponent<EnemyNavigation>();

            // Cache renderers + their original colors so we can flash white on hit and restore.
            _renderers = GetComponentsInChildren<Renderer>(true);
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mat = _renderers[i].material; // instance copy — safe to mutate
                _originalColors[i] = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                                   : mat.HasProperty("_Color")     ? mat.GetColor("_Color")
                                   : Color.white;
            }
        }

        private void TickFlash()
        {
            if (_flashTimer <= 0f) return;
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f) ApplyColorToAll(GetOriginal);
        }

        private Color GetOriginal(int i) => _originalColors[i];

        private void ApplyColorToAll(System.Func<int, Color> picker)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                var mat = r.material;
                Color c = picker(i);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            }
        }

        private void TriggerFlash()
        {
            _flashTimer = HitFlashDuration;
            ApplyColorToAll(_ => HitFlashColor);
        }

        /// <summary>
        /// Initializes the enemy after being retrieved from the pool.
        /// </summary>
        public virtual void Initialize(float difficultyMultiplier, EnemyBase prefabRef)
        {
            _difficultyMultiplier = difficultyMultiplier;
            _prefabRef            = prefabRef;

            MaxHealth     = enemyData.maxHealth * _difficultyMultiplier;
            CurrentHealth = MaxHealth;
            IsDead        = false;

            _knockbackVelocity = Vector3.zero;
            _stunTimer         = 0f;
            _freezeTimer       = 0f;
            BehaviorTimer      = 0f;

            if (_nav != null)
                _nav.Initialize(enemyData.moveSpeed * _difficultyMultiplier);

            // Register with the centralized registry
            ActiveEnemyRegistry.Instance?.Register(this);
        }

        protected virtual void Update()
        {
            TickFlash();

            if (IsDead) return;

            // ── Freeze tick ───────────────────────────────────────
            if (_freezeTimer > 0f)
            {
                _freezeTimer -= Time.deltaTime;
                if (_freezeTimer <= 0f)
                {
                    _freezeTimer = 0f;
                    // Resume pathfinding after freeze, unless still stunned by knockback
                    if (_stunTimer <= 0f)
                        _nav?.SetStopped(false);
                }
                return; // completely halted while frozen
            }

            // ── Knockback tick ────────────────────────────────────
            if (_stunTimer > 0f)
            {
                _stunTimer -= Time.deltaTime;
                _nav?.Move(_knockbackVelocity * Time.deltaTime);

                // Exponential decay over the remaining stun window
                float safeDt = Mathf.Max(_stunTimer, 0.001f);
                _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero,
                    Time.deltaTime / safeDt);

                if (_stunTimer <= 0f)
                {
                    _knockbackVelocity = Vector3.zero;
                    _nav?.SetStopped(false);
                }
                return;
            }

            // ── Normal behaviour ──────────────────────────────────
            UpdateBehavior();
        }

        /// <summary>Override in subclasses to implement move/attack logic.</summary>
        protected abstract void UpdateBehavior();

        // ─── Damage ───────────────────────────────────────────────────────────

        public virtual void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (amount <= 0f) return;

            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);

            if (CurrentHealth <= 0f)
                Die();
            else
            {
                AudioService.Instance?.PlaySfx(SfxId.EnemyHit, 0.4f);
                TriggerFlash();
            }
        }

        // ─── Knockback ────────────────────────────────────────────────────────

        /// <summary>
        /// Injects a knockback velocity. Called by KnockbackSystem — do not call directly.
        /// Uses agent.isStopped so the enemy stays on the NavMesh at all times.
        /// </summary>
        public void ApplyKnockback(Vector3 velocity, float stunDuration)
        {
            if (IsDead || IsFrozen) return;
            _knockbackVelocity = velocity;
            _stunTimer         = stunDuration;
            _nav?.SetStopped(true);
        }

        /// <summary>
        /// Stuns the enemy in place for <paramref name="duration"/> seconds with no displacement.
        /// Extends existing stun rather than resetting it.
        /// </summary>
        public void Stun(float duration)
        {
            if (IsDead) return;
            _stunTimer         = Mathf.Max(_stunTimer, duration);
            _knockbackVelocity = Vector3.zero;
            _nav?.SetStopped(true);
        }

        // ─── Freeze ───────────────────────────────────────────────────────────

        /// <summary>
        /// Freezes all movement for <paramref name="duration"/> seconds (Orologion pickup).
        /// </summary>
        public void Freeze(float duration)
        {
            if (IsDead) return;
            _freezeTimer = duration;
            _nav?.SetStopped(true);
        }

        /// <summary>Applies a material tint to the first renderer found. Used by EliteBurstDriver.</summary>
        public void ApplyTint(Color color)
        {
            var rend = GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = color;
        }

        // ─── Death ────────────────────────────────────────────────────────────

        protected virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;

            AudioService.Instance?.PlaySfx(SfxId.EnemyDeath, 0.6f);
            VfxService.Instance?.SpawnDeathBurst(transform.position + Vector3.up * 0.5f, _originalColors.Length > 0 ? _originalColors[0] : Color.white);

            // Unregister BEFORE returning to pool
            ActiveEnemyRegistry.Instance?.Unregister(this);

            // Fire event — XPDropHandler, GoldDropHandler, HealthOrbDropHandler, ChestDropHandler
            // each listen independently. EnemyBase never touches drop logic.
            Event<EnemyKilledEvent>.Fire(new EnemyKilledEvent
            {
                Enemy         = this,
                EnemyData     = enemyData,
                DeathPosition = transform.position
            });

            // Also fire legacy EventBus for backward-compat until full migration
            Core.EventBus.EnemyKilled(this);

            ReturnToPool();
        }

        protected void ReturnToPool()
        {
            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        // ─── IPoolable ────────────────────────────────────────────────────────

        public virtual void OnSpawn()
        {
            IsDead       = false;
            _stunTimer   = 0f;
            _freezeTimer = 0f;
            _knockbackVelocity = Vector3.zero;
        }

        public virtual void OnDespawn()
        {
            IsDead = true;
            ActiveEnemyRegistry.Instance?.Unregister(this);
        }
    }
}
