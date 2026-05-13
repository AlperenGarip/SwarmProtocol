using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;

namespace SwarmProtocol.UI
{
    /// <summary>
    /// Game over screen. Displays stage reached, total kills, and gold collected.
    /// Restart restarts from Stage 1; Menu returns to main menu.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("References")]
        [SerializeField] private StageManager stageManager;

        // NOTE: subscribe in Awake/OnDestroy (not OnEnable/OnDisable) because this script lives
        // ON the GameOverPanel — once Start deactivates the panel, OnDisable would unsubscribe
        // and the death event would never trigger the show.
        private void Awake()     { EventBus.OnGameStateChanged += OnGameStateChanged; }
        private void OnDestroy() { EventBus.OnGameStateChanged -= OnGameStateChanged; }

        private void Start()
        {
            // Restart and Menu both reload the scene — gives a clean reset of every system
            // (player HP, weapons, enemies, drops, stage timer, etc.) without each one needing
            // its own bespoke "reset on Menu" hook.
            restartButton?.onClick.AddListener(Restart);
            menuButton?.onClick.AddListener(BackToMenu);
            gameOverPanel?.SetActive(false);
        }

        private void Restart()
        {
            AudioService.Instance?.PlaySfx(SfxId.UIClick);
            SessionFlags.AutoStartOnLoad = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void BackToMenu()
        {
            AudioService.Instance?.PlaySfx(SfxId.UIClick);
            SessionFlags.AutoStartOnLoad = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private float _runStartTime;
        private float _runEndTime;

        private void OnGameStateChanged(GameState newState)
        {
            // Track session start so we can show how long the player survived.
            if (newState == GameState.Playing && _runStartTime <= 0f)
                _runStartTime = Time.unscaledTime;
            else if (newState == GameState.Menu)
                _runStartTime = 0f;

            bool show = newState == GameState.GameOver;
            if (show) _runEndTime = Time.unscaledTime;
            gameOverPanel?.SetActive(show);
            if (show) PopulateStats();
        }

        private void PopulateStats()
        {
            if (stageManager != null)
            {
                if (stageText != null)
                    stageText.text = $"Stage Reached: {stageManager.CurrentStageNumber} / {stageManager.TotalStages}";
                if (killCountText != null)
                    killCountText.text = $"Kills: {stageManager.TotalKills}";
            }

            if (goldText != null)
                goldText.text = $"Gold Collected: {GoldManager.Instance?.Gold ?? 0}";

            if (timeText != null)
            {
                float elapsed = _runStartTime > 0f ? Mathf.Max(0f, _runEndTime - _runStartTime) : 0f;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                timeText.text = $"Time Survived: {minutes:D2}:{seconds:D2}";
            }
        }
    }
}
