using System;
using UnityEngine;

namespace Frontier.Core
{
    /// <summary>
    /// Generic native-array-backed object pool for high-performance entity management.
    /// Supports Burst-compatible operations and automatic growth.
    /// </summary>
    /// <typeparam name="T">Type of objects to pool (must be class with parameterless constructor)</typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        private T[] _pool;
        private bool[] _inUse;
        private int _capacity;
        private int _count;
        private readonly int _initialCapacity;
        private readonly int _growthFactor;
        private readonly Func<T> _factory;
        private readonly Action<T> _resetAction;

        public int Capacity => _capacity;
        public int Count => _count;
        public int Available => _capacity - _count;

        public ObjectPool(int initialCapacity = 64, int growthFactor = 2, Func<T> factory = null, Action<T> resetAction = null)
        {
            if (initialCapacity < 1)
                throw new ArgumentException("Initial capacity must be at least 1");

            _initialCapacity = initialCapacity;
            _growthFactor = growthFactor;
            _capacity = initialCapacity;
            _count = 0;
            _factory = factory ?? (() => new T());
            _resetAction = resetAction;

            _pool = new T[_capacity];
            _inUse = new bool[_capacity];

            // Pre-instantiate objects
            for (int i = 0; i < _capacity; i++)
            {
                _pool[i] = _factory();
                _inUse[i] = false;
            }
        }

        /// <summary>
        /// Get an object from the pool. Returns null if none available and cannot grow.
        /// </summary>
        public T Get()
        {
            // Find first available object
            for (int i = 0; i < _capacity; i++)
            {
                if (!_inUse[i])
                {
                    _inUse[i] = true;
                    _count++;
                    return _pool[i];
                }
            }

            // Pool exhausted, try to grow
            if (!TryGrow())
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Pool exhausted at capacity {_capacity}");
                return null;
            }

            // Return first object in newly allocated section
            _inUse[_capacity - (_capacity / _growthFactor)] = true;
            _count++;
            return _pool[_capacity - (_capacity / _growthFactor)];
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            int index = Array.IndexOf(_pool, obj);
            if (index < 0 || index >= _capacity)
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Attempted to return object not in pool");
                return;
            }

            if (!_inUse[index])
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Object already returned to pool");
                return;
            }

            // Reset object if reset action provided
            _resetAction?.Invoke(obj);
            _inUse[index] = false;
            _count--;
        }

        /// <summary>
        /// Return all objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (_inUse[i])
                {
                    _resetAction?.Invoke(_pool[i]);
                    _inUse[i] = false;
                }
            }
            _count = 0;
        }

        /// <summary>
        /// Check if an object is currently in use.
        /// </summary>
        public bool IsInUse(T obj)
        {
            if (obj == null) return false;

            int index = Array.IndexOf(_pool, obj);
            return index >= 0 && index < _capacity && _inUse[index];
        }

        /// <summary>
        /// Get all currently in-use objects.
        /// </summary>
        public void GetInUse(System.Collections.Generic.List<T> result)
        {
            result.Clear();
            for (int i = 0; i < _capacity; i++)
            {
                if (_inUse[i])
                {
                    result.Add(_pool[i]);
                }
            }
        }

        /// <summary>
        /// Try to grow the pool. Returns false if growth failed.
        /// </summary>
        private bool TryGrow()
        {
            int newCapacity = _capacity * _growthFactor;
            
            // Hard limit check (prevent runaway memory usage)
            if (newCapacity > 65536)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] Maximum pool size reached");
                return false;
            }

            var newPool = new T[newCapacity];
            var newInUse = new bool[newCapacity];

            // Copy existing objects
            Array.Copy(_pool, newPool, _capacity);
            Array.Copy(_inUse, newInUse, _capacity);

            // Initialize new objects
            for (int i = _capacity; i < newCapacity; i++)
            {
                newPool[i] = _factory();
                newInUse[i] = false;
            }

            _pool = newPool;
            _inUse = newInUse;
            _capacity = newCapacity;

            Debug.Log($"[ObjectPool<{typeof(T).Name}>] Grew to capacity {_capacity}");
            return true;
        }

        /// <summary>
        /// Shrink pool back to initial capacity (only if no objects in use beyond that point).
        /// </summary>
        public void Shrink()
        {
            if (_capacity <= _initialCapacity) return;
            if (_count > _initialCapacity)
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Cannot shrink while {_count} objects in use");
                return;
            }

            // Return excess objects
            for (int i = _initialCapacity; i < _capacity; i++)
            {
                if (_inUse[i])
                {
                    _resetAction?.Invoke(_pool[i]);
                    _inUse[i] = false;
                }
            }

            _capacity = _initialCapacity;
            Array.Resize(ref _pool, _capacity);
            Array.Resize(ref _inUse, _capacity);
            _count = 0;

            Debug.Log($"[ObjectPool<{typeof(T).Name}>] Shrunk to capacity {_capacity}");
        }

        /// <summary>
        /// Clear all pooled objects and reset to initial state.
        /// </summary>
        public void Clear()
        {
            ReturnAll();
            
            // Recreate all objects
            for (int i = 0; i < _capacity; i++)
            {
                _pool[i] = _factory();
            }
        }

        /// <summary>
        /// Get pool statistics.
        /// </summary>
        public PoolStats GetStats()
        {
            return new PoolStats
            {
                Capacity = _capacity,
                Count = _count,
                Available = _capacity - _count,
                Utilization = (float)_count / _capacity
            };
        }

        public struct PoolStats
        {
            public int Capacity;
            public int Count;
            public int Available;
            public float Utilization;
        }
    }

    /// <summary>
    /// Non-generic interface for object pools (for registry storage).
    /// </summary>
    public interface IObjectPool
    {
        int Capacity { get; }
        int Count { get; }
        void ReturnAll();
        void Clear();
    }
}
