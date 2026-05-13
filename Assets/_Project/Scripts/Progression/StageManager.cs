using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Enemies;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression
{
    /// <summary>
    /// Thin stage orchestrator. Owns the stage list and current index.
    /// Tells StageTimer to start/stop; all spawn logic lives in orthogonal driver components.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [Header("Stages")]
        [SerializeField] private List<StageDataSO> stages;

        [Header("References")]
        [SerializeField] private SwarmProtocol.Stage.StageTimer stageTimer;

        [Header("Transition")]
        [Tooltip("Real-time seconds to pause between stages before auto-advancing.")]
        [SerializeField] private float transitionDuration = 3f;

        public int   CurrentStageIndex  { get; private set; }
        public int   CurrentStageNumber => CurrentStageIndex + 1;
        public int   TotalStages        => stages != null ? stages.Count : 0;
        public float TimeRemaining      => stageTimer != null ? stageTimer.TimeRemaining : 0f;
        public int   TotalKills         { get; private set; }
        public bool  IsStageActive      => stageTimer != null && stageTimer.IsRunning;

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleGameStateChanged;
            EventBus.OnEnemyKilled      += HandleEnemyKilled;
            Event<StageCompleteEvent>.Subscribe(HandleStageComplete);
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleGameStateChanged;
            EventBus.OnEnemyKilled      -= HandleEnemyKilled;
            Event<StageCompleteEvent>.Unsubscribe(HandleStageComplete);
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    CurrentStageIndex = 0;
                    TotalKills        = 0;
                    stageTimer?.StopStage();
                    break;

                case GameState.Playing when !IsStageActive:
                    StartCurrentStage();
                    break;
            }
        }

        private void HandleEnemyKilled(EnemyBase _) => TotalKills++;

        private void HandleStageComplete(StageCompleteEvent e)
        {
            CurrentStageIndex++;

            if (CurrentStageIndex >= TotalStages)
            {
                // Final stage cleared — go to Victory screen instead of straight to menu.
                GameManager.Instance?.EnterVictory();
                return;
            }

            GameManager.Instance?.EnterStageTransition();
            // timeScale=0 during transition — must use real-time wait
            StartCoroutine(AutoAdvanceAfterDelay());
        }

        private IEnumerator AutoAdvanceAfterDelay()
        {
            yield return new WaitForSecondsRealtime(transitionDuration);
            // Guard: only resume if we're still in transition (player may have quit etc.)
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.StageTransition)
            {
                GameManager.Instance.StartNextStage();
            }
        }

        private void StartCurrentStage()
        {
            if (stages == null || CurrentStageIndex >= stages.Count) return;
            stageTimer?.StartStage(stages[CurrentStageIndex], CurrentStageNumber);
        }
    }
}
