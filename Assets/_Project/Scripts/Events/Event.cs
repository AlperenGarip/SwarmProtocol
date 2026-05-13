using System;

namespace SwarmProtocol.Events
{
    /// <summary>
    /// Generic typed event channel. Each event type T is a separate static channel,
    /// fully orthogonal — adding a new event means creating a new struct file only.
    /// T must be a struct so event data is stack-allocated (no GC pressure).
    /// </summary>
    public static class Event<T> where T : struct
    {
        private static event Action<T> _listeners;

        public static void Subscribe(Action<T> listener)   => _listeners += listener;
        public static void Unsubscribe(Action<T> listener) => _listeners -= listener;
        public static void Fire(T data)                    => _listeners?.Invoke(data);

        /// <summary>Clears all subscriptions. Call via EventSystem.ResetAll() on restart.</summary>
        public static void Reset() => _listeners = null;
    }

    /// <summary>
    /// Central reset point — clears every typed event channel.
    /// Call on game-over restart to prevent stale MonoBehaviour subscriptions.
    /// </summary>
    public static class EventSystem
    {
        public static void ResetAll()
        {
            // Combat
            Event<EnemyKilledEvent>.Reset();
            Event<PlayerDamagedEvent>.Reset();
            Event<PlayerDeathEvent>.Reset();

            // Progression
            Event<XPCollectedEvent>.Reset();
            Event<LevelUpEvent>.Reset();
            Event<GoldCollectedEvent>.Reset();
            Event<WeaponLeveledUpEvent>.Reset();
            Event<WeaponEvolvedEvent>.Reset();
            Event<PassiveAddedEvent>.Reset();
            Event<PassiveAppliedEvent>.Reset();
            Event<OverchargeTriggeredEvent>.Reset();

            // Stage
            Event<StageStartEvent>.Reset();
            Event<StageCompleteEvent>.Reset();
            Event<StageTimerTickEvent>.Reset();

            // Pickups
            Event<ChestOpenedEvent>.Reset();
            Event<OrologionActivatedEvent>.Reset();
            Event<NdujaActivatedEvent>.Reset();

            // Game State
            Event<GameStateChangedEvent>.Reset();

            // Stats
            Event<StatsChangedEvent>.Reset();
        }
    }
}
