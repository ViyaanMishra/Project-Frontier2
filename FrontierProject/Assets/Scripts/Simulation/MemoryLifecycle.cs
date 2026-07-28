using System;
using Unity.Collections;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// Tracks entity birth, death, and serialization state.
    /// Manages entity lifecycle events and cleanup.
    /// </summary>
    public class MemoryLifecycle : IDisposable
    {
        public enum EntityState
        {
            None,
            Born,
            Active,
            Dying,
            Dead,
            Serialized,
            Deserialized
        }
        
        [Serializable]
        public struct EntityLifecycleRecord
        {
            public ulong EntityGUID_High;
            public ulong EntityGUID_Low;
            public EntityState State;
            public long BirthTick;
            public long DeathTick;
            public long LastSerializedTick;
            public int SerializationVersion;
            public bool IsPersistent; // Should survive scene transitions
            public bool IsDirty; // Needs serialization
        }
        
        private NativeHashMap<ulong, EntityLifecycleRecord> _records;
        private NativeList<ulong> _deadEntities;
        private NativeList<ulong> _dirtyEntities;
        private long _currentTick;
        private int _maxRecords;
        
        public int ActiveEntityCount => _records.Count() - _deadEntities.Length;
        public int DeadEntityCount => _deadEntities.Length;
        public int DirtyEntityCount => _dirtyEntities.Length;
        
        public MemoryLifecycle(int capacity = 10000)
        {
            _maxRecords = capacity;
            _records = new NativeHashMap<ulong, EntityLifecycleRecord>(capacity, Allocator.Persistent);
            _deadEntities = new NativeList<ulong>(capacity, Allocator.Persistent);
            _dirtyEntities = new NativeList<ulong>(capacity, Allocator.Persistent);
            _currentTick = 0;
        }
        
        public void SetCurrentTick(long tick)
        {
            _currentTick = tick;
        }
        
        public ulong RegisterBirth(ulong guidHigh, ulong guidLow, bool isPersistent = false)
        {
            ulong key = guidHigh ^ guidLow; // Simple hash combination
            
            if (_records.ContainsKey(key))
            {
                Debug.LogWarning($"MemoryLifecycle: Entity {key} already registered!");
                return key;
            }
            
            if (_records.Count() >= _maxRecords)
            {
                Debug.LogWarning($"MemoryLifecycle: Capacity ({_maxRecords}) exceeded! Consider cleanup.");
                // Force cleanup of dead entities
                ProcessDeaths();
            }
            
            var record = new EntityLifecycleRecord
            {
                EntityGUID_High = guidHigh,
                EntityGUID_Low = guidLow,
                State = EntityState.Born,
                BirthTick = _currentTick,
                DeathTick = -1,
                LastSerializedTick = -1,
                SerializationVersion = 1,
                IsPersistent = isPersistent,
                IsDirty = true
            };
            
            _records[key] = record;
            _dirtyEntities.Add(key);
            
            EventBus.Publish(new OnEntityBorn
            {
                EntityGUID_High = guidHigh,
                EntityGUID_Low = guidLow,
                Tick = _currentTick
            });
            
            return key;
        }
        
        public void MarkActive(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return;
            
            record.State = EntityState.Active;
            _records[key] = record;
        }
        
        public void MarkDying(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return;
            
            record.State = EntityState.Dying;
            record.IsDirty = true;
            _records[key] = record;
            
            if (!_dirtyEntities.Contains(key))
                _dirtyEntities.Add(key);
        }
        
        public void RegisterDeath(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return;
            
            record.State = EntityState.Dead;
            record.DeathTick = _currentTick;
            record.IsDirty = true;
            _records[key] = record;
            
            _deadEntities.Add(key);
            
            EventBus.Publish(new OnEntityDied
            {
                EntityGUID_High = record.EntityGUID_High,
                EntityGUID_Low = record.EntityGUID_Low,
                Tick = _currentTick,
                Lifetime = _currentTick - record.BirthTick
            });
        }
        
        public void MarkSerialized(ulong key, int version)
        {
            if (!_records.TryGetValue(key, out var record))
                return;
            
            record.State = EntityState.Serialized;
            record.LastSerializedTick = _currentTick;
            record.SerializationVersion = version;
            record.IsDirty = false;
            _records[key] = record;
            
            // Remove from dirty list
            for (int i = 0; i < _dirtyEntities.Length; i++)
            {
                if (_dirtyEntities[i] == key)
                {
                    _dirtyEntities.RemoveAtSwapBack(i);
                    break;
                }
            }
        }
        
        public void MarkDeserialized(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return;
            
            record.State = EntityState.Deserialized;
            _records[key] = record;
            
            EventBus.Publish(new OnEntityDeserialized
            {
                EntityGUID_High = record.EntityGUID_High,
                EntityGUID_Low = record.EntityGUID_Low,
                Tick = _currentTick
            });
        }
        
        public void MarkDirty(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return;
            
            record.IsDirty = true;
            _records[key] = record;
            
            if (!_dirtyEntities.Contains(key))
                _dirtyEntities.Add(key);
        }
        
        public EntityLifecycleRecord GetRecord(ulong key)
        {
            if (_records.TryGetValue(key, out var record))
                return record;
            return default;
        }
        
        public bool IsAlive(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return false;
            return record.State != EntityState.Dead;
        }
        
        public bool IsDirty(ulong key)
        {
            if (!_records.TryGetValue(key, out var record))
                return false;
            return record.IsDirty;
        }
        
        public NativeList<ulong> GetDirtyEntities()
        {
            return _dirtyEntities;
        }
        
        public void ProcessDeaths()
        {
            for (int i = _deadEntities.Length - 1; i >= 0; i--)
            {
                ulong key = _deadEntities[i];
                
                if (_records.TryGetValue(key, out var record))
                {
                    // Only remove non-persistent entities or after persistence timeout
                    if (!record.IsPersistent)
                    {
                        _records.Remove(key);
                        _deadEntities.RemoveAtSwapBack(i);
                        
                        EventBus.Publish(new OnEntityCleanup
                        {
                            EntityGUID_High = record.EntityGUID_High,
                            EntityGUID_Low = record.EntityGUID_Low
                        });
                    }
                }
                else
                {
                    _deadEntities.RemoveAtSwapBack(i);
                }
            }
        }
        
        public void CleanupAllDead()
        {
            ProcessDeaths();
        }
        
        public void Dispose()
        {
            _records.Dispose();
            _deadEntities.Dispose();
            _dirtyEntities.Dispose();
        }
    }
    
    /// <summary>
    /// Event fired when an entity is born.
    /// </summary>
    public struct OnEntityBorn
    {
        public ulong EntityGUID_High;
        public ulong EntityGUID_Low;
        public long Tick;
    }
    
    /// <summary>
    /// Event fired when an entity dies.
    /// </summary>
    public struct OnEntityDied
    {
        public ulong EntityGUID_High;
        public ulong EntityGUID_Low;
        public long Tick;
        public long Lifetime;
    }
    
    /// <summary>
    /// Event fired when an entity is deserialized.
    /// </summary>
    public struct OnEntityDeserialized
    {
        public ulong EntityGUID_High;
        public ulong EntityGUID_Low;
        public long Tick;
    }
    
    /// <summary>
    /// Event fired when an entity is cleaned up from memory.
    /// </summary>
    public struct OnEntityCleanup
    {
        public ulong EntityGUID_High;
        public ulong EntityGUID_Low;
    }
}
