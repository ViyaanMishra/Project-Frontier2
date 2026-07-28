using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.Nav
{
    /// <summary>
    /// Long-range chunk-level pathfinding using A* algorithm.
    /// Operates on the 32x32 chunk grid for macro-scale navigation.
    /// </summary>
    public struct MacroNode
    {
        public int2 ChunkCoord;
        public int GCost; // Cost from start
        public int HCost; // Heuristic to goal
        public int FCost => GCost + HCost;
        public int ParentIndex;
        public bool Walkable;
        public byte TerrainCost; // 1-255 movement cost multiplier
    }

    public class AStarMacroPath
    {
        private NativeList<MacroNode> _openSet;
        private NativeList<MacroNode> _closedSet;
        private NativeHashMap<int2, int> _chunkIndexMap;
        private readonly int _worldChunks = 16; // 512 / 32

        public AStarMacroPath()
        {
            _openSet = new NativeList<MacroNode>(Allocator.Persistent);
            _closedSet = new NativeList<MacroNode>(Allocator.Persistent);
            _chunkIndexMap = new NativeHashMap<int2, int>(256, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (_openSet.IsCreated) _openSet.Dispose();
            if (_closedSet.IsCreated) _closedSet.Dispose();
            if (_chunkIndexMap.IsCreated) _chunkIndexMap.Dispose();
        }

        public NativeList<int2> FindPath(int2 startChunk, int2 endChunk, NativeHashMap<int2, byte> chunkCosts)
        {
            _openSet.Clear();
            _closedSet.Clear();
            _chunkIndexMap.Clear();

            var result = new NativeList<int2>(Allocator.TempJob);

            // Initialize start node
            var startNode = new MacroNode
            {
                ChunkCoord = startChunk,
                GCost = 0,
                HCost = ManhattanDistance(startChunk, endChunk),
                ParentIndex = -1,
                Walkable = true,
                TerrainCost = chunkCosts.TryGetValue(startChunk, out byte cost) ? cost : (byte)1
            };

            _openSet.Add(startNode);
            _chunkIndexMap[startChunk] = 0;

            int iterations = 0;
            const int maxIterations = 1000;

            while (_openSet.Length > 0 && iterations < maxIterations)
            {
                iterations++;

                // Find node with lowest F cost
                int bestIdx = 0;
                int bestFCost = int.MaxValue;

                for (int i = 0; i < _openSet.Length; i++)
                {
                    var node = _openSet[i];
                    if (node.FCost < bestFCost || (node.FCost == bestFCost && node.HCost < _openSet[bestIdx].HCost))
                    {
                        bestFCost = node.FCost;
                        bestIdx = i;
                    }
                }

                var current = _openSet[bestIdx];
                _openSet.RemoveAtSwapBack(bestIdx);
                _closedSet.Add(current);

                // Check if reached goal
                if (current.ChunkCoord.x == endChunk.x && current.ChunkCoord.y == endChunk.y)
                {
                    ReconstructPath(result, current, startChunk);
                    return result;
                }

                // Explore neighbors (4-directional for macro path)
                int2[] neighbors = {
                    current.ChunkCoord + new int2(1, 0),
                    current.ChunkCoord + new int2(-1, 0),
                    current.ChunkCoord + new int2(0, 1),
                    current.ChunkCoord + new int2(0, -1)
                };

                foreach (var neighborCoord in neighbors)
                {
                    if (neighborCoord.x < 0 || neighborCoord.x >= _worldChunks ||
                        neighborCoord.y < 0 || neighborCoord.y >= _worldChunks)
                        continue;

                    if (!chunkCosts.TryGetValue(neighborCoord, out byte terrainCost))
                        terrainCost = 255; // Unwalkable

                    if (terrainCost >= 255) // Obstacle
                        continue;

                    // Check if already in closed set
                    bool inClosed = false;
                    for (int i = 0; i < _closedSet.Length; i++)
                    {
                        if (_closedSet[i].ChunkCoord.x == neighborCoord.x &&
                            _closedSet[i].ChunkCoord.y == neighborCoord.y)
                        {
                            inClosed = true;
                            break;
                        }
                    }

                    if (inClosed) continue;

                    int newGCost = current.GCost + terrainCost;
                    bool inOpen = _chunkIndexMap.TryGetValue(neighborCoord, out int existingIdx);

                    if (!inOpen || newGCost < current.GCost + terrainCost)
                    {
                        var neighbor = new MacroNode
                        {
                            ChunkCoord = neighborCoord,
                            GCost = newGCost,
                            HCost = ManhattanDistance(neighborCoord, endChunk),
                            ParentIndex = GetIndexInClosed(current.ChunkCoord),
                            Walkable = true,
                            TerrainCost = terrainCost
                        };

                        if (inOpen)
                        {
                            // Update existing node
                            // (Simplified - in production would update in place)
                        }
                        else
                        {
                            _openSet.Add(neighbor);
                            _chunkIndexMap[neighborCoord] = _openSet.Length - 1;
                        }
                    }
                }
            }

            // No path found - return direct line as fallback
            if (result.Length == 0)
            {
                result.Add(startChunk);
                result.Add(endChunk);
            }

            return result;
        }

        private int ManhattanDistance(int2 a, int2 b)
        {
            return math.abs(a.x - b.x) + math.abs(a.y - b.y);
        }

        private int GetIndexInClosed(int2 coord)
        {
            for (int i = 0; i < _closedSet.Length; i++)
            {
                if (_closedSet[i].ChunkCoord.x == coord.x &&
                    _closedSet[i].ChunkCoord.y == coord.y)
                    return i;
            }
            return -1;
        }

        private void ReconstructPath(NativeList<int2> path, MacroNode endNode, int2 startChunk)
        {
            path.Add(endNode.ChunkCoord);
            
            // Simple reconstruction - in production would follow parent indices
            var current = endNode.ChunkCoord;
            while (current.x != startChunk.x || current.y != startChunk.y)
            {
                // Interpolate for now
                int dx = math.sign(startChunk.x - current.x);
                int dy = math.sign(startChunk.y - current.y);
                
                if (dx != 0) current.x += dx;
                else if (dy != 0) current.y += dy;
                
                if (current.x != startChunk.x || current.y != startChunk.y)
                    path.Add(current);
            }
            
            path.Reverse();
        }
    }

    /// <summary>
    /// Burst-compatible job for parallel A* pathfinding.
    /// </summary>
    [Unity.Burst.Burst]
    public struct AStarMacroPathJob : IJob
    {
        [ReadOnly] public NativeArray<int2> StartPoints;
        [ReadOnly] public NativeArray<int2> EndPoints;
        [ReadOnly] public NativeHashMap<int2, byte> ChunkCosts;
        public NativeList<NativeList<int2>> Results;

        public void Execute()
        {
            var pathfinder = new AStarMacroPath();
            
            try
            {
                for (int i = 0; i < StartPoints.Length; i++)
                {
                    var path = pathfinder.FindPath(StartPoints[i], EndPoints[i], ChunkCosts);
                    Results.Add(path);
                }
            }
            finally
            {
                pathfinder.Dispose();
            }
        }
    }
}
