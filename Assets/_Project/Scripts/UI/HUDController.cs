using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Core;
using SwarmProtocol.Player;
using SwarmProtocol.Progression;

namespace SwarmProtocol.UI
{
    /// <summary>
    /// HUD displaying health, XP, stage timer, gold, and kill count.
    /// Listens to EventBus — never polls components directly.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("XP")]
        [SerializeField] private Slider xpBar;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Stage")]
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI killCountText;

        [Header("Gold")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerXP playerXP;
        [SerializeField] private StageManager stageManager;

        [Header("HUD Root")]
        [SerializeField] private GameObject hudRoot;

        private void OnEnable()
        {
            EventBus.OnPlayerDamaged    += OnPlayerDamaged;
            EventBus.OnLevelUp          += OnLevelUp;
            EventBus.OnStageStart       += OnStageStart;
            EventBus.OnStageTimerTick   += OnStageTimerTick;
            EventBus.OnEnemyKilled      += OnEnemyKilled;
            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerDamaged    -= OnPlayerDamaged;
            EventBus.OnLevelUp          -= OnLevelUp;
            EventBus.OnStageStart       -= OnStageStart;
            EventBus.OnStageTimerTick   -= OnStageTimerTick;
            EventBus.OnEnemyKilled      -= OnEnemyKilled;
            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Start() => RefreshAll();

        private void LateUpdate()
        {
            if (playerXP != null && xpBar != null)
                xpBar.value = playerXP.LevelProgress;

            if (goldText != null)
                goldText.text = $"Gold: {GoldManager.Instance?.Gold ?? 0}";
        }

        // ─── Event Handlers ──────────────────────────────────────

        private void OnGameStateChanged(GameState newState)
        {
            if (hudRoot != null)
                hudRoot.SetActive(newState == GameState.Playing || newState == GameState.LevelUp);
        }

        private void OnPlayerDamaged(float amount)  => UpdateHealthBar();
        private void OnLevelUp(int newLevel)            { UpdateLevelText(); }
        private void OnStageStart(int stageNumber)      => UpdateStageText();
        private void OnStageTimerTick(float remaining)  => UpdateTimerText(remaining);
        private void OnEnemyKilled(Enemies.EnemyBase e) => UpdateKillCount();

        // ─── UI Updates ──────────────────────────────────────────

        private void RefreshAll()
        {
            UpdateHealthBar();
            UpdateXPBar();
            UpdateLevelText();
            UpdateStageText();
            UpdateKillCount();
            UpdateGoldText();
            if (stageManager != null) UpdateTimerText(stageManager.TimeRemaining);
        }

        private void UpdateHealthBar()
        {
            if (playerHealth == null) return;
            if (healthBar != null) healthBar.value = playerHealth.HealthPercent;
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(playerHealth.CurrentHP)} / {Mathf.CeilToInt(playerHealth.MaxHP)}";
        }

        private void UpdateXPBar()
        {
            if (playerXP == null || xpBar == null) return;
            xpBar.value = playerXP.LevelProgress;
        }

        private void UpdateLevelText()
        {
            if (playerXP == null || levelText == null) return;
            levelText.text = $"Lv.{playerXP.CurrentLevel}";
        }

        private void UpdateStageText()
        {
            if (stageManager == null || stageText == null) return;
            stageText.text = $"Stage {stageManager.CurrentStageNumber} / {stageManager.TotalStages}";
        }

        private void UpdateTimerText(float remaining)
        {
            if (timerText == null) return;
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{minutes:0}:{seconds:00}";
        }

        private void UpdateKillCount()
        {
            if (stageManager == null || killCountText == null) return;
            killCountText.text = $"Kills: {stageManager.TotalKills}";
        }

        private void UpdateGoldText()
        {
            if (goldText == null) return;
            goldText.text = $"Gold: {GoldManager.Instance?.Gold ?? 0}";
        }
    }
}
