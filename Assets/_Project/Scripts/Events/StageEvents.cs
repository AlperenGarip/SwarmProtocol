using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Events
{
    public struct StageStartEvent
    {
        public int          StageNumber;
        public float        StageDuration;
        public StageDataSO  StageData;
    }

    public struct StageCompleteEvent
    {
        public int StageNumber;
        public int TotalKills;
        public int GoldEarned;
    }

    public struct StageTimerTickEvent
    {
        public float Remaining;
        public float Elapsed;
        public float StageDuration;
    }
}
