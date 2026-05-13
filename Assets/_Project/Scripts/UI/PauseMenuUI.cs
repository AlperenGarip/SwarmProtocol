using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SwarmProtocol.Core;

namespace SwarmProtocol.UI
{
    /// <summary>
    /// Pause menu panel. Escape toggles pause during Playing/Paused states.
    /// Resume and Quit-to-Menu buttons wired to GameManager.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitToMenuButton;

        private void OnEnable()  { EventBus.OnGameStateChanged += OnGameStateChanged; }
        private void OnDisable() { EventBus.OnGameStateChanged -= OnGameStateChanged; }

        private void Start()
        {
            resumeButton?.onClick.AddListener(() => GameManager.Instance?.TogglePause());
            quitToMenuButton?.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
            pausePanel?.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                var state = GameManager.Instance?.CurrentState;
                if (state == GameState.Playing || state == GameState.Paused)
                    GameManager.Instance.TogglePause();
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            pausePanel?.SetActive(newState == GameState.Paused);
        }
    }
}
