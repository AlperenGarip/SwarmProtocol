using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using SwarmProtocol.Core;
using SwarmProtocol.Combat;
using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat.Strategies
{
    [CreateAssetMenu(fileName = "AuraStrategy", menuName = "SwarmProtocol/Strategies/Aura")]
    public class AuraStrategySO : FireStrategySO
    {
        public enum DetectionMethod
        {
            ActiveEnemyRegistry,        // O(N) sqrMagnitude scan, no physics
            PhysicsOverlapSphereNonAlloc // baseline for benchmarking — uses Unity Physics
        }

        [Header("Benchmark toggle")]
        [Tooltip("Switch detection method for performance comparison. Default = ActiveEnemyRegistry.")]
        public DetectionMethod detection = DetectionMethod.ActiveEnemyRegistry;

        [Tooltip("Layer mask used only when detection = PhysicsOverlapSphereNonAlloc. Should include the Enemy layer.")]
        public LayerMask physicsEnemyMask = ~0; // default: everything — set to Enemy layer in inspector

        // Profiler markers — show up as named rows in the Profiler timeline
        private static readonly ProfilerMarker _markerExecute   = new("Aura.Execute");
        private static readonly ProfilerMarker _markerRegistry  = new("Aura.RegistryQuery");
        private static readonly ProfilerMarker _markerPhysics   = new("Aura.PhysicsOverlap");
        private static readonly ProfilerMarker _markerApply     = new("Aura.ApplyDamage");

        private static readonly List<EnemyBase> _hitBuffer  = new();
        private static readonly Collider[]      _physBuffer = new Collider[256];

        public override void Execute(FireContext ctx)
        {
            using (_markerExecute.Auto())
            {
                float range    = ctx.WeaponData.range * ctx.AreaMultiplier;
                float sqrRange = range * range;

                _hitBuffer.Clear();

                if (detection == DetectionMethod.ActiveEnemyRegistry)
                {
                    using (_markerRegistry.Auto())
                    {
                        ActiveEnemyRegistry.Instance?.GetEnemiesInRange(ctx.FirePoint.position, sqrRange, _hitBuffer);
                    }
                }
                else
                {
                    using (_markerPhysics.Auto())
                    {
                        int hitCount = Physics.OverlapSphereNonAlloc(
                            ctx.FirePoint.position, range, _physBuffer, physicsEnemyMask);
                        for (int i = 0; i < hitCount; i++)
                        {
                            var eb = _physBuffer[i].GetComponent<EnemyBase>();
                            if (eb != null) _hitBuffer.Add(eb);
                        }
                    }
                }

                using (_markerApply.Auto())
                {
                    float damage = (ctx.WeaponData.baseDamage + ctx.LevelDamageBonus) * ctx.OverchargeDamageMultiplier;
                    foreach (var enemy in _hitBuffer)
                    {
                        if (enemy == null) continue;
                        DamageSystem.ApplyDamage(enemy, damage, ctx.DamageMultiplier, ctx.CritChance, ctx.FirePoint.position,
                            ctx.WeaponData.knockbackForce, ctx.WeaponData.knockbackDuration);
                    }
                }
            }
        }
    }
}
