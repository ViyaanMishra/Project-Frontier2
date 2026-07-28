using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// Manages 32x32 chunk grid over 512x512 world with load/unload lifecycle.
    /// Total: 256 chunks (16x16 grid of chunks).
    /// </summary>
    public class ChunkManager : IDisposable
    {
        public const int WorldSize = 512;
        public const int ChunkSize = 32;
        public const int ChunksPerAxis = WorldSize / ChunkSize; // 16
        public const int TotalChunks = ChunksPerAxis * ChunksPerAxis; // 256
        
        public enum ChunkState
        {
            Unloaded,
            Loading,
            Active,
            Unloading
        }
        
        [Serializable]
        public struct ChunkInfo
        {
            public int ChunkX;
            public int ChunkZ;
            public ChunkState State;
            public int EntityCount;
            public long LastAccessedTick;
            public bool IsDirty;
            public float LoadPriority;
        }
        
        private NativeArray<ChunkInfo> _chunks;
        private NativeHashMap<int2, int> _chunkCoordToIndex;
        private int _viewDistanceChunks;
        private int _currentCenterX;
        private int _currentCenterZ;
        
        public int ViewDistanceChunks
        {
            get => _viewDistanceChunks;
            set => _viewDistanceChunks = Mathf.Clamp(value, 1, ChunksPerAxis / 2);
        }
        
        public NativeArray<ChunkInfo> Chunks => _chunks;
        public int ActiveChunkCount { get; private set; }
        
        public ChunkManager(int viewDistance = 5)
        {
            _chunks = new NativeArray<ChunkInfo>(TotalChunks, Allocator.Persistent);
            _chunkCoordToIndex = new NativeHashMap<int2, int>(TotalChunks, Allocator.Persistent);
            _viewDistanceChunks = viewDistance;
            
            Initialize();
        }
        
        private void Initialize()
        {
            for (int i = 0; i < TotalChunks; i++)
            {
                int x = i % ChunksPerAxis;
                int z = i / ChunksPerAxis;
                
                _chunks[i] = new ChunkInfo
                {
                    ChunkX = x,
                    ChunkZ = z,
                    State = ChunkState.Unloaded,
                    EntityCount = 0,
                    LastAccessedTick = 0,
                    IsDirty = false,
                    LoadPriority = float.MaxValue
                };
                
                _chunkCoordToIndex[new int2(x, z)] = i;
            }
            
            ActiveChunkCount = 0;
        }
        
        public int GetChunkIndex(int worldX, int worldZ)
        {
            int chunkX = Mathf.Clamp(worldX / ChunkSize, 0, ChunksPerAxis - 1);
            int chunkZ = Mathf.Clamp(worldZ / ChunkSize, 0, ChunksPerAxis - 1);
            return chunkZ * ChunksPerAxis + chunkX;
        }
        
        public int GetChunkIndexFromCoords(int chunkX, int chunkZ)
        {
            chunkX = Mathf.Clamp(chunkX, 0, ChunksPerAxis - 1);
            chunkZ = Mathf.Clamp(chunkZ, 0, ChunksPerAxis - 1);
            return chunkZ * ChunksPerAxis + chunkX;
        }
        
        public ref ChunkInfo GetChunkAt(int worldX, int worldZ)
        {
            int index = GetChunkIndex(worldX, worldZ);
            return ref _chunks[index];
        }
        
        public ref ChunkInfo GetChunkByCoords(int chunkX, int chunkZ)
        {
            int index = GetChunkIndexFromCoords(chunkX, chunkZ);
            return ref _chunks[index];
        }
        
        public void UpdateViewCenter(int worldX, int worldZ, long currentTick)
        {
            _currentCenterX = worldX / ChunkSize;
            _currentCenterZ = worldZ / ChunkSize;
            
            // Calculate load priorities for all chunks
            for (int i = 0; i < TotalChunks; i++)
            {
                ref ChunkInfo chunk = ref _chunks[i];
                int dx = chunk.ChunkX - _currentCenterX;
                int dz = chunk.ChunkZ - _currentCenterZ;
                chunk.LoadPriority = dx * dx + dz * dz;
                _chunks[i] = chunk;
            }
            
            // Sort by priority (simple bubble for now, could use NativeSort)
            // In production, use a job to sort indices
            ProcessChunkLoading(currentTick);
        }
        
        private void ProcessChunkLoading(long currentTick)
        {
            for (int i = 0; i < TotalChunks; i++)
            {
                ref ChunkInfo chunk = ref _chunks[i];
                int dx = chunk.ChunkX - _currentCenterX;
                int dz = chunk.ChunkZ - _currentCenterZ;
                int distSq = dx * dx + dz * dz;
                int viewDistSq = _viewDistanceChunks * _viewDistanceChunks;
                
                if (distSq <= viewDistSq)
                {
                    // Should be loaded
                    if (chunk.State == ChunkState.Unloaded)
                    {
                        chunk.State = ChunkState.Loading;
                        // Trigger async load event
                        EventBus.Publish(new OnChunkLoadRequested
                        {
                            ChunkX = chunk.ChunkX,
                            ChunkZ = chunk.ChunkZ,
                            ChunkIndex = i
                        });
                    }
                    else if (chunk.State == ChunkState.Loading)
                    {
                        // Wait for async load to complete
                    }
                    else if (chunk.State == ChunkState.Active)
                    {
                        chunk.LastAccessedTick = currentTick;
                    }
                }
                else
                {
                    // Should be unloaded
                    if (chunk.State == ChunkState.Active && chunk.EntityCount == 0)
                    {
                        chunk.State = ChunkState.Unloading;
                        EventBus.Publish(new OnChunkUnloadRequested
                        {
                            ChunkX = chunk.ChunkX,
                            ChunkZ = chunk.ChunkZ,
                            ChunkIndex = i
                        });
                    }
                }
                
                _chunks[i] = chunk;
            }
        }
        
        public void MarkChunkLoaded(int chunkIndex)
        {
            if (chunkIndex >= 0 && chunkIndex < TotalChunks)
            {
                ref ChunkInfo chunk = ref _chunks[chunkIndex];
                if (chunk.State == ChunkState.Loading)
                {
                    chunk.State = ChunkState.Active;
                    ActiveChunkCount++;
                    _chunks[chunkIndex] = chunk;
                    
                    EventBus.Publish(new OnChunkLoaded
                    {
                        ChunkX = chunk.ChunkX,
                        ChunkZ = chunk.ChunkZ,
                        ChunkIndex = chunkIndex
                    });
                }
            }
        }
        
        public void MarkChunkUnloaded(int chunkIndex)
        {
            if (chunkIndex >= 0 && chunkIndex < TotalChunks)
            {
                ref ChunkInfo chunk = ref _chunks[chunkIndex];
                if (chunk.State == ChunkState.Unloading)
                {
                    chunk.State = ChunkState.Unloaded;
                    ActiveChunkCount--;
                    chunk.EntityCount = 0;
                    _chunks[chunkIndex] = chunk;
                    
                    EventBus.Publish(new OnChunkUnloaded
                    {
                        ChunkX = chunk.ChunkX,
                        ChunkZ = chunk.ChunkZ,
                        ChunkIndex = chunkIndex
                    });
                }
            }
        }
        
        public void AddEntityToChunk(int chunkIndex)
        {
            if (chunkIndex >= 0 && chunkIndex < TotalChunks)
            {
                ref ChunkInfo chunk = ref _chunks[chunkIndex];
                chunk.EntityCount++;
                chunk.IsDirty = true;
                _chunks[chunkIndex] = chunk;
            }
        }
        
        public void RemoveEntityFromChunk(int chunkIndex)
        {
            if (chunkIndex >= 0 && chunkIndex < TotalChunks)
            {
                ref ChunkInfo chunk = ref _chunks[chunkIndex];
                chunk.EntityCount = Mathf.Max(0, chunk.EntityCount - 1);
                chunk.IsDirty = true;
                _chunks[chunkIndex] = chunk;
            }
        }
        
        public void Dispose()
        {
            if (_chunks.IsCreated)
                _chunks.Dispose();
            if (_chunkCoordToIndex.IsCreated)
                _chunkCoordToIndex.Dispose();
        }
    }
    
    /// <summary>
    /// Event fired when a chunk load is requested.
    /// </summary>
    public struct OnChunkLoadRequested
    {
        public int ChunkX;
        public int ChunkZ;
        public int ChunkIndex;
    }
    
    /// <summary>
    /// Event fired when a chunk unload is requested.
    /// </summary>
    public struct OnChunkUnloadRequested
    {
        public int ChunkX;
        public int ChunkZ;
        public int ChunkIndex;
    }
    
    /// <summary>
    /// Event fired when a chunk has finished loading.
    /// </summary>
    public struct OnChunkLoaded
    {
        public int ChunkX;
        public int ChunkZ;
        public int ChunkIndex;
    }
    
    /// <summary>
    /// Event fired when a chunk has finished unloading.
    /// </summary>
    public struct OnChunkUnloaded
    {
        public int ChunkX;
        public int ChunkZ;
        public int ChunkIndex;
    }
}
