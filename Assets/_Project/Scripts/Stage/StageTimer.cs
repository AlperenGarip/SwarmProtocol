using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Stage
{
    /// <summary>
    /// Owns the stage countdown. Fires StageStartEvent, per-second StageTimerTickEvent,
    /// and StageCompleteEvent. Nothing else — all other stage logic is orthogonal drivers.
    /// </summary>
    public class StageTimer : MonoBehaviour
    {
        public float TimeRemaining { get; private set; }
        public float ElapsedTime   { get; private set; }
        public float StageDuration { get; private set; }
        public bool  IsRunning     { get; private set; }

        private int   _stageNumber;
        private float _tickAccumulator;

        public void StartStage(StageDataSO data, int stageNumber)
        {
            StageDuration    = data.stageDuration;
            TimeRemaining    = data.stageDuration;
            ElapsedTime      = 0f;
            _stageNumber     = stageNumber;
            _tickAccumulator = 0f;
            IsRunning        = true;

            Event<StageStartEvent>.Fire(new StageStartEvent
            {
                StageNumber   = stageNumber,
                StageDuration = data.stageDuration,
                StageData     = data
            });
            EventBus.StageStarted(stageNumber);
        }

        public void StopStage()
        {
            IsRunning = false;
        }

        private void Update()
        {
            if (!IsRunning) return;

            ElapsedTime   += Time.deltaTime;
            TimeRemaining  = Mathf.Max(StageDuration - ElapsedTime, 0f);

            _tickAccumulator += Time.deltaTime;
            if (_tickAccumulator >= 1f)
            {
                _tickAccumulator -= 1f;
                EventBus.StageTimerTick(TimeRemaining);
                Event<StageTimerTickEvent>.Fire(new StageTimerTickEvent
                {
                    Remaining     = TimeRemaining,
                    Elapsed       = ElapsedTime,
                    StageDuration = StageDuration
                });
            }

            if (TimeRemaining <= 0f)
            {
                IsRunning = false;
                Event<StageCompleteEvent>.Fire(new StageCompleteEvent
                {
                    StageNumber = _stageNumber,
                    TotalKills  = 0,
                    GoldEarned  = 0
                });
                EventBus.StageCompleted(_stageNumber);
            }
        }
    }
}
