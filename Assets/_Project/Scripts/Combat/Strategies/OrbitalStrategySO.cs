using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Combat;
using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat.Strategies
{
    [CreateAssetMenu(fileName = "OrbitalStrategy", menuName = "SwarmProtocol/Strategies/Orbital")]
    public class OrbitalStrategySO : FireStrategySO
    {
        private static readonly List<EnemyBase> _hitBuffer = new();

        public override void Execute(FireContext ctx)
        {
            float range    = ctx.WeaponData.range * ctx.AreaMultiplier;
            float outerSqr = range * range;
            float innerSqr = (range * 0.4f) * (range * 0.4f);
            ActiveEnemyRegistry.Instance?.GetEnemiesInRange(ctx.FirePoint.position, outerSqr, _hitBuffer);
            float damage = (ctx.WeaponData.baseDamage + ctx.LevelDamageBonus) * ctx.OverchargeDamageMultiplier;
            foreach (var enemy in _hitBuffer)
            {
                if (enemy == null) continue;
                float sqrDist = (enemy.transform.position - ctx.FirePoint.position).sqrMagnitude;
                if (sqrDist < innerSqr) continue;
                DamageSystem.ApplyDamage(enemy, damage, ctx.DamageMultiplier, ctx.CritChance, ctx.FirePoint.position,
                    ctx.WeaponData.knockbackForce, ctx.WeaponData.knockbackDuration);
            }
        }
    }
}
