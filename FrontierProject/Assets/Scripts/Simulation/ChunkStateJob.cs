using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Frontier.Simulation
{
    /// <summary>
    /// IJobParallelFor for updating chunk state in parallel.
    /// Processes entity simulation, resource updates, and environmental effects.
    /// </summary>
    [BurstCompile]
    public struct ChunkStateJob : IJobParallelFor
    {
        // Input: Chunk data
        [ReadOnly] public NativeArray<ChunkManager.ChunkInfo> Chunks;
        [ReadOnly] public int ActiveChunkCount;
        
        // Input: Simulation parameters
        [ReadOnly] public long CurrentTick;
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float3 PlayerPosition;
        
        // Input/Output: Entity data per chunk (simplified representation)
        public NativeArray<int> EntityCounts;
        public NativeArray<float> ResourceLevels;
        public NativeArray<float> TemperatureValues;
        public NativeArray<float> ToxicityValues;
        
        // Output: State changes
        public NativeList<int> DirtyChunkIndices;
        
        // Configuration
        [ReadOnly] public float AmbientTemperature;
        [ReadOnly] public float BaseToxicity;
        [ReadOnly] public float ResourceRegenRate;
        
        public void Execute(int index)
        {
            if (index >= ActiveChunkCount)
                return;
            
            ref var chunk = ref Chunks[index];
            
            // Skip unloaded chunks
            if (chunk.State != ChunkManager.ChunkState.Active)
                return;
            
            bool isDirty = false;
            
            // Update resource levels with regeneration
            float currentResource = ResourceLevels[index];
            if (currentResource < 100f)
            {
                float regen = ResourceRegenRate * DeltaTime;
                ResourceLevels[index] = math.min(100f, currentResource + regen);
                isDirty = true;
            }
            
            // Update temperature based on biome, time, and player proximity
            float temp = CalculateTemperature(index, chunk, CurrentTick);
            if (math.abs(temp - TemperatureValues[index]) > 0.1f)
            {
                TemperatureValues[index] = temp;
                isDirty = true;
            }
            
            // Update toxicity (dispersal over time)
            float tox = CalculateToxicity(index, chunk);
            if (math.abs(tox - ToxicityValues[index]) > 0.01f)
            {
                ToxicityValues[index] = tox;
                isDirty = true;
            }
            
            // Mark chunk as dirty if any state changed
            if (isDirty)
            {
                lock (DirtyChunkIndices)
                {
                    DirtyChunkIndices.Add(index);
                }
            }
        }
        
        private float CalculateTemperature(int index, ref ChunkManager.ChunkInfo chunk, long tick)
        {
            // Base ambient temperature
            float temp = AmbientTemperature;
            
            // Day/night cycle effect (24-hour cycle at 60 ticks/sec = 86400 ticks)
            float dayProgress = (tick % 86400) / 86400f;
            float dayNightFactor = math.sin(dayProgress * math.PI * 2f - math.PI / 2f); // Peak at noon
            temp += dayNightFactor * 10f; // +/- 10 degrees
            
            // Distance from player (heat from activity)
            float3 chunkCenter = new float3(
                chunk.ChunkX * ChunkManager.ChunkSize + ChunkManager.ChunkSize / 2f,
                0f,
                chunk.ChunkZ * ChunkManager.ChunkSize + ChunkManager.ChunkSize / 2f
            );
            float distSq = math.distancesq(chunkCenter, PlayerPosition);
            if (distSq < 1000f) // Within ~32 units
            {
                temp += (1f - distSq / 1000f) * 5f; // Up to +5 degrees near player
            }
            
            return temp;
        }
        
        private float CalculateToxicity(int index, ref ChunkManager.ChunkInfo chunk)
        {
            float tox = BaseToxicity;
            
            // Toxicity dispersal over time
            float dispersionRate = 0.001f * DeltaTime;
            tox = math.max(BaseToxicity, tox - dispersionRate);
            
            // Entity contribution (more entities = more pollution)
            int entityCount = EntityCounts[index];
            if (entityCount > 0)
            {
                tox += entityCount * 0.01f;
            }
            
            return math.min(100f, tox);
        }
    }
    
    /// <summary>
    /// Job for processing entity updates within chunks.
    /// </summary>
    [BurstCompile]
    public struct EntityUpdateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> ChunkEntityStarts;
        [ReadOnly] public NativeArray<int> ChunkEntityCounts;
        
        // Entity component data (flattened arrays)
        public NativeArray<float3> Positions;
        public NativeArray<float3> Velocities;
        public NativeArray<float> Health;
        public NativeArray<int> States;
        
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float Gravity;
        
        public void Execute(int entityIndex)
        {
            // Get entity position
            float3 pos = Positions[entityIndex];
            float3 vel = Velocities[entityIndex];
            float hp = Health[entityIndex];
            int state = States[entityIndex];
            
            // Apply gravity
            vel.y -= Gravity * DeltaTime;
            
            // Simple integration
            pos += vel * DeltaTime;
            
            // Ground collision (simplified - assumes y=0 is ground)
            if (pos.y < 0f)
            {
                pos.y = 0f;
                vel.y = 0f;
            }
            
            // Write back
            Positions[entityIndex] = pos;
            Velocities[entityIndex] = vel;
            Health[entityIndex] = hp;
            States[entityIndex] = state;
        }
    }
    
    /// <summary>
    /// Job for calculating chunk load priorities in parallel.
    /// </summary>
    [BurstCompile]
    public struct ChunkPriorityJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ChunkManager.ChunkInfo> Chunks;
        [WriteOnly] public NativeArray<float> Priorities;
        [ReadOnly] public int2 PlayerChunkCoord;
        [ReadOnly] public int ViewDistance;
        
        public void Execute(int index)
        {
            ref var chunk = ref Chunks[index];
            
            int dx = chunk.ChunkX - PlayerChunkCoord.x;
            int dz = chunk.ChunkZ - PlayerChunkCoord.y;
            int distSq = dx * dx + dz * dz;
            
            // Priority is inverse distance squared
            Priorities[index] = distSq;
        }
    }
}
