using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.Nav
{
    /// <summary>
    /// Flow field pathfinding for group movement of large squads.
    /// Generates a vector field pointing toward the goal from every cell.
    /// </summary>
    public struct FlowFieldCell
    {
        public float2 Direction;      // Normalized direction to goal
        public float Cost;            // Movement cost through this cell
        public int Distance;          // Distance to goal (for heatmap)
        public bool Walkable;         // Is this cell traversable
        public byte TerrainType;      // Terrain type index
    }

    public class FlowFieldPath
    {
        private const int CellSize = 2; // meters per cell
        private NativeArray<FlowFieldCell> _grid;
        private int _width;
        private int _height;
        private readonly int _worldSize = 512;

        public FlowFieldPath()
        {
            Initialize();
        }

        private void Initialize()
        {
            _width = _worldSize / CellSize;
            _height = _worldSize / CellSize;
            
            if (_grid.IsCreated) _grid.Dispose();
            _grid = new NativeArray<FlowFieldCell>(_width * _height, Allocator.Persistent);
            
            // Initialize all cells as walkable with default cost
            for (int i = 0; i < _grid.Length; i++)
            {
                _grid[i] = new FlowFieldCell
                {
                    Direction = math.float2(0, 0),
                    Cost = 1.0f,
                    Distance = int.MaxValue,
                    Walkable = true,
                    TerrainType = 0
                };
            }
        }

        public void Dispose()
        {
            if (_grid.IsCreated)
                _grid.Dispose();
        }

        /// <summary>
        /// Generate flow field toward a target position.
        /// Uses Dijkstra's algorithm to calculate distances from goal.
        /// </summary>
        public void GenerateFlowField(float3 targetPos, NativeHashMap<int2, byte> terrainCosts)
        {
            int targetX = Mathf.Clamp((int)(targetPos.x / CellSize), 0, _width - 1);
            int targetY = Mathf.Clamp((int)(targetPos.z / CellSize), 0, _height - 1);

            // Reset distances
            for (int i = 0; i < _grid.Length; i++)
            {
                var cell = _grid[i];
                cell.Distance = int.MaxValue;
                cell.Direction = math.float2(0, 0);
                _grid[i] = cell;
            }

            // Set goal distance to 0
            int goalIdx = targetY * _width + targetX;
            var goalCell = _grid[goalIdx];
            goalCell.Distance = 0;
            _grid[goalIdx] = goalCell;

            // Dijkstra's algorithm using simple queue (production would use heap)
            var openSet = new NativeList<int2>(Allocator.TempJob);
            openSet.Add(new int2(targetX, targetY));

            while (openSet.Length > 0)
            {
                // Find cell with lowest distance
                int bestIdx = 0;
                int bestDist = int.MaxValue;

                for (int i = 0; i < openSet.Length; i++)
                {
                    int2 coord = openSet[i];
                    int idx = coord.y * _width + coord.x;
                    if (_grid[idx].Distance < bestDist)
                    {
                        bestDist = _grid[idx].Distance;
                        bestIdx = i;
                    }
                }

                int2 current = openSet[bestIdx];
                openSet.RemoveAtSwapBack(bestIdx);

                int currentIdx = current.y * _width + current.x;
                int currentDist = _grid[currentIdx].Distance;

                // Process 8 neighbors
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = current.x + dx;
                        int ny = current.y + dy;

                        if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                            continue;

                        int neighborIdx = ny * _width + nx;
                        var neighbor = _grid[neighborIdx];

                        if (!neighbor.Walkable) continue;

                        // Get terrain cost
                        int2 chunkCoord = new int2(nx / 16, ny / 16);
                        byte terrainCost = terrainCosts.TryGetValue(chunkCoord, out byte cost) ? cost : (byte)1;
                        
                        // Diagonal movement costs more
                        float moveCost = (dx != 0 && dy != 0) ? 1.414f : 1.0f;
                        moveCost *= terrainCost;

                        int newDist = currentDist + (int)(moveCost * 10); // Scale for integer math

                        if (newDist < neighbor.Distance)
                        {
                            neighbor.Distance = newDist;
                            
                            // Calculate direction to goal (opposite of gradient)
                            int2 goalDir = new int2(targetX - nx, targetY - ny);
                            float len = math.length(new float2(goalDir.x, goalDir.y));
                            if (len > 0)
                                neighbor.Direction = math.normalize(new float2(goalDir.x, goalDir.y));
                            else
                                neighbor.Direction = math.float2(0, 0);

                            _grid[neighborIdx] = neighbor;

                            if (!Contains(openSet, nx, ny))
                                openSet.Add(new int2(nx, ny));
                        }
                    }
                }
            }

            openSet.Dispose();
        }

        private bool Contains(NativeList<int2> list, int x, int y)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].x == x && list[i].y == y)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the flow direction at a world position.
        /// </summary>
        public float2 GetDirection(float3 worldPos)
        {
            int x = Mathf.Clamp((int)(worldPos.x / CellSize), 0, _width - 1);
            int y = Mathf.Clamp((int)(worldPos.z / CellSize), 0, _height - 1);
            
            int idx = y * _width + x;
            return _grid[idx].Direction;
        }

        /// <summary>
        /// Get the cost at a world position.
        /// </summary>
        public float GetCost(float3 worldPos)
        {
            int x = Mathf.Clamp((int)(worldPos.x / CellSize), 0, _width - 1);
            int y = Mathf.Clamp((int)(worldPos.z / CellSize), 0, _height - 1);
            
            int idx = y * _width + x;
            return _grid[idx].Cost;
        }

        /// <summary>
        /// Mark a cell as unwalkable (for dynamic obstacles).
        /// </summary>
        public void SetWalkable(float3 worldPos, bool walkable)
        {
            int x = Mathf.Clamp((int)(worldPos.x / CellSize), 0, _width - 1);
            int y = Mathf.Clamp((int)(worldPos.z / CellSize), 0, _height - 1);
            
            int idx = y * _width + x;
            var cell = _grid[idx];
            cell.Walkable = walkable;
            _grid[idx] = cell;
        }

        /// <summary>
        /// Burst job for parallel flow field following.
        /// </summary>
        [Unity.Burst.Burst]
        public struct FollowFlowFieldJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<FlowFieldCell> Grid;
            [ReadOnly] public int Width;
            [ReadOnly] public int CellSize;
            [ReadOnly] public NativeArray<float3> Positions;
            public NativeArray<float3> Velocities;
            [ReadOnly] public float Speed;
            [ReadOnly] public float DeltaTime;

            public void Execute(int index)
            {
                float3 pos = Positions[index];
                int x = Mathf.Clamp((int)(pos.x / CellSize), 0, Width - 1);
                int y = Mathf.Clamp((int)(pos.z / CellSize), 0, (Grid.Length / Width) - 1);
                
                int idx = y * Width + x;
                FlowFieldCell cell = Grid[idx];

                if (cell.Walkable && math.length(cell.Direction) > 0.01f)
                {
                    float3 velocity = new float3(cell.Direction.x, 0, cell.Direction.y) * Speed;
                    Velocities[index] = math.lerp(Velocities[index], velocity, DeltaTime * 5.0f);
                }
            }
        }

        /// <summary>
        /// Create a job to follow the flow field.
        /// </summary>
        public FollowFlowFieldJob CreateFollowJob(
            NativeArray<float3> positions,
            NativeArray<float3> velocities,
            float speed,
            float deltaTime)
        {
            return new FollowFlowFieldJob
            {
                Grid = _grid,
                Width = _width,
                CellSize = CellSize,
                Positions = positions,
                Velocities = velocities,
                Speed = speed,
                DeltaTime = deltaTime
            };
        }

        private void OnDrawGizmos()
        {
            // Visualization helper (call from MonoBehaviour)
            Gizmos.color = Color.green;
            
            int step = 10; // Draw every Nth cell
            for (int y = 0; y < _height; y += step)
            {
                for (int x = 0; x < _width; x += step)
                {
                    int idx = y * _width + x;
                    var cell = _grid[idx];
                    
                    if (!cell.Walkable) continue;
                    
                    float3 pos = new float3(x * CellSize + CellSize / 2f, 0.5f, y * CellSize + CellSize / 2f);
                    float3 dir = new float3(cell.Direction.x, 0, cell.Direction.y) * 2f;
                    
                    Gizmos.DrawRay(pos, dir);
                }
            }
        }
    }
}
