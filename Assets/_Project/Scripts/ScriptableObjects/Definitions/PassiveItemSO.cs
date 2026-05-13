using UnityEngine;
using SwarmProtocol.Stats;

namespace SwarmProtocol.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewPassive", menuName = "SwarmProtocol/Passive")]
    public class PassiveItemSO : ScriptableObject
    {
        [Header("Display")]
        public string passiveName;
        [TextArea] public string description;
        public Sprite icon;
        public UpgradeRarity rarity = UpgradeRarity.Common;

        [Header("Effect")]
        public StatType statType;
        public float value;        // flat amount, or fraction if isPercentage (0.1 = +10%)
        public bool isPercentage;  // true = multiplicative (1 + value), false = additive
    }
}
