using System;
using Unity.Collections;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// Archetype-based entity filtering system for efficient queries.
    /// Supports component-based filtering with bitmask matching.
    /// </summary>
    public class EntityQuerySystem : IDisposable
    {
        [Flags]
        public enum ComponentType : ulong
        {
            None = 0,
            Transform = 1UL << 0,
            Health = 1UL << 1,
            Inventory = 1UL << 2,
            AI = 1UL << 3,
            Vehicle = 1UL << 4,
            Building = 1UL << 5,
            Item = 1UL << 6,
            NPC = 1UL << 7,
            Wildlife = 1UL << 8,
            Resource = 1UL << 9,
            Container = 1UL << 10,
            PowerConsumer = 1UL << 11,
            PowerProducer = 1UL << 12,
            Craftable = 1UL << 13,
            Destructible = 1UL << 14,
            Movable = 1UL << 15,
            Static = 1UL << 16,
            Dynamic = 1UL << 17,
            Living = 1UL << 18,
            Mechanical = 1UL << 19,
            Organic = 1UL << 20,
            Anomalous = 1UL << 21,
            PlayerControlled = 1UL << 22,
            FactionMember = 1UL << 23,
            Hostile = 1UL << 24,
            Friendly = 1UL << 25,
            Neutral = 1UL << 26,
            InCombat = 1UL << 27,
            Idle = 1UL << 28,
            Working = 1UL << 29,
            Sleeping = 1UL << 30,
            Dead = 1UL << 31,
            // Extended flags can continue in high bits
            Custom1 = 1UL << 32,
            Custom2 = 1UL << 33,
            Custom3 = 1UL << 34,
            Custom4 = 1UL << 35,
            Custom5 = 1UL << 36,
            Custom6 = 1UL << 37,
            Custom7 = 1UL << 38,
            Custom8 = 1UL << 39,
            Custom9 = 1UL << 40,
            Custom10 = 1UL << 41,
        }
        
        [Serializable]
        public struct EntityArchetype
        {
            public ulong EntityID;
            public ComponentType Components;
            public int ChunkIndex;
            public bool IsActive;
        }
        
        [Serializable]
        public struct QueryFilter
        {
            public ComponentType RequiredComponents; // Must have ALL of these
            public ComponentType ExcludedComponents; // Must have NONE of these
            public int? MinChunkX;
            public int? MaxChunkX;
            public int? MinChunkZ;
            public int? MaxChunkZ;
        }
        
        private NativeList<EntityArchetype> _entities;
        private NativeHashMap<ulong, int> _entityIdToIndex;
        private int _capacity;
        
        public int EntityCount => _entities.Length;
        
        public EntityQuerySystem(int capacity = 50000)
        {
            _capacity = capacity;
            _entities = new NativeList<EntityArchetype>(capacity, Allocator.Persistent);
            _entityIdToIndex = new NativeHashMap<ulong, int>(capacity, Allocator.Persistent);
        }
        
        public int RegisterEntity(ulong entityId, ComponentType components, int chunkIndex)
        {
            if (_entityIdToIndex.ContainsKey(entityId))
            {
                Debug.LogWarning($"EntityQuerySystem: Entity {entityId} already registered!");
                return _entityIdToIndex[entityId];
            }
            
            if (_entities.Length >= _capacity)
            {
                Debug.LogWarning($"EntityQuerySystem: Capacity ({_capacity}) exceeded!");
                return -1;
            }
            
            int index = _entities.Length;
            var archetype = new EntityArchetype
            {
                EntityID = entityId,
                Components = components,
                ChunkIndex = chunkIndex,
                IsActive = true
            };
            
            _entities.Add(archetype);
            _entityIdToIndex[entityId] = index;
            
            return index;
        }
        
        public void UnregisterEntity(ulong entityId)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out int index))
                return;
            
            // Mark as inactive instead of removing (preserves indices)
            var entity = _entities[index];
            entity.IsActive = false;
            _entities[index] = entity;
            
            _entityIdToIndex.Remove(entityId);
        }
        
        public void UpdateEntityComponents(ulong entityId, ComponentType components)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out int index))
                return;
            
            var entity = _entities[index];
            entity.Components = components;
            _entities[index] = entity;
        }
        
        public void UpdateEntityChunk(ulong entityId, int newChunkIndex)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out int index))
                return;
            
            var entity = _entities[index];
            entity.ChunkIndex = newChunkIndex;
            _entities[index] = entity;
        }
        
        public NativeList<int> Query(QueryFilter filter)
        {
            var results = new NativeList<int>(Allocator.Temp);
            
            for (int i = 0; i < _entities.Length; i++)
            {
                ref var entity = ref _entities.ElementAt(i);
                
                if (!entity.IsActive)
                    continue;
                
                // Check required components (must have ALL)
                if ((entity.Components & filter.RequiredComponents) != filter.RequiredComponents)
                    continue;
                
                // Check excluded components (must have NONE)
                if ((entity.Components & filter.ExcludedComponents) != 0)
                    continue;
                
                // Optional: Filter by chunk bounds
                if (filter.MinChunkX.HasValue || filter.MaxChunkX.HasValue ||
                    filter.MinChunkZ.HasValue || filter.MaxChunkZ.HasValue)
                {
                    // Would need chunk manager to resolve chunk coords from index
                    // For now, skip spatial filtering in this method
                }
                
                results.Add(i);
            }
            
            return results;
        }
        
        public EntityArchetype GetEntity(int index)
        {
            if (index < 0 || index >= _entities.Length)
                return default;
            return _entities[index];
        }
        
        public EntityArchetype? GetEntityById(ulong entityId)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out int index))
                return null;
            return _entities[index];
        }
        
        public bool HasComponent(ulong entityId, ComponentType component)
        {
            if (!_entityIdToIndex.TryGetValue(entityId, out int index))
                return false;
            
            return (_entities[index].Components & component) != 0;
        }
        
        public int CountByComponent(ComponentType component)
        {
            int count = 0;
            for (int i = 0; i < _entities.Length; i++)
            {
                if (_entities[i].IsActive && 
                    (_entities[i].Components & component) != 0)
                {
                    count++;
                }
            }
            return count;
        }
        
        public void Dispose()
        {
            _entities.Dispose();
            _entityIdToIndex.Dispose();
        }
    }
    
    /// <summary>
    /// Helper class for building common query filters.
    /// </summary>
    public static class QueryPresets
    {
        public static EntityQuerySystem.QueryFilter AllLiving => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.Living,
            ExcludedComponents = EntityQuerySystem.ComponentType.Dead
        };
        
        public static EntityQuerySystem.QueryFilter AllHostile => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.Hostile,
            ExcludedComponents = EntityQuerySystem.ComponentType.Friendly | EntityQuerySystem.ComponentType.Dead
        };
        
        public static EntityQuerySystem.QueryFilter AllNPCs => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.NPC,
            ExcludedComponents = EntityQuerySystem.ComponentType.Dead
        };
        
        public static EntityQuerySystem.QueryFilter AllVehicles => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.Vehicle,
            ExcludedComponents = EntityQuerySystem.ComponentType.Dead
        };
        
        public static EntityQuerySystem.QueryFilter AllBuildings => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.Building,
            ExcludedComponents = EntityQuerySystem.ComponentType.Movable
        };
        
        public static EntityQuerySystem.QueryFilter AllPowerConsumers => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.PowerConsumer,
            ExcludedComponents = EntityQuerySystem.ComponentType.Dead
        };
        
        public static EntityQuerySystem.QueryFilter AllResources => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.Resource,
            ExcludedComponents = EntityQuerySystem.ComponentType.None
        };
        
        public static EntityQuerySystem.QueryFilter AllInCombat => new EntityQuerySystem.QueryFilter
        {
            RequiredComponents = EntityQuerySystem.ComponentType.InCombat,
            ExcludedComponents = EntityQuerySystem.ComponentType.Dead
        };
    }
}
