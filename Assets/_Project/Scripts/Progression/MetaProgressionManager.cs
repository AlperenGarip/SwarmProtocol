using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Stats;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression
{
    public class MetaProgressionManager : MonoBehaviour
    {
        public static MetaProgressionManager Instance { get; private set; }

        [SerializeField] private List<PowerUpDataSO> allPowerUps = new();
        [SerializeField] private PowerUpDataSO rerollPowerUp;
        [SerializeField] private PowerUpDataSO skipPowerUp;
        [SerializeField] private PowerUpDataSO banishPowerUp;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public IReadOnlyList<PowerUpDataSO> AllPowerUps => allPowerUps;

        public int GetRank(PowerUpDataSO data)
        {
            if (data == null) return 0;
            return PlayerPrefs.GetInt(Key(data), 0);
        }

        public bool IsMaxRank(PowerUpDataSO data) => data != null && GetRank(data) >= data.maxRank;

        public int GetCost(PowerUpDataSO data) => data == null ? 0 : data.baseCost * (GetRank(data) + 1);

        public bool TryPurchase(PowerUpDataSO data)
        {
            if (data == null || IsMaxRank(data)) return false;
            if (GoldService.Instance == null || !GoldService.Instance.TrySpendPersistent(GetCost(data))) return false;
            PlayerPrefs.SetInt(Key(data), GetRank(data) + 1);
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>Adds PowerUp stat bonuses on top of existing base values (call from PlayerStats.RebuildChain).</summary>
        public void ApplyStatBonuses(Dictionary<StatType, float> baseValues)
        {
            foreach (var pu in allPowerUps)
            {
                if (pu == null || pu.isChargeType || pu.affectedStat == StatType.None) continue;
                int rank = GetRank(pu);
                if (rank <= 0) continue;
                float bonus = pu.valuePerRank * rank;
                if (!baseValues.ContainsKey(pu.affectedStat))
                    baseValues[pu.affectedStat] = 0f;
                if (pu.isPercentage)
                    baseValues[pu.affectedStat] *= (1f + bonus);
                else
                    baseValues[pu.affectedStat] += bonus;
            }
        }

        public int ExtraRerollCharges => rerollPowerUp != null ? GetRank(rerollPowerUp) * rerollPowerUp.chargesPerRank : 0;
        public int ExtraSkipCharges   => skipPowerUp   != null ? GetRank(skipPowerUp)   * skipPowerUp.chargesPerRank   : 0;
        public int ExtraBanishCharges => banishPowerUp != null ? GetRank(banishPowerUp) * banishPowerUp.chargesPerRank : 0;

        public void ResetAllRanks()
        {
            foreach (var pu in allPowerUps)
                if (pu != null) PlayerPrefs.DeleteKey(Key(pu));
            PlayerPrefs.Save();
        }

        private static string Key(PowerUpDataSO data) => $"SwarmProtocol_PU_{data.powerUpName}_Rank";
    }
}
