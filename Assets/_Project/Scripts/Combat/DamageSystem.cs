using UnityEngine;
using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat
{
    /// <summary>
    /// Central damage calculation and application.
    /// Every hit goes through here — damage math + optional knockback in one call.
    /// </summary>
    public static class DamageSystem
    {
        /// <summary>
        /// Applies damage with no player modifiers (raw weapon damage).
        /// </summary>
        public static void ApplyDamage(EnemyBase enemy, float baseDamage)
        {
            if (enemy == null) return;
            enemy.TakeDamage(CalculateDamage(baseDamage, 1f, 0f));
        }

        /// <summary>
        /// Applies damage with full player stat modifiers and optional knockback.
        /// </summary>
        /// <param name="sourcePosition">World position of the hit source (for knockback direction).</param>
        /// <param name="knockbackForce">0 = use GameConfigSO default.</param>
        /// <param name="knockbackDuration">0 = use GameConfigSO default.</param>
        public static void ApplyDamage(EnemyBase enemy, float baseDamage,
                                        float damageMultiplier, float critChance,
                                        Vector3 sourcePosition = default,
                                        float knockbackForce   = 0f,
                                        float knockbackDuration = 0f)
        {
            if (enemy == null) return;

            float finalDamage = CalculateDamage(baseDamage, damageMultiplier, critChance);
            enemy.TakeDamage(finalDamage);

            // Apply knockback if any force is specified OR if GameConfigSO supplies a default.
            // We check ServiceLocator so this remains a static utility with no serialized refs.
            if (Core.ServiceLocator.TryGet<GameConfigSO>(out var config))
            {
                float force    = knockbackForce    > 0f ? knockbackForce    : config.defaultKnockbackForce;
                float duration = knockbackDuration > 0f ? knockbackDuration : config.defaultKnockbackDuration;
                if (force > 0f)
                    KnockbackSystem.Apply(enemy, sourcePosition, force, duration);
            }
            else if (knockbackForce > 0f)
            {
                // No config registered yet — use supplied values directly
                KnockbackSystem.Apply(enemy, sourcePosition, knockbackForce, knockbackDuration);
            }
        }

        /// <summary>Calculates final damage without applying it.</summary>
        public static float CalculateDamage(float baseDamage, float damageMultiplier, float critChance)
        {
            float damage = baseDamage * damageMultiplier;
            if (critChance > 0f && Random.value <= critChance)
                damage *= 2f;
            return Mathf.Max(damage, 0f);
        }
    }
}
