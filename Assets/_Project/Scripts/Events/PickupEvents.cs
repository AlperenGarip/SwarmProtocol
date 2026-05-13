using UnityEngine;

namespace SwarmProtocol.Events
{
    public struct ChestOpenedEvent
    {
        public float StageElapsedTime;   // used for time-gate evolution check
        public Vector3 ChestPosition;
    }

    public struct OrologionActivatedEvent
    {
        public float FreezeDuration;
    }

    public struct NdujaActivatedEvent
    {
        public float OverrideDuration;
    }
}
