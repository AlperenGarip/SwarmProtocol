using UnityEngine;

namespace SwarmProtocol.Progression
{
    /// <summary>
    /// Persistent gold layer on top of GoldManager's session gold.
    /// GoldManager tracks gold earned this run; GoldService persists it across runs
    /// via PlayerPrefs for the meta-progression shop (Step 21).
    ///
    /// Call CommitSessionGold() at run end (GameOver / StageTransition after final stage)
    /// to bank the session earnings into persistent storage.
    /// </summary>
    public class GoldService : MonoBehaviour
    {
        private const string PersistentGoldKey = "SwarmProtocol_PersistentGold";

        public static GoldService Instance { get; private set; }

        public int PersistentGold => PlayerPrefs.GetInt(PersistentGoldKey, 0);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Banks all session gold from GoldManager into PlayerPrefs.
        /// Call when a run ends (death or final stage clear).
        /// </summary>
        public void CommitSessionGold()
        {
            int session = GoldManager.Instance != null ? GoldManager.Instance.Gold : 0;
            if (session <= 0) return;

            PlayerPrefs.SetInt(PersistentGoldKey, PersistentGold + session);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Spends persistent gold (meta-shop purchases).
        /// Returns false and makes no change if insufficient funds.
        /// </summary>
        public bool TrySpendPersistent(int amount)
        {
            if (PersistentGold < amount) return false;
            PlayerPrefs.SetInt(PersistentGoldKey, PersistentGold - amount);
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>For debug / testing only.</summary>
        public void ResetPersistentGold()
        {
            PlayerPrefs.SetInt(PersistentGoldKey, 0);
            PlayerPrefs.Save();
        }
    }
}
