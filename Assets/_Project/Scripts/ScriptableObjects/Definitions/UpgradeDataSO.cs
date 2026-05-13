using UnityEngine;

namespace SwarmProtocol.ScriptableObjects
{
    public enum UpgradeType
    {
        DamageUp,
        FireRateUp,
        SpeedUp,
        MaxHPUp,
        Piercing,
        AoE,
        SlowAura,
        MagnetRange,
        CritChance,
        Regeneration
    }

    public enum UpgradeRarity
    {
        Common,
        Rare,
        Epic
    }

    /// <summary>
    /// Data container for upgrade configuration.
    /// Create instances via right-click → Create → SwarmProtocol → Upgrade.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "SwarmProtocol/Upgrade")]
    public class UpgradeDataSO : ScriptableObject
    {
        [Header("General")]
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Effect")]
        public UpgradeType upgradeType;
        public float value;             // modifier amount
        public bool isPercentage;       // true = multiplicative, false = additive

        [Header("Rarity & Synergy")]
        public UpgradeRarity rarity = UpgradeRarity.Common;
        public string[] synergyTags;    // e.g., ["fire", "rate"] — matching tags trigger synergy bonuses
    }
}
