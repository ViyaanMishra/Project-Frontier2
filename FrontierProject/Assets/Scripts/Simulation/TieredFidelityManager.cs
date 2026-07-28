using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// 4-tier LOD simulation system for performance optimization.
    /// Tier 1: Full physics + AI (nearby entities)
    /// Tier 2: Simplified physics + AI (medium distance)
    /// Tier 3: Macro ticks only (far distance)
    /// Tier 4: Frozen/sleeping (very far or unloaded)
    /// </summary>
    public class TieredFidelityManager : IDisposable
    {
        public enum SimulationTier
        {
            Tier1_Full = 0,      // Every tick, full simulation
            Tier2_Reduced = 1,   // Every 4th tick
            Tier3_Macro = 2,     // Every 16th tick
            Tier4_Frozen = 3     // No simulation
        }
        
        [Serializable]
        public struct FidelityConfig
        {
            public float Tier1Radius;      // e.g., 64 units
            public float Tier2Radius;      // e.g., 128 units
            public float Tier3Radius;      // e.g., 256 units
            public int TicksPerUpdate_T2;  // e.g., 4
            public int TicksPerUpdate_T3;  // e.g., 16
        }
        
        private static readonly FidelityConfig DefaultConfig = new FidelityConfig
        {
            Tier1Radius = 64f,
            Tier2Radius = 128f,
            Tier3Radius = 256f,
            TicksPerUpdate_T2 = 4,
            TicksPerUpdate_T3 = 16
        };
        
        private NativeArray<SimulationTier> _entityTiers;
        private NativeArray<float3> _entityPositions;
        private float3 _playerPosition;
        private FidelityConfig _config;
        private int _entityCapacity;
        private int _entityCount;
        
        public int EntityCount => _entityCount;
        public FidelityConfig Config => _config;
        
        public TieredFidelityManager(int capacity = 10000, FidelityConfig config = default)
        {
            _entityCapacity = capacity;
            _config = config.Tier1Radius > 0 ? config : DefaultConfig;
            
            _entityTiers = new NativeArray<SimulationTier>(capacity, Allocator.Persistent);
            _entityPositions = new NativeArray<float3>(capacity, Allocator.Persistent);
            _entityCount = 0;
            _playerPosition = float3.zero;
        }
        
        public int RegisterEntity(float3 position)
        {
            if (_entityCount >= _entityCapacity)
            {
                Debug.LogWarning($"TieredFidelityManager: Entity capacity ({_entityCapacity}) exceeded!");
                return -1;
            }
            
            int index = _entityCount;
            _entityPositions[index] = position;
            _entityTiers[index] = CalculateTier(position, _playerPosition);
            _entityCount++;
            
            return index;
        }
        
        public void UnregisterEntity(int index)
        {
            if (index < 0 || index >= _entityCount)
                return;
            
            // Swap with last entity
            _entityCount--;
            if (index != _entityCount)
            {
                _entityPositions[index] = _entityPositions[_entityCount];
                _entityTiers[index] = _entityTiers[_entityCount];
            }
        }
        
        public void UpdateEntityPosition(int index, float3 newPosition)
        {
            if (index < 0 || index >= _entityCount)
                return;
            
            _entityPositions[index] = newPosition;
        }
        
        public void UpdatePlayerPosition(float3 newPosition)
        {
            _playerPosition = newPosition;
            
            // Recalculate all tiers
            for (int i = 0; i < _entityCount; i++)
            {
                _entityTiers[i] = CalculateTier(_entityPositions[i], _playerPosition);
            }
        }
        
        public SimulationTier GetEntityTier(int index)
        {
            if (index < 0 || index >= _entityCount)
                return SimulationTier.Tier4_Frozen;
            return _entityTiers[index];
        }
        
        public bool ShouldUpdateEntity(int index, long currentTick)
        {
            if (index < 0 || index >= _entityCount)
                return false;
            
            SimulationTier tier = _entityTiers[index];
            
            switch (tier)
            {
                case SimulationTier.Tier1_Full:
                    return true;
                case SimulationTier.Tier2_Reduced:
                    return currentTick % _config.TicksPerUpdate_T2 == 0;
                case SimulationTier.Tier3_Macro:
                    return currentTick % _config.TicksPerUpdate_T3 == 0;
                case SimulationTier.Tier4_Frozen:
                default:
                    return false;
            }
        }
        
        private SimulationTier CalculateTier(float3 entityPos, float3 playerPos)
        {
            float distSq = math.distancesq(entityPos, playerPos);
            float t1Sq = _config.Tier1Radius * _config.Tier1Radius;
            float t2Sq = _config.Tier2Radius * _config.Tier2Radius;
            float t3Sq = _config.Tier3Radius * _config.Tier3Radius;
            
            if (distSq <= t1Sq)
                return SimulationTier.Tier1_Full;
            else if (distSq <= t2Sq)
                return SimulationTier.Tier2_Reduced;
            else if (distSq <= t3Sq)
                return SimulationTier.Tier3_Macro;
            else
                return SimulationTier.Tier4_Frozen;
        }
        
        public JobHandle ScheduleTierUpdate<T>(T jobData, JobHandle dependsOn) 
            where T : struct, IJobParallelFor
        {
            // This would be extended to schedule different jobs per tier
            // For now, returns the dependency handle
            return dependsOn;
        }
        
        public void Dispose()
        {
            if (_entityTiers.IsCreated)
                _entityTiers.Dispose();
            if (_entityPositions.IsCreated)
                _entityPositions.Dispose();
        }
    }
    
    /// <summary>
    /// Burst-compatible job for updating entity tiers in parallel.
    /// </summary>
    [BurstCompile]
    public struct UpdateEntityTiersJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> EntityPositions;
        [WriteOnly] public NativeArray<TieredFidelityManager.SimulationTier> EntityTiers;
        public float3 PlayerPosition;
        public float Tier1RadiusSq;
        public float Tier2RadiusSq;
        public float Tier3RadiusSq;
        
        public void Execute(int index)
        {
            float3 entityPos = EntityPositions[index];
            float distSq = math.distancesq(entityPos, PlayerPosition);
            
            TieredFidelityManager.SimulationTier tier;
            
            if (distSq <= Tier1RadiusSq)
                tier = TieredFidelityManager.SimulationTier.Tier1_Full;
            else if (distSq <= Tier2RadiusSq)
                tier = TieredFidelityManager.SimulationTier.Tier2_Reduced;
            else if (distSq <= Tier3RadiusSq)
                tier = TieredFidelityManager.SimulationTier.Tier3_Macro;
            else
                tier = TieredFidelityManager.SimulationTier.Tier4_Frozen;
            
            EntityTiers[index] = tier;
        }
    }
}
