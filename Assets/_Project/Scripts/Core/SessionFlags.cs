namespace SwarmProtocol.Core
{
    /// <summary>
    /// Cross-scene-reload flags. Static, so values survive a SceneManager.LoadScene call
    /// (Unity only resets statics on assembly reload, not on scene reload).
    /// </summary>
    public static class SessionFlags
    {
        /// <summary>If true, GameManager auto-enters Playing state right after the scene loads.</summary>
        public static bool AutoStartOnLoad;
    }
}
