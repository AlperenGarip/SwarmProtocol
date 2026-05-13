using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Factories
{
    /// <summary>
    /// Centralizes pickup spawning via the object pool.
    /// All drop handlers call this instead of touching ObjectPoolManager directly.
    /// </summary>
    public static class PickupFactory
    {
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation = default)
            where T : MonoBehaviour, IPoolable
        {
            if (prefab == null || ObjectPoolManager.Instance == null) return null;
            return ObjectPoolManager.Instance.Get(prefab, position, rotation);
        }
    }
}
