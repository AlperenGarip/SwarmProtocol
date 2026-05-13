using UnityEngine;

namespace SwarmProtocol.Enemies.Behaviors
{
    /// <summary>
    /// Tank behavior: slow, relentless chase. High HP/damage is configured
    /// in EnemyDataSO — this behavior only drives movement.
    /// </summary>
    [CreateAssetMenu(fileName = "TankBehavior", menuName = "SwarmProtocol/Behaviors/Tank")]
    public class TankBehaviorSO : EnemyBehaviorSO
    {
        [Tooltip("Extra stop distance — tank stops slightly closer than swarmers.")]
        [SerializeField] private float stopDistance = 0.5f;

        public override void Execute(EnemyBase enemy, EnemyNavigation nav, Transform playerTransform)
        {
            if (playerTransform == null) return;

            float dist = Vector3.Distance(enemy.transform.position, playerTransform.position);
            if (dist > stopDistance)
                nav.MoveTo(playerTransform.position);
            else
                nav.Stop();
        }
    }
}
