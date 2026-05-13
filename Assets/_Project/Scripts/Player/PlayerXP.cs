using UnityEngine;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;

namespace SwarmProtocol.Player
{
    /// <summary>
    /// Tracks XP collection, calculates level thresholds, and fires level-up events.
    /// XP requirement scales with each level.
    /// </summary>
    public class PlayerXP : MonoBehaviour
    {
        [Header("Leveling")]
        [SerializeField] private int baseXPToLevel = 20;
        [SerializeField] private float xpScalingFactor = 1.3f; // each level requires more XP

        public int CurrentXP { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public int XPToNextLevel { get; private set; }

        /// <summary>
        /// XP progress toward next level, 0 to 1.
        /// </summary>
        public float LevelProgress => XPToNextLevel > 0 ? (float)CurrentXP / XPToNextLevel : 0f;

        private void Awake()
        {
            XPToNextLevel = baseXPToLevel;
        }

        private void OnEnable()
        {
            EventBus.OnXPCollected += AddXP;
        }

        private void OnDisable()
        {
            EventBus.OnXPCollected -= AddXP;
        }

        private void AddXP(int amount)
        {
            CurrentXP += amount;

            while (CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                CurrentLevel++;
                XPToNextLevel = Mathf.RoundToInt(baseXPToLevel * Mathf.Pow(xpScalingFactor, CurrentLevel - 1));
                EventBus.LevelUp(CurrentLevel);
                AudioService.Instance?.PlaySfx(SfxId.LevelUp);
            }
        }

        /// <summary>
        /// Resets XP and level for a new run.
        /// </summary>
        public void ResetXP()
        {
            CurrentXP = 0;
            CurrentLevel = 1;
            XPToNextLevel = baseXPToLevel;
        }
    }
}
