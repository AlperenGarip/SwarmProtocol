using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Progression;
using SwarmProtocol.Progression.Commands;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Chest
{
    public static class ChestRewardResolver
    {
        public static int RollTier(GameConfigSO config)
        {
            if (config == null) return 1;
            float[] weights =
            {
                config.chestTierWeight1,
                config.chestTierWeight2,
                config.chestTierWeight3,
                config.chestTierWeight4,
                config.chestTierWeight5,
            };

            float sum = 0f;
            foreach (var w in weights) sum += w;

            float roll       = Random.Range(0f, sum);
            float cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative) return i + 1;
            }
            return 1;
        }

        public static List<IUpgradeCommand> RollItems(int columnCount)
            => UpgradeManager.Instance?.RollChestOptions(columnCount) ?? new List<IUpgradeCommand>();
    }
}
