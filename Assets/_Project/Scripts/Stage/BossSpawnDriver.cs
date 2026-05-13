using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Enemies;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Stage
{
    /// <summary>
    /// Orthogonal listener to StageStartEvent. Spawns bosses at fixed elapsed-time
    /// timestamps defined in StageDataSO.bossSpawns. Each index fires exactly once.
    /// </summary>
    public class BossSpawnDriver : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private StageTimer   stageTimer;

        private StageDataSO      _stageData;
        private bool             _isActive;
        private readonly HashSet<int> _firedIndices = new();

        private void OnEnable()
        {
            Event<StageStartEvent>.Subscribe(OnStageStart);
            Event<StageCompleteEvent>.Subscribe(OnStageComplete);
        }

        private void OnDisable()
        {
            Event<StageStartEvent>.Unsubscribe(OnStageStart);
            Event<StageCompleteEvent>.Unsubscribe(OnStageComplete);
        }

        private void OnStageStart(StageStartEvent e)
        {
            _stageData = e.StageData;
            _isActive  = true;
            _firedIndices.Clear();
        }

        private void OnStageComplete(StageCompleteEvent e) => _isActive = false;

        private void Update()
        {
            if (!_isActive || _stageData == null || stageTimer == null) return;

            float elapsed = stageTimer.ElapsedTime;
            for (int i = 0; i < _stageData.bossSpawns.Count; i++)
            {
                if (_firedIndices.Contains(i)) continue;

                var boss = _stageData.bossSpawns[i];
                if (elapsed >= boss.atTime && boss.bossEnemy != null)
                {
                    _firedIndices.Add(i);
                    enemySpawner?.Spawn(boss.bossEnemy, _stageData.difficultyMultiplier);
                }
            }
        }
    }
}
