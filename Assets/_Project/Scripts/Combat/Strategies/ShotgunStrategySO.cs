using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Combat.Strategies
{
    [CreateAssetMenu(fileName = "ShotgunStrategy", menuName = "SwarmProtocol/Strategies/Shotgun")]
    public class ShotgunStrategySO : FireStrategySO
    {
        public override void Execute(FireContext ctx)
        {
            if (ctx.WeaponData?.projectilePrefab == null) return;

            var prefabBase = ctx.WeaponData.projectilePrefab.GetComponent<ProjectileBase>();
            if (prefabBase == null) return;

            Vector3 spawnPos  = ctx.FirePoint != null ? ctx.FirePoint.position : Vector3.zero;
            float halfSpread  = ctx.WeaponData.spreadAngle * 0.5f;
            int   pellets     = Mathf.Max(1, ctx.WeaponData.projectilesPerShot);

            for (int i = 0; i < pellets; i++)
            {
                float   angle = Random.Range(-halfSpread, halfSpread);
                Vector3 dir   = Quaternion.Euler(0f, angle, 0f) * ctx.AimDirection;

                var proj = ObjectPoolManager.Instance.Get(prefabBase, spawnPos, Quaternion.identity);
                proj.Initialize(
                    ctx.WeaponData.baseDamage + ctx.LevelDamageBonus,
                    ctx.WeaponData.projectileSpeed,
                    ctx.WeaponData.range * ctx.AreaMultiplier,
                    ctx.WeaponData.piercing,
                    dir,
                    prefabBase,
                    ctx.WeaponData.knockbackForce,
                    ctx.WeaponData.knockbackDuration,
                    ctx.DamageMultiplier,
                    ctx.CritChance);
            }
        }
    }
}
