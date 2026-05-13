using System.Collections.Generic;
using UnityEngine;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// Singleton that manages multiple object pools keyed by prefab.
    /// Auto-creates pools on first request for a given prefab.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private int defaultPrewarmCount = 10;

        private readonly Dictionary<int, object> _pools = new(); // keyed by prefab instance ID
        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _poolRoot = new GameObject("[ObjectPools]").transform;
            _poolRoot.SetParent(transform);
        }

        /// <summary>
        /// Gets an object from the pool for the given prefab.
        /// Creates the pool if it doesn't exist yet.
        /// </summary>
        public T Get<T>(T prefab) where T : MonoBehaviour, IPoolable
        {
            var pool = GetOrCreatePool(prefab);
            return pool.Get();
        }

        /// <summary>
        /// Gets an object from the pool with a specific position and rotation.
        /// </summary>
        public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable
        {
            var pool = GetOrCreatePool(prefab);
            return pool.Get(position, rotation);
        }

        /// <summary>
        /// Returns an object to its pool.
        /// </summary>
        public void Return<T>(T prefab, T instance) where T : MonoBehaviour, IPoolable
        {
            int key = prefab.GetInstanceID();
            if (_pools.TryGetValue(key, out var poolObj) && poolObj is ObjectPool<T> pool)
            {
                pool.Return(instance);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] No pool found for {prefab.name}. Destroying instead.");
                Destroy(instance.gameObject);
            }
        }

        /// <summary>
        /// Returns all active instances to their pools.
        /// Useful for wave reset or scene transitions.
        /// </summary>
        public void ReturnAll<T>(T prefab) where T : MonoBehaviour, IPoolable
        {
            int key = prefab.GetInstanceID();
            if (_pools.TryGetValue(key, out var poolObj) && poolObj is ObjectPool<T> pool)
            {
                pool.ReturnAll();
            }
        }

        /// <summary>
        /// Pre-warms a pool for a specific prefab with a given count.
        /// Call this during loading to avoid runtime allocations.
        /// </summary>
        public void Prewarm<T>(T prefab, int count) where T : MonoBehaviour, IPoolable
        {
            int key = prefab.GetInstanceID();
            if (!_pools.ContainsKey(key))
            {
                var parent = CreatePoolParent(prefab.name);
                var pool = new ObjectPool<T>(prefab, parent, count);
                _pools[key] = pool;
            }
        }

        private ObjectPool<T> GetOrCreatePool<T>(T prefab) where T : MonoBehaviour, IPoolable
        {
            int key = prefab.GetInstanceID();
            if (_pools.TryGetValue(key, out var poolObj) && poolObj is ObjectPool<T> pool)
            {
                return pool;
            }

            var parent = CreatePoolParent(prefab.name);
            var newPool = new ObjectPool<T>(prefab, parent, defaultPrewarmCount);
            _pools[key] = newPool;
            return newPool;
        }

        private Transform CreatePoolParent(string prefabName)
        {
            var parent = new GameObject($"Pool_{prefabName}").transform;
            parent.SetParent(_poolRoot);
            return parent;
        }
    }
}
