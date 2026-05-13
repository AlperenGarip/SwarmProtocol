using UnityEngine;
using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Applies knockback velocity to an enemy when it takes damage.
    /// Uses agent.isStopped + agent.Move() — NEVER disables NavMeshAgent.
    ///
    /// Called from DamageSystem after every hit. Force and duration can come from
    /// the weapon's WeaponLevelData or fall back to GameConfigSO defaults.
    /// </summary>
    public static class KnockbackSystem
    {
        /// <summary>
        /// Injects a knockback impulse into <paramref name="enemy"/>.
        /// </summary>
        /// <param name="enemy">The enemy that was hit.</param>
        /// <param name="sourcePosition">World position of the hit source (weapon/player).</param>
        /// <param name="force">Units of knockback velocity.</param>
        /// <param name="stunDuration">Seconds the enemy is stopped.</param>
        public static void Apply(EnemyBase enemy, Vector3 sourcePosition, float force, float stunDuration)
        {
            if (enemy == null || force <= 0f || stunDuration <= 0f) return;

            Vector3 direction = (enemy.transform.position - sourcePosition).normalized;
            if (direction.sqrMagnitude < 0.01f)
                direction = enemy.transform.forward;  // fallback if exactly on top

            enemy.ApplyKnockback(direction * force, stunDuration);
        }

        /// <summary>
        /// Convenience overload that reads defaults from <paramref name="config"/>
        /// when the weapon doesn't specify its own values.
        /// </summary>
        public static void Apply(EnemyBase enemy, Vector3 sourcePosition,
                                 float weaponForce, float weaponDuration,
                                 GameConfigSO config)
        {
            float force    = weaponForce    > 0f ? weaponForce    : config.defaultKnockbackForce;
            float duration = weaponDuration > 0f ? weaponDuration : config.defaultKnockbackDuration;
            Apply(enemy, sourcePosition, force, duration);
        }
    }
}
