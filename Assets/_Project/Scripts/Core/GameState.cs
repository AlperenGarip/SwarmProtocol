namespace SwarmProtocol.Core
{
    /// <summary>
    /// All possible game states for the FSM.
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        LevelUp,          // timeScale=0 — upgrade selection
        ChestOpen,        // timeScale=0 — two-stage slot machine
        Paused,           // timeScale=0 — escape menu
        StageTransition,  // brief stats screen between stages
        GameOver,
        Victory           // timeScale=0 — final stage cleared, run completed
    }
}
