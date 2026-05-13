using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.Factories;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Stage
{
    public class CurseScalingDriver : MonoBehaviour
    {
        [SerializeField] private StageSpawnDriver spawnDriver;
        [SerializeField] private GameConfigSO     config;

        private void OnEnable()  => Event<StatsChangedEvent>.Subscribe(OnStatsChanged);
        private void OnDisable() => Event<StatsChangedEvent>.Unsubscribe(OnStatsChanged);

        private void OnStatsChanged(StatsChangedEvent e)
        {
            float curse = e.CurseLevel;
            float bonusPerLevel     = config != null ? config.curseDifficultyBonusPerLevel        : 0.10f;
            float reductionPerLevel = config != null ? config.curseSpawnIntervalReductionPerLevel : 0.05f;

            EnemyFactory.GlobalDifficultyBonus = curse * bonusPerLevel;

            if (spawnDriver != null)
                spawnDriver.SpawnIntervalMultiplier = 1f / (1f + curse * reductionPerLevel);
        }
    }
}
