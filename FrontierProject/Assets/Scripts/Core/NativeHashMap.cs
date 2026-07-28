using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Frontier.Core
{
    /// <summary>
    /// Burst-compatible hash map using native arrays.
    /// Provides O(1) average lookup for entity data in job systems.
    /// </summary>
    /// <typeparam name="TKey">Key type (must be unmanaged)</typeparam>
    /// <typeparam name="TValue">Value type (must be unmanaged)</typeparam>
    public unsafe struct NativeHashMap<TKey, TValue> : IDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private uint* _buckets;
        
        [NativeDisableUnsafePtrRestriction]
        private int* _next;
        
        [NativeDisableUnsafePtrRestriction]
        private TKey* _keys;
        
        [NativeDisableUnsafePtrRestriction]
        private TValue* _values;
        
        private int _capacity;
        private int _count;
        private int _freeList;
        private Allocator _allocator;
        private bool _isCreated;

        public int Count => _count;
        public int Capacity => _capacity;
        public bool IsCreated => _isCreated;
        public bool IsEmpty => _count == 0;

        public NativeHashMap(int capacity, Allocator allocator = Allocator.TempJob)
        {
            if (capacity < 8) capacity = 8;
            
            // Round up to power of 2 for faster hashing
            int pot = 8;
            while (pot < capacity) pot <<= 1;
            capacity = pot;

            _capacity = capacity;
            _count = 0;
            _freeList = -1;
            _allocator = allocator;
            _isCreated = true;

            _buckets = (uint*)UnsafeUtility.Malloc(sizeof(uint) * _capacity, UnsafeUtility.AlignOf<uint>(), allocator);
            _next = (int*)UnsafeUtility.Malloc(sizeof(int) * _capacity, UnsafeUtility.AlignOf<int>(), allocator);
            _keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * _capacity, UnsafeUtility.AlignOf<TKey>(), allocator);
            _values = (TValue*)UnsafeUtility.Malloc(sizeof(TValue) * _capacity, UnsafeUtility.AlignOf<TValue>(), allocator);

            // Initialize buckets to -1 (empty)
            for (int i = 0; i < _capacity; i++)
            {
                _buckets[i] = uint.MaxValue;
                _next[i] = -1;
            }
        }

        /// <summary>
        /// Add or update a key-value pair.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (!_isCreated) throw new InvalidOperationException("HashMap not created");

            uint hash = Hash(key);
            int bucket = (int)(hash & (_capacity - 1));

            // Check if key already exists
            int index = (int)_buckets[bucket];
            while (index != -1)
            {
                if (_keys[index].Equals(key))
                {
                    _values[index] = value; // Update existing
                    return;
                }
                index = _next[index];
            }

            // Get new index from free list or allocate
            int newIndex;
            if (_freeList != -1)
            {
                newIndex = _freeList;
                _freeList = _next[newIndex];
            }
            else if (_count < _capacity)
            {
                newIndex = _count;
            }
            else
            {
                // HashMap is full - in production you'd grow here
                throw new OverflowException($"NativeHashMap is full at capacity {_capacity}");
            }

            // Insert at head of chain
            _keys[newIndex] = key;
            _values[newIndex] = value;
            _next[newIndex] = (int)_buckets[bucket];
            _buckets[bucket] = (uint)newIndex;
            _count++;
        }

        /// <summary>
        /// Try to get a value by key.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;
            if (!_isCreated) return false;

            uint hash = Hash(key);
            int bucket = (int)(hash & (_capacity - 1));

            int index = (int)_buckets[bucket];
            while (index != -1)
            {
                if (_keys[index].Equals(key))
                {
                    value = _values[index];
                    return true;
                }
                index = _next[index];
            }

            return false;
        }

        /// <summary>
        /// Remove a key-value pair.
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!_isCreated) return false;

            uint hash = Hash(key);
            int bucket = (int)(hash & (_capacity - 1));

            int prevIndex = -1;
            int index = (int)_buckets[bucket];

            while (index != -1)
            {
                if (_keys[index].Equals(key))
                {
                    // Remove from chain
                    if (prevIndex == -1)
                    {
                        _buckets[bucket] = (uint)_next[index];
                    }
                    else
                    {
                        _next[prevIndex] = _next[index];
                    }

                    // Add to free list
                    _next[index] = _freeList;
                    _freeList = index;
                    _count--;

                    return true;
                }
                prevIndex = index;
                index = _next[index];
            }

            return false;
        }

        /// <summary>
        /// Check if key exists.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            if (!_isCreated) return false;

            uint hash = Hash(key);
            int bucket = (int)(hash & (_capacity - 1));

            int index = (int)_buckets[bucket];
            while (index != -1)
            {
                if (_keys[index].Equals(key))
                {
                    return true;
                }
                index = _next[index];
            }

            return false;
        }

        /// <summary>
        /// Clear all entries.
        /// </summary>
        public void Clear()
        {
            if (!_isCreated) return;

            for (int i = 0; i < _capacity; i++)
            {
                _buckets[i] = uint.MaxValue;
                _next[i] = -1;
            }

            _count = 0;
            _freeList = -1;
        }

        /// <summary>
        /// Get enumerator for iteration.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator
        {
            private readonly NativeHashMap<TKey, TValue> _map;
            private int _index;

            internal Enumerator(NativeHashMap<TKey, TValue> map)
            {
                _map = map;
                _index = -1;
            }

            public bool MoveNext()
            {
                _index++;
                while (_index < _map._capacity && _map._buckets[_index & (_map._capacity - 1)] == uint.MaxValue)
                {
                    _index++;
                }
                return _index < _map._capacity;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    int bucket = _index & (_map._capacity - 1);
                    int slotIndex = (int)_map._buckets[bucket];
                    return new KeyValuePair<TKey, TValue>(_map._keys[slotIndex], _map._values[slotIndex]);
                }
            }
        }

        /// <summary>
        /// Simple hash function for unmanaged types.
        /// </summary>
        private static uint Hash(TKey key)
        {
            // FNV-1a hash variant
            uint hash = 2166136261u;
            unsafe
            {
                byte* ptr = (byte*)&key;
                for (int i = 0; i < sizeof(TKey); i++)
                {
                    hash ^= ptr[i];
                    hash *= 16777619u;
                }
            }
            return hash;
        }

        public void Dispose()
        {
            if (!_isCreated) return;

            if (_buckets != null)
            {
                UnsafeUtility.Free(_buckets, _allocator);
                _buckets = null;
            }
            if (_next != null)
            {
                UnsafeUtility.Free(_next, _allocator);
                _next = null;
            }
            if (_keys != null)
            {
                UnsafeUtility.Free(_keys, _allocator);
                _keys = null;
            }
            if (_values != null)
            {
                UnsafeUtility.Free(_values, _allocator);
                _values = null;
            }

            _isCreated = false;
            _count = 0;
            _capacity = 0;
        }
    }

    /// <summary>
    /// Simplified integer-keyed native hash map for entity lookups.
    /// </summary>
    public unsafe struct NativeIntMap<TValue> : IDisposable where TValue : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private TValue* _entries;
        private bool* _occupied;
        private int _capacity;
        private int _count;
        private Allocator _allocator;
        private bool _isCreated;

        public int Count => _count;
        public bool IsCreated => _isCreated;

        public NativeIntMap(int capacity, Allocator allocator = Allocator.TempJob)
        {
            _capacity = capacity;
            _count = 0;
            _allocator = allocator;
            _isCreated = true;

            _entries = (TValue*)UnsafeUtility.Malloc(sizeof(TValue) * _capacity, UnsafeUtility.AlignOf<TValue>(), allocator);
            _occupied = (bool*)UnsafeUtility.Malloc(sizeof(bool) * _capacity, UnsafeUtility.AlignOf<bool>(), allocator);

            for (int i = 0; i < _capacity; i++)
            {
                _occupied[i] = false;
            }
        }

        public void Add(int key, TValue value)
        {
            if (!_isCreated) throw new InvalidOperationException("Map not created");
            
            int index = key & (_capacity - 1);
            
            // Linear probing
            while (_occupied[index])
            {
                index = (index + 1) & (_capacity - 1);
            }

            _entries[index] = value;
            _occupied[index] = true;
            _count++;
        }

        public bool TryGet(int key, out TValue value)
        {
            value = default;
            if (!_isCreated) return false;

            int index = key & (_capacity - 1);
            int startIndex = index;

            while (_occupied[index])
            {
                if (index == key & (_capacity - 1))
                {
                    value = _entries[index];
                    return true;
                }
                index = (index + 1) & (_capacity - 1);
                
                if (index == startIndex) break;
            }

            return false;
        }

        public void Clear()
        {
            if (!_isCreated) return;

            for (int i = 0; i < _capacity; i++)
            {
                _occupied[i] = false;
            }
            _count = 0;
        }

        public void Dispose()
        {
            if (!_isCreated) return;

            if (_entries != null)
            {
                UnsafeUtility.Free(_entries, _allocator);
                _entries = null;
            }
            if (_occupied != null)
            {
                UnsafeUtility.Free(_occupied, _allocator);
                _occupied = null;
            }

            _isCreated = false;
        }
    }
}
