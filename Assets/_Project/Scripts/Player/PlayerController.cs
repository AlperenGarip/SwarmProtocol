using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Profiling;
using SwarmProtocol.Core;

namespace SwarmProtocol.Player
{
    /// <summary>
    /// Handles player movement (WASD) and auto-rotation toward the nearest enemy.
    /// Enemy search is throttled to 10/sec to avoid per-frame FindGameObjectsWithTag cost.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;

        [Header("Movement")]
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 40f;

        [Header("Auto-Aim")]
        [SerializeField] private float enemySearchInterval = 0.1f; // 10 checks/sec — saves ~90% CPU vs every frame

        [Header("Benchmark toggle")]
        [Tooltip("If true, force the FindGameObjectsWithTag fallback path even when ActiveEnemyRegistry is available. For performance comparison only.")]
        public bool useFindWithTagBenchmark = false;

        private static readonly ProfilerMarker _markerNearestRegistry    = new("PlayerController.FindNearest_Registry");
        private static readonly ProfilerMarker _markerNearestFindWithTag = new("PlayerController.FindNearest_FindWithTag");

        private Rigidbody _rb;
        private UnityEngine.Camera _mainCamera;
        private Vector2 _moveInput;
        private Vector3 _currentVelocity;
        private bool _isActive;

        private Transform _nearestEnemy;
        private float _enemySearchTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _mainCamera = UnityEngine.Camera.main;

            _rb.constraints = RigidbodyConstraints.FreezePositionY
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationZ;
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState newState)
        {
            _isActive = newState == GameState.Playing;
            if (!_isActive)
            {
                _moveInput = Vector2.zero;
                _rb.linearVelocity = Vector3.zero;
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            _moveInput = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f),
                (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f)
            );

            // Throttled enemy search — imperceptible at 10/sec, saves ~90% CPU vs every frame
            _enemySearchTimer -= Time.deltaTime;
            if (_enemySearchTimer <= 0f)
            {
                _nearestEnemy = FindNearestEnemy();
                _enemySearchTimer = enemySearchInterval;
            }
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;
            HandleMovement();
            FaceNearestEnemy();
        }

        private void HandleMovement()
        {
            float maxSpeed = playerStats != null ? playerStats.MoveSpeed : 8f;

            // Camera-relative movement: W moves in camera's forward direction, not world forward
            Vector3 targetDirection;
            if (_mainCamera != null)
            {
                Vector3 camForward = _mainCamera.transform.forward;
                Vector3 camRight   = _mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y   = 0f;
                camForward.Normalize();
                camRight.Normalize();
                targetDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
            }
            else
            {
                targetDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
            }

            if (targetDirection.sqrMagnitude > 0.01f)
            {
                Vector3 targetVelocity = targetDirection * maxSpeed;
                _currentVelocity = Vector3.MoveTowards(
                    _currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                _currentVelocity = Vector3.MoveTowards(
                    _currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            }

            _rb.linearVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);
        }

        private void FaceNearestEnemy()
        {
            Vector3 lookDir = _nearestEnemy != null
                ? _nearestEnemy.position - transform.position
                : _currentVelocity; // fall back to movement direction when no enemies

            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, 15f * Time.fixedDeltaTime);
            }
        }

        private Transform FindNearestEnemy()
        {
            // Use ActiveEnemyRegistry (O(n) sqrMagnitude) instead of FindGameObjectsWithTag
            var registry = Core.ActiveEnemyRegistry.Instance;
            if (registry != null && !useFindWithTagBenchmark)
            {
                using (_markerNearestRegistry.Auto())
                {
                    var nearest = registry.GetNearest(transform.position);
                    return nearest?.transform;
                }
            }

            // Fallback (also: forced via benchmark toggle).
            // FindGameObjectsWithTag allocates a new GameObject[] every call → GC pressure.
            using (_markerNearestFindWithTag.Auto())
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                Transform nearestT = null;
                float minDistSq = float.MaxValue;
                foreach (GameObject enemy in enemies)
                {
                    float distSq = (enemy.transform.position - transform.position).sqrMagnitude;
                    if (distSq < minDistSq) { minDistSq = distSq; nearestT = enemy.transform; }
                }
                return nearestT;
            }
        }

        public float NormalizedSpeed
        {
            get
            {
                float maxSpeed = playerStats != null ? playerStats.MoveSpeed : 8f;
                return maxSpeed > 0f ? _currentVelocity.magnitude / maxSpeed : 0f;
            }
        }

        public Vector3 MoveDirection => _currentVelocity.normalized;

        public Vector3 AimDirection
        {
            get
            {
                if (_nearestEnemy != null)
                {
                    // Aim at enemy center (account for height difference)
                    var col = _nearestEnemy.GetComponent<Collider>();
                    Vector3 targetCenter = col != null ? col.bounds.center : _nearestEnemy.position;
                    Vector3 dir = targetCenter - transform.position;
                    if (dir.sqrMagnitude > 0.01f) return dir.normalized;
                }
                return transform.forward;
            }
        }
    }
}
