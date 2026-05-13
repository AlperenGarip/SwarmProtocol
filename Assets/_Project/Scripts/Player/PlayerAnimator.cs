using UnityEngine;

namespace SwarmProtocol.Player
{
    /// <summary>
    /// Wrapper for the Animator component.
    /// Manages animation parameters based on movement and combat state.
    /// Skeleton implementation for Phase 1 — real animations will be integrated in Phase 2.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;

        private Animator _animator;

        // Animation parameter hashes (cached for performance)
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int IsAttackingParam = Animator.StringToHash("IsAttacking");
        private static readonly int IsDeadParam = Animator.StringToHash("IsDead");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_animator == null || playerController == null) return;

            // Set speed for blend tree (walk/run)
            _animator.SetFloat(SpeedParam, playerController.NormalizedSpeed);
        }

        /// <summary>
        /// Trigger attack animation. Called by weapon system.
        /// </summary>
        public void PlayAttack()
        {
            if (_animator == null) return;
            _animator.SetTrigger(IsAttackingParam);
        }

        /// <summary>
        /// Trigger death animation.
        /// </summary>
        public void PlayDeath()
        {
            if (_animator == null) return;
            _animator.SetBool(IsDeadParam, true);
        }

        /// <summary>
        /// Reset all animation states. Called on game restart.
        /// </summary>
        public void ResetAnimations()
        {
            if (_animator == null) return;
            _animator.SetFloat(SpeedParam, 0f);
            _animator.SetBool(IsDeadParam, false);
        }
    }
}
