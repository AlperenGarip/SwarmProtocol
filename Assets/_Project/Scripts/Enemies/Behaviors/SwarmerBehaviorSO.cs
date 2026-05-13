using UnityEngine;

namespace SwarmProtocol.Enemies.Behaviors
{
    [CreateAssetMenu(fileName = "SwarmerBehavior", menuName = "SwarmProtocol/Behaviors/Swarmer")]
    public class SwarmerBehaviorSO : EnemyBehaviorSO
    {
        public override void Execute(EnemyBase enemy, EnemyNavigation nav, Transform playerTransform)
        {
            if (playerTransform == null) return;
            nav.MoveTo(playerTransform.position);
        }
    }
}
