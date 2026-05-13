using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Combat;
using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat.Strategies
{
    [CreateAssetMenu(fileName = "NdujaStrategy", menuName = "SwarmProtocol/Strategies/Nduja")]
    public class NdujaStrategySO : FireStrategySO
    {
        [Range(10f, 180f)] public float coneAngleDegrees = 90f;

        private static readonly List<EnemyBase> _hitBuffer = new();

        public override void Execute(FireContext ctx)
        {
            float range        = ctx.WeaponData.range * ctx.AreaMultiplier;
            float sqrRange     = range * range;
            float cosThreshold = Mathf.Cos(coneAngleDegrees * 0.5f * Mathf.Deg2Rad);
            Vector3 aimDir     = ctx.AimDirection.normalized;

            ActiveEnemyRegistry.Instance?.GetEnemiesInRange(ctx.FirePoint.position, sqrRange, _hitBuffer);
            float damage = (ctx.WeaponData.baseDamage + ctx.LevelDamageBonus) * ctx.OverchargeDamageMultiplier;

            foreach (var enemy in _hitBuffer)
            {
                if (enemy == null) continue;
                Vector3 toEnemy = enemy.transform.position - ctx.FirePoint.position;
                if (toEnemy.sqrMagnitude < 0.01f) continue;
                if (Vector3.Dot(toEnemy.normalized, aimDir) < cosThreshold) continue;
                DamageSystem.ApplyDamage(enemy, damage, ctx.DamageMultiplier, ctx.CritChance, ctx.FirePoint.position,
                    ctx.WeaponData.knockbackForce, ctx.WeaponData.knockbackDuration);
            }
        }
    }
}
