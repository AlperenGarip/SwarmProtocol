using System;
using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// Static event bus for decoupled system communication.
    /// All major game events flow through here so systems don't need direct references.
    /// </summary>
    public static class EventBus
    {
        // ─── Combat ───────────────────────────────────────────────
        public static event Action<EnemyBase> OnEnemyKilled;
        public static event Action<float>     OnPlayerDamaged;
        public static event Action            OnPlayerDeath;

        // ─── Progression ──────────────────────────────────────────
        public static event Action<int>                  OnXPCollected;
        public static event Action<int>                  OnLevelUp;
        public static event Action<int>                  OnGoldCollected;
        public static event Action<WeaponDataSO, int>    OnWeaponLeveledUp;   // weapon, newLevel
        public static event Action<UpgradeDataSO>        OnUpgradeSelected;   // passive/stat upgrade picked

        // ─── Stage ────────────────────────────────────────────────
        public static event Action<int>   OnStageStart;       // stageNumber
        public static event Action<int>   OnStageComplete;    // stageNumber
        public static event Action<float> OnStageTimerTick;   // remainingSeconds

        // ─── Game State ───────────────────────────────────────────
        public static event Action<GameState> OnGameStateChanged;

        // ─── Invoke Methods ───────────────────────────────────────

        public static void EnemyKilled(EnemyBase enemy)          => OnEnemyKilled?.Invoke(enemy);
        public static void PlayerDamaged(float amount)            => OnPlayerDamaged?.Invoke(amount);
        public static void PlayerDied()                           => OnPlayerDeath?.Invoke();

        public static void XPCollected(int amount)                 => OnXPCollected?.Invoke(amount);
        public static void LevelUp(int newLevel)                   => OnLevelUp?.Invoke(newLevel);
        public static void GoldCollected(int amount)               => OnGoldCollected?.Invoke(amount);
        public static void WeaponLeveledUp(WeaponDataSO w, int l)  => OnWeaponLeveledUp?.Invoke(w, l);
        public static void UpgradeSelected(UpgradeDataSO upgrade)   => OnUpgradeSelected?.Invoke(upgrade);

        public static void StageStarted(int stageNumber)          => OnStageStart?.Invoke(stageNumber);
        public static void StageCompleted(int stageNumber)        => OnStageComplete?.Invoke(stageNumber);
        public static void StageTimerTick(float remaining)        => OnStageTimerTick?.Invoke(remaining);

        public static void GameStateChanged(GameState newState)   => OnGameStateChanged?.Invoke(newState);

        /// <summary>
        /// Clears all event subscriptions. Call on game restart to prevent stale references.
        /// </summary>
        public static void ClearAll()
        {
            OnEnemyKilled      = null;
            OnPlayerDamaged    = null;
            OnPlayerDeath      = null;
            OnXPCollected      = null;
            OnLevelUp          = null;
            OnGoldCollected    = null;
            OnWeaponLeveledUp  = null;
            OnUpgradeSelected  = null;
            OnStageStart       = null;
            OnStageComplete    = null;
            OnStageTimerTick   = null;
            OnGameStateChanged = null;
        }
    }
}
