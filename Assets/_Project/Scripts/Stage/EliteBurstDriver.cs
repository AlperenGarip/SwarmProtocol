using UnityEngine;
using SwarmProtocol.Enemies;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Stage
{
    /// <summary>
    /// Orthogonal listener to StageStartEvent. Fires the elite burst exactly once
    /// when TimeRemaining drops to or below StageDataSO.eliteBurstTimeBeforeEnd.
    /// </summary>
    public class EliteBurstDriver : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private StageTimer   stageTimer;
        [SerializeField] private Color        eliteTint = new Color(0.9f, 0.2f, 0.2f);

        private StageDataSO _stageData;
        private bool        _isActive;
        private bool        _burstFired;

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
            _stageData  = e.StageData;
            _isActive   = true;
            _burstFired = false;
        }

        private void OnStageComplete(StageCompleteEvent e) => _isActive = false;

        private void Update()
        {
            if (!_isActive || _burstFired || _stageData?.eliteEnemyData == null || stageTimer == null) return;

            if (stageTimer.TimeRemaining <= _stageData.eliteBurstTimeBeforeEnd)
            {
                _burstFired = true;
                for (int i = 0; i < _stageData.eliteBurstCount; i++)
                {
                    var enemy = enemySpawner?.Spawn(_stageData.eliteEnemyData, _stageData.difficultyMultiplier);
                    enemy?.ApplyTint(eliteTint);
                }
            }
        }
    }
}
