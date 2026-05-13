using UnityEngine;

namespace SwarmProtocol.ScriptableObjects
{
    /// <summary>
    /// Single ScriptableObject holding all tunable game constants.
    /// No magic numbers in code — everything lives here.
    /// Create via right-click → Create → SwarmProtocol → Config → GameConfig.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "SwarmProtocol/Config/GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Weapon Slots")]
        public int maxWeaponSlots  = 6;
        public int maxPassiveSlots = 6;

        [Header("Level-Up")]
        [Range(1, 10)] public int upgradeOptionsCount = 3;
        [Range(0f, 1f)] public float skipXPPercent = 0.20f;
        public int startingRerollCharges = 1;
        public int startingSkipCharges   = 1;
        public int startingBanishCharges = 1;

        [Header("I-Frames (after upgrade pick)")]
        public float postLevelUpIFrameDuration = 1.5f;

        [Header("Knockback")]
        public float defaultKnockbackForce    = 5f;
        public float defaultKnockbackDuration = 0.25f;

        [Header("Overcharge")]
        public float overchargeDuration           = 60f;
        public float overchargeFireRateMultiplier = 1.5f;
        public float overchargeDamageMultiplier   = 1.25f;

        [Header("Jackpot Rewards")]
        public int pairGoldReward  = 50;
        public int tripleExtraLevels = 1;  // +2 total (+1 base +1 extra)

        [Header("Chest Tier Weights (1-5)")]
        public float chestTierWeight1 = 0.40f;
        public float chestTierWeight2 = 0.30f;
        public float chestTierWeight3 = 0.18f;
        public float chestTierWeight4 = 0.09f;
        public float chestTierWeight5 = 0.03f;

        [Header("Consumable Fallback")]
        [Range(0f, 1f)] public float floorChickenHealPercent = 0.30f;
        public int coinBagGoldAmount = 100;

        [Header("Special Pickups")]
        public float orologionFreezeDuration = 8f;
        public float ndujaFrittaDuration     = 10f;

        [Header("Spawn Distances")]
        public float minSpawnDistance = 10f;
        public float maxSpawnDistance = 18f;

        [Header("Enemy Health Orb Drop")]
        [Range(0, 100)] public int healthOrbDropChanceDefault = 3;  // % per kill base

        [Header("Curse Scaling")]
        public float curseSpawnIntervalReductionPerLevel = 0.05f;
        public float curseDifficultyBonusPerLevel        = 0.10f;

        [Header("XP")]
        public float xpRequirementBase   = 10f;
        public float xpRequirementGrowth = 1.15f;
    }
}
