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
    /// Shown after the final stage timer expires. Displays run stats + Restart/Menu buttons.
    /// Mirrors GameOverUI's structure (panel-on-self host → subscribes in Awake/OnDestroy).
    /// </summary>
    public class VictoryUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject victoryPanel;

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

        private float _runStartTime;
        private float _runEndTime;

        // Subscribe in Awake (not OnEnable) — same fix as GameOverUI: this script lives on
        // the panel that toggles itself inactive, so OnDisable would unsubscribe forever.
        private void Awake()     { EventBus.OnGameStateChanged += OnGameStateChanged; }
        private void OnDestroy() { EventBus.OnGameStateChanged -= OnGameStateChanged; }

        private void Start()
        {
            restartButton?.onClick.AddListener(Restart);
            menuButton?.onClick.AddListener(BackToMenu);
            victoryPanel?.SetActive(false);
        }

        private void OnGameStateChanged(GameState newState)
        {
            // Track session start so we can show how long the run took.
            if (newState == GameState.Playing && _runStartTime <= 0f)
                _runStartTime = Time.unscaledTime;
            else if (newState == GameState.Menu)
                _runStartTime = 0f;

            bool show = newState == GameState.Victory;
            if (show) _runEndTime = Time.unscaledTime;
            victoryPanel?.SetActive(show);
            if (show) PopulateStats();
        }

        private void PopulateStats()
        {
            if (stageManager != null)
            {
                if (stageText != null)
                    stageText.text = $"All {stageManager.TotalStages} Stages Cleared!";
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
                timeText.text = $"Time: {minutes:D2}:{seconds:D2}";
            }
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
    }
}
