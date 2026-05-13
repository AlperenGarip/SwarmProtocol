namespace SwarmProtocol.Core
{
    /// <summary>
    /// Interface for objects managed by the object pool.
    /// Provides lifecycle hooks for spawn/despawn instead of relying on SetActive alone.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is retrieved from the pool and placed in the scene.
        /// Use this to reset state, enable components, etc.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// Use this to disable components, cancel coroutines, etc.
        /// </summary>
        void OnDespawn();
    }
}
