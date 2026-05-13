using System.Collections.Generic;
using UnityEngine;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// Generic object pool for MonoBehaviour components that implement IPoolable.
    /// Pre-warms a configurable number of instances and grows on demand.
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new();
        private readonly HashSet<T> _inUse = new();

        public int CountAvailable => _available.Count;
        public int CountInUse => _inUse.Count;
        public int CountTotal => CountAvailable + CountInUse;

        public ObjectPool(T prefab, Transform parent, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < prewarmCount; i++)
            {
                T instance = CreateInstance();
                instance.gameObject.SetActive(false);
                _available.Enqueue(instance);
            }
        }

        /// <summary>
        /// Gets an object from the pool. Creates a new one if the pool is empty.
        /// </summary>
        public T Get()
        {
            T instance;

            if (_available.Count > 0)
            {
                instance = _available.Dequeue();
            }
            else
            {
                instance = CreateInstance();
            }

            instance.gameObject.SetActive(true);
            instance.OnSpawn();
            _inUse.Add(instance);
            return instance;
        }

        /// <summary>
        /// Gets an object from the pool and sets its position/rotation.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        public void Return(T instance)
        {
            if (instance == null) return;
            if (!_inUse.Remove(instance)) return; // wasn't from this pool or already returned

            instance.OnDespawn();
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_parent);
            _available.Enqueue(instance);
        }

        /// <summary>
        /// Returns all in-use objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            // Copy to avoid modifying collection during iteration
            var inUseCopy = new List<T>(_inUse);
            foreach (var instance in inUseCopy)
            {
                Return(instance);
            }
        }

        private T CreateInstance()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.name = $"{_prefab.name}_pooled";
            return instance;
        }
    }
}
