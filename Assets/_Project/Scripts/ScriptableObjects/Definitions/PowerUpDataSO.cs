using UnityEngine;
using SwarmProtocol.Stats;

namespace SwarmProtocol.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewPowerUp", menuName = "SwarmProtocol/PowerUp")]
    public class PowerUpDataSO : ScriptableObject
    {
        public string powerUpName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Stats")]
        public StatType affectedStat = StatType.None;
        public int maxRank = 5;
        public float valuePerRank;
        public bool isPercentage;
        public int baseCost;

        [Header("Charge Type")]
        public bool isChargeType;
        public int chargesPerRank = 2;
    }
}
