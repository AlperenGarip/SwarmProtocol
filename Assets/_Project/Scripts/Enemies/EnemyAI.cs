using UnityEngine;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Enemy AI states.
    /// </summary>
    public enum EnemyAIState
    {
        Idle,
        Chase,
        Attack,
        Death
    }

    /// <summary>
    /// Simple FSM driving enemy behavior: Idle → Chase → Attack → Death.
    /// Works with EnemyNavigation for movement and EnemyBase for health/death.
    /// Configurable thresholds allow different enemy types to behave differently.
    /// </summary>
    [RequireComponent(typeof(EnemyNavigation))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 500f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float contactDamage = 10f;

        public EnemyAIState CurrentState { get; private set; } = EnemyAIState.Idle;

        private EnemyNavigation _navigation;
        private EnemyBase _enemyBase;
        private Transform _playerTransform;
        private float _attackTimer;
        private bool _isActive;

        private void Awake()
        {
            _navigation = GetComponent<EnemyNavigation>();
            _enemyBase = GetComponent<EnemyBase>();
        }

        private void OnEnable()
        {
            Core.EventBus.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            Core.EventBus.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(Core.GameState newState)
        {
            _isActive = newState == Core.GameState.Playing;
            if (!_isActive && _navigation != null)
            {
                _navigation.Stop();
            }
        }

        /// <summary>
        /// Initializes AI with data from EnemyDataSO (called by EnemySwarmer/EnemyTank/etc.).
        /// </summary>
        public void Initialize(float attackRangeOverride, float attackCooldownOverride,
                                float contactDamageOverride, float difficultyMultiplier)
        {
            attackRange = attackRangeOverride;
            attackCooldown = attackCooldownOverride;
            contactDamage = contactDamageOverride * difficultyMultiplier;
            CurrentState = EnemyAIState.Idle;
            _attackTimer = 0f;

            // Enemy may be spawned while game is already Playing — check current state directly
            _isActive = Core.GameManager.Instance != null &&
                        Core.GameManager.Instance.CurrentState == Core.GameState.Playing;
        }

        private void Update()
        {
            if (!_isActive || _enemyBase == null || _enemyBase.IsDead)
            {
                if (_enemyBase != null && _enemyBase.IsDead && CurrentState != EnemyAIState.Death)
                    TransitionTo(EnemyAIState.Death);
                return;
            }

            // Knockback / Orologion freeze — EnemyBase owns the movement block; we just wait.
            if (_enemyBase.IsMovementBlocked) return;

            // Find player reference if needed
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) _playerTransform = player.transform;
                else return;
            }

            // Tick attack cooldown
            if (_attackTimer > 0f)
                _attackTimer -= Time.deltaTime;

            // FSM update
            switch (CurrentState)
            {
                case EnemyAIState.Idle:
                    UpdateIdle();
                    break;
                case EnemyAIState.Chase:
                    UpdateChase();
                    break;
                case EnemyAIState.Attack:
                    UpdateAttack();
                    break;
            }
        }

        private void UpdateIdle()
        {
            // Vampire-Survivors-style: enemies always chase the player from spawn.
            // detectionRange field kept for future stealth/aggro mechanics, but no longer gates.
            TransitionTo(EnemyAIState.Chase);
        }

        private void UpdateChase()
        {
            float distance = DistanceToPlayer();

            if (distance <= attackRange)
            {
                TransitionTo(EnemyAIState.Attack);
                return;
            }

            _navigation.MoveTo(_playerTransform.position);
        }

        private void UpdateAttack()
        {
            float distance = DistanceToPlayer();

            if (distance > attackRange * 1.2f) // hysteresis
            {
                TransitionTo(EnemyAIState.Chase);
                return;
            }

            if (_attackTimer <= 0f)
            {
                PerformAttack();
                _attackTimer = attackCooldown;
            }
        }

        private void PerformAttack()
        {
            // For melee enemies: damage the player on contact
            var playerHealth = _playerTransform.GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }

        private void TransitionTo(EnemyAIState newState)
        {
            // Exit current state
            switch (CurrentState)
            {
                case EnemyAIState.Chase:
                    _navigation.Stop();
                    break;
            }

            CurrentState = newState;

            // Enter new state
            switch (newState)
            {
                case EnemyAIState.Death:
                    _navigation.Stop();
                    break;
            }
        }

        private float DistanceToPlayer()
        {
            if (_playerTransform == null) return float.MaxValue;
            return Vector3.Distance(transform.position, _playerTransform.position);
        }
    }
}
