using UnityEngine;
using Unity.Profiling;
using SwarmProtocol.Core;

namespace SwarmProtocol.Combat.Strategies
{
    [CreateAssetMenu(fileName = "RapidFireStrategy", menuName = "SwarmProtocol/Strategies/RapidFire")]
    public class RapidFireStrategySO : FireStrategySO
    {
        public enum SpawnMethod
        {
            Pooled,        // ObjectPoolManager.Get<T>() — production path
            Instantiate    // GameObject.Instantiate + Destroy — benchmark baseline
        }

        [Header("Benchmark toggle")]
        [Tooltip("Switch projectile spawn method for performance comparison. Default = Pooled.")]
        public SpawnMethod spawnMethod = SpawnMethod.Pooled;

        private static readonly ProfilerMarker _markerPooled      = new("RapidFire.Spawn_Pooled");
        private static readonly ProfilerMarker _markerInstantiate = new("RapidFire.Spawn_Instantiate");

        public override void Execute(FireContext ctx)
        {
            if (ctx.WeaponData?.projectilePrefab == null) return;

            var prefabBase = ctx.WeaponData.projectilePrefab.GetComponent<ProjectileBase>();
            if (prefabBase == null) return;

            Vector3 spawnPos = ctx.FirePoint != null ? ctx.FirePoint.position : Vector3.zero;

            ProjectileBase proj;
            bool destroyOnFinish;

            if (spawnMethod == SpawnMethod.Pooled)
            {
                using (_markerPooled.Auto())
                {
                    proj = ObjectPoolManager.Instance.Get(prefabBase, spawnPos, Quaternion.identity);
                }
                destroyOnFinish = false;
            }
            else
            {
                using (_markerInstantiate.Auto())
                {
                    var go = Object.Instantiate(ctx.WeaponData.projectilePrefab, spawnPos, Quaternion.identity);
                    proj = go.GetComponent<ProjectileBase>();
                }
                destroyOnFinish = true;
            }

            proj.Initialize(
                ctx.WeaponData.baseDamage + ctx.LevelDamageBonus,
                ctx.WeaponData.projectileSpeed,
                ctx.WeaponData.range * ctx.AreaMultiplier,
                ctx.WeaponData.piercing,
                ctx.AimDirection,
                prefabBase,
                ctx.WeaponData.knockbackForce,
                ctx.WeaponData.knockbackDuration,
                ctx.DamageMultiplier,
                ctx.CritChance,
                destroyOnFinish);
        }
    }
}
