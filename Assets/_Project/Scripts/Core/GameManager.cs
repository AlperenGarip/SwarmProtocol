using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// Singleton GameManager responsible for game state FSM transitions.
    /// VS-style: LevelUp replaces Shop/WaveTransition — triggered on every player level-up.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState  { get; private set; } = GameState.Menu;
        public GameState PreviousState { get; private set; } = GameState.Menu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            EventBus.OnPlayerDeath += HandlePlayerDeath;
            EventBus.OnLevelUp     += HandleLevelUp;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // First-time scene entry — sceneLoaded already fired before Start runs.
            EnterFreshScene();
        }

        private void OnDestroy()
        {
            EventBus.OnPlayerDeath -= HandlePlayerDeath;
            EventBus.OnLevelUp     -= HandleLevelUp;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => EnterFreshScene();

        /// <summary>
        /// Resets to Menu (or jumps straight to Playing if SessionFlags.AutoStartOnLoad is set).
        /// Always force-fires the GameStateChanged event so newly-spawned scene subscribers see it.
        /// </summary>
        private void EnterFreshScene()
        {
            bool autoStart = SessionFlags.AutoStartOnLoad;
            SessionFlags.AutoStartOnLoad = false;

            // Force the state event to fire even if we're "already" in this state from before reload.
            CurrentState = GameState.GameOver; // sentinel so TransitionTo() doesn't early-out
            TransitionTo(autoStart ? GameState.Playing : GameState.Menu);
        }

        private void Update()
        {
            // Temporary shortcut: Space to start from Menu
            if (CurrentState == GameState.Menu && Keyboard.current.spaceKey.wasPressedThisFrame)
                StartGame();

            // Unity quirk: once the cursor has been Locked, clicking inside the Game view
            // re-applies the lock even after switching to None. For any non-Playing state
            // (where the player needs to click UI), re-assert the unlocked/visible cursor
            // every frame so a stray click can't trap it.
            if (CurrentState != GameState.Playing)
            {
                if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
                if (!Cursor.visible)                          Cursor.visible   = true;
            }
        }

        // ─── Public State Transitions ─────────────────────────────

        /// <summary>Called by Main Menu "Start" button or Space shortcut.</summary>
        public void StartGame()
        {
            if (CurrentState != GameState.Menu) return;
            TransitionTo(GameState.Playing);
        }

        /// <summary>Called by UpgradeManager after the player picks an upgrade.</summary>
        public void ResumeFromLevelUp()
        {
            if (CurrentState != GameState.LevelUp) return;
            TransitionTo(GameState.Playing);
        }

        /// <summary>Called by TreasureChest on player interaction.</summary>
        public void EnterChestOpen()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionTo(GameState.ChestOpen);
        }

        /// <summary>Called by ChestSlotMachine when rewards are fully distributed.</summary>
        public void ResumeFromChestOpen()
        {
            if (CurrentState != GameState.ChestOpen) return;
            TransitionTo(GameState.Playing);
        }

        /// <summary>Called by StageManager when a stage timer expires.</summary>
        public void EnterStageTransition()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionTo(GameState.StageTransition);
        }

        /// <summary>Called by StageTransitionUI when the player is ready for the next stage.</summary>
        public void StartNextStage()
        {
            if (CurrentState != GameState.StageTransition) return;
            TransitionTo(GameState.Playing);
        }

        /// <summary>Toggle pause from anywhere during Playing.</summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
                TransitionTo(GameState.Paused);
            else if (CurrentState == GameState.Paused)
                TransitionTo(GameState.Playing);
        }

        /// <summary>Return to main menu from any state.</summary>
        public void ReturnToMenu()
        {
            TransitionTo(GameState.Menu);
        }

        /// <summary>Called by StageManager after the final stage completes — shows victory screen.</summary>
        public void EnterVictory()
        {
            TransitionTo(GameState.Victory);
        }

        // ─── Event Handlers ───────────────────────────────────────

        private void HandlePlayerDeath()
        {
            TransitionTo(GameState.GameOver);
        }

        private void HandleLevelUp(int newLevel)
        {
            if (CurrentState == GameState.Playing)
                TransitionTo(GameState.LevelUp);
        }

        // ─── State Transition Logic ────────────────────────────────

        private void TransitionTo(GameState newState)
        {
            if (newState == CurrentState) return;

            OnExitState(CurrentState);
            PreviousState = CurrentState;
            CurrentState  = newState;
            OnEnterState(newState);

            EventBus.GameStateChanged(newState);
        }

        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible   = false;
                    break;

                case GameState.LevelUp:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    break;

                case GameState.ChestOpen:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    break;

                case GameState.StageTransition:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    GoldService.Instance?.CommitSessionGold();
                    break;

                case GameState.Victory:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                    GoldService.Instance?.CommitSessionGold();
                    break;
            }
        }

        private void OnExitState(GameState state) { }
    }
}
