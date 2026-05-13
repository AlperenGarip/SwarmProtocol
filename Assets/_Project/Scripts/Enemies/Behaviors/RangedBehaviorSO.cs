using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Enemies.Behaviors
{
    /// <summary>
    /// Ranged behavior: chases until within attackRange, then stops and fires pooled projectiles.
    /// Uses EnemyBase.BehaviorTimer for per-instance attack cooldown.
    /// Requires the enemy prefab to have an EnemyProjectile prefab wired here.
    /// </summary>
    [CreateAssetMenu(fileName = "RangedBehavior", menuName = "SwarmProtocol/Behaviors/Ranged")]
    public class RangedBehaviorSO : EnemyBehaviorSO
    {
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private float           projectileSpeed = 10f;
        [SerializeField] private float           projectileRange = 20f;

        public override void Execute(EnemyBase enemy, EnemyNavigation nav, Transform playerTransform)
        {
            if (playerTransform == null || enemy.EnemyData == null) return;

            enemy.BehaviorTimer -= Time.deltaTime;

            float dist        = Vector3.Distance(enemy.transform.position, playerTransform.position);
            float attackRange = enemy.EnemyData.attackRange;

            if (dist <= attackRange)
            {
                nav.Stop();

                Vector3 lookDir = playerTransform.position - enemy.transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    enemy.transform.rotation = Quaternion.LookRotation(lookDir);

                if (enemy.BehaviorTimer <= 0f)
                {
                    FireProjectile(enemy, playerTransform);
                    enemy.BehaviorTimer = enemy.EnemyData.attackCooldown;
                }
            }
            else
            {
                nav.MoveTo(playerTransform.position);
            }
        }

        private void FireProjectile(EnemyBase enemy, Transform playerTransform)
        {
            if (projectilePrefab == null || ObjectPoolManager.Instance == null) return;

            Vector3 spawnPos = enemy.transform.position + Vector3.up * 0.5f;
            Vector3 dir      = playerTransform.position - spawnPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            dir.Normalize();

            var proj = ObjectPoolManager.Instance.Get(projectilePrefab, spawnPos, Quaternion.identity);
            proj?.Initialize(
                damage:    enemy.EnemyData.damage * enemy.DifficultyMultiplier,
                speed:     projectileSpeed,
                maxRange:  projectileRange,
                direction: dir,
                prefabRef: projectilePrefab);
        }
    }
}
