using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Enemies;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Stage
{
    /// <summary>
    /// Orthogonal listener to StageStartEvent. Drives bracket-based continuous spawning
    /// for the duration of a stage. Reads ElapsedTime from StageTimer each frame.
    /// </summary>
    public class StageSpawnDriver : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private StageTimer   stageTimer;

        private StageDataSO      _stageData;
        private bool             _isActive;
        private readonly List<float> _bracketTimers = new();

        public float SpawnIntervalMultiplier { get; set; } = 1f;

        private void OnEnable()
        {
            Event<StageStartEvent>.Subscribe(OnStageStart);
            Event<StageCompleteEvent>.Subscribe(OnStageComplete);
            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            Event<StageStartEvent>.Unsubscribe(OnStageStart);
            Event<StageCompleteEvent>.Unsubscribe(OnStageComplete);
            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnStageStart(StageStartEvent e)
        {
            _stageData = e.StageData;
            _isActive  = true;
            _bracketTimers.Clear();
            foreach (var _ in _stageData.spawnBrackets)
                _bracketTimers.Add(0f);
        }

        private void OnStageComplete(StageCompleteEvent e) => _isActive = false;

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver || state == GameState.Menu)
                _isActive = false;
        }

        private void Update()
        {
            if (!_isActive || _stageData == null || stageTimer == null) return;

            float elapsed = stageTimer.ElapsedTime;
            var brackets  = _stageData.spawnBrackets;

            for (int i = 0; i < brackets.Count; i++)
            {
                var bracket = brackets[i];
                if (elapsed < bracket.fromTime) continue;

                _bracketTimers[i] -= Time.deltaTime;
                if (_bracketTimers[i] <= 0f)
                {
                    for (int j = 0; j < bracket.spawnCount; j++)
                        enemySpawner?.Spawn(bracket.enemyData, _stageData.difficultyMultiplier);
                    _bracketTimers[i] = bracket.spawnInterval * SpawnIntervalMultiplier;
                }
            }
        }
    }
}
