using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Enemies;

namespace SwarmProtocol.Combat.Strategies
{
    [CreateAssetMenu(fileName = "MeleeStrategy", menuName = "SwarmProtocol/Strategies/Melee")]
    public class MeleeStrategySO : FireStrategySO
    {
        private static readonly List<EnemyBase> _hitBuffer = new();

        public override void Execute(FireContext ctx)
        {
            if (ctx.WeaponData == null) return;

            float range    = ctx.WeaponData.range * ctx.AreaMultiplier;
            float sqrRange = range * range;
            ActiveEnemyRegistry.Instance?.GetEnemiesInRange(ctx.FirePoint.position, sqrRange, _hitBuffer);

            float damage = (ctx.WeaponData.baseDamage + ctx.LevelDamageBonus)
                           * ctx.OverchargeDamageMultiplier;

            foreach (var enemy in _hitBuffer)
            {
                if (enemy == null) continue;
                DamageSystem.ApplyDamage(
                    enemy, damage,
                    damageMultiplier: ctx.DamageMultiplier,
                    critChance:       ctx.CritChance,
                    sourcePosition:   ctx.FirePoint.position,
                    knockbackForce:   ctx.WeaponData.knockbackForce,
                    knockbackDuration: ctx.WeaponData.knockbackDuration);
            }
        }
    }
}
