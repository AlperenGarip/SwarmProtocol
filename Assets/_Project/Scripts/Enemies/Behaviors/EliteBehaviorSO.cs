using UnityEngine;

namespace SwarmProtocol.Enemies.Behaviors
{
    /// <summary>
    /// Elite behavior: aggressive chaser. Within closeRange it circles the player
    /// (moves perpendicular) instead of stopping, making it harder to kite.
    /// Visual distinction comes from the red tint applied by EliteBurstDriver.
    /// Stats (HP, damage, speed) are set higher via EnemyDataSO.
    /// </summary>
    [CreateAssetMenu(fileName = "EliteBehavior", menuName = "SwarmProtocol/Behaviors/Elite")]
    public class EliteBehaviorSO : EnemyBehaviorSO
    {
        [Tooltip("Distance at which the elite switches from chasing to circling.")]
        [SerializeField] private float closeRange = 3f;

        public override void Execute(EnemyBase enemy, EnemyNavigation nav, Transform playerTransform)
        {
            if (playerTransform == null) return;

            float dist = Vector3.Distance(enemy.transform.position, playerTransform.position);

            if (dist > closeRange)
            {
                nav.MoveTo(playerTransform.position);
            }
            else
            {
                // Circle: move to a point offset 45° around the player
                Vector3 toEnemy     = (enemy.transform.position - playerTransform.position).normalized;
                Vector3 orbitTarget = playerTransform.position +
                    Quaternion.Euler(0f, 45f, 0f) * toEnemy * closeRange;
                nav.MoveTo(orbitTarget);
            }
        }
    }
}
