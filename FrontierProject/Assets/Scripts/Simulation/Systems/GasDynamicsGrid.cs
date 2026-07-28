using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// 3D cellular automata for gas dynamics simulation.
    /// Tracks O2, toxicity, temperature, and pressure per voxel.
    /// </summary>
    public struct GasVoxel
    {
        public float oxygen;        // 0.0 - 1.0
        public float toxicity;      // 0.0 - 1.0
        public float temperature;   // Kelvin
        public float pressure;      // kPa
        public float coConcentration; // CO concentration
        public byte materialType;   // Air, Wall, Vent, etc.
    }

    public class GasDynamicsGrid
    {
        public const int GridSize = 64;
        public const float CellSize = 0.5f;
        
        private NativeArray<GasVoxel> _grid;
        private NativeArray<GasVoxel> _nextGrid;
        private bool _isDirty;
        
        public int Width => GridSize;
        public int Height => GridSize;
        public int Depth => GridSize;
        public int TotalCells => GridSize * GridSize * GridSize;
        
        public GasDynamicsGrid()
        {
            _grid = new NativeArray<GasVoxel>(TotalCells, Allocator.Persistent);
            _nextGrid = new NativeArray<GasVoxel>(TotalCells, Allocator.Persistent);
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            for (int i = 0; i < TotalCells; i++)
            {
                _grid[i] = new GasVoxel
                {
                    oxygen = 0.21f, // Earth-like atmosphere
                    toxicity = 0f,
                    temperature = 293f, // 20°C
                    pressure = 101.325f, // Standard atmospheric pressure
                    coConcentration = 0f,
                    materialType = 0 // Air
                };
            }
        }
        
        public GasVoxel GetVoxel(int x, int y, int z)
        {
            if (!IsValid(x, y, z)) return default;
            return _grid[GetIndex(x, y, z)];
        }
        
        public void SetVoxel(int x, int y, int z, GasVoxel voxel)
        {
            if (!IsValid(x, y, z)) return;
            _grid[GetIndex(x, y, z)] = voxel;
            _isDirty = true;
        }
        
        public void SetMaterialType(int x, int y, int z, byte type)
        {
            if (!IsValid(x, y, z)) return;
            int idx = GetIndex(x, y, z);
            var v = _grid[idx];
            v.materialType = type;
            _grid[idx] = v;
            _isDirty = true;
        }
        
        public void AddGas(int x, int y, int z, float oxygenDelta, float toxicityDelta, float heatDelta)
        {
            if (!IsValid(x, y, z)) return;
            int idx = GetIndex(x, y, z);
            var v = _grid[idx];
            v.oxygen = math.clamp(v.oxygen + oxygenDelta, 0f, 1f);
            v.toxicity = math.clamp(v.toxicity + toxicityDelta, 0f, 1f);
            v.temperature = math.max(0f, v.temperature + heatDelta);
            _grid[idx] = v;
            _isDirty = true;
        }
        
        public void AddCO(int x, int y, int z, float amount)
        {
            if (!IsValid(x, y, z)) return;
            int idx = GetIndex(x, y, z);
            var v = _grid[idx];
            v.coConcentration = math.clamp(v.coConcentration + amount, 0f, 1f);
            v.oxygen = math.max(0f, v.oxygen - amount * 0.5f); // CO displaces O2
            _grid[idx] = v;
            _isDirty = true;
        }
        
        [BurstCompile]
        public struct DiffusionJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<GasVoxel> input;
            [WriteOnly] public NativeArray<GasVoxel> output;
            public int gridSize;
            public float diffusionRate;
            public float thermalConductivity;
            
            public void Execute(int index)
            {
                int x = index % gridSize;
                int y = (index / gridSize) % gridSize;
                int z = index / (gridSize * gridSize);
                
                GasVoxel center = input[index];
                
                // Skip solid materials
                if (center.materialType == 1) // Wall
                {
                    output[index] = center;
                    return;
                }
                
                float avgOxygen = center.oxygen;
                float avgToxicity = center.toxicity;
                float avgTemp = center.temperature;
                float avgPressure = center.pressure;
                float avgCO = center.coConcentration;
                int neighborCount = 1;
                
                // Sample 6 neighbors (von Neumann neighborhood)
                int[] dx = {1, -1, 0, 0, 0, 0};
                int[] dy = {0, 0, 1, -1, 0, 0};
                int[] dz = {0, 0, 0, 0, 1, -1};
                
                for (int i = 0; i < 6; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    int nz = z + dz[i];
                    
                    if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && nz >= 0 && nz < gridSize)
                    {
                        int nIdx = nx + ny * gridSize + nz * gridSize * gridSize;
                        GasVoxel neighbor = input[nIdx];
                        
                        if (neighbor.materialType != 1) // Not a wall
                        {
                            avgOxygen += neighbor.oxygen;
                            avgToxicity += neighbor.toxicity;
                            avgTemp += neighbor.temperature;
                            avgPressure += neighbor.pressure;
                            avgCO += neighbor.coConcentration;
                            neighborCount++;
                        }
                    }
                }
                
                avgOxygen /= neighborCount;
                avgToxicity /= neighborCount;
                avgTemp /= neighborCount;
                avgPressure /= neighborCount;
                avgCO /= neighborCount;
                
                // Apply diffusion
                GasVoxel result = center;
                result.oxygen = math.lerp(center.oxygen, avgOxygen, diffusionRate);
                result.toxicity = math.lerp(center.toxicity, avgToxicity, diffusionRate);
                result.temperature = math.lerp(center.temperature, avgTemp, thermalConductivity);
                result.pressure = math.lerp(center.pressure, avgPressure, diffusionRate);
                result.coConcentration = math.lerp(center.coConcentration, avgCO, diffusionRate);
                
                output[index] = result;
            }
        }
        
        public JobHandle SimulateStep(JobHandle inputDeps, float deltaTime)
        {
            var job = new DiffusionJob
            {
                input = _grid,
                output = _nextGrid,
                gridSize = GridSize,
                diffusionRate = 0.1f * deltaTime * 60f,
                thermalConductivity = 0.05f * deltaTime * 60f
            };
            
            JobHandle handle = job.Schedule(TotalCells, 64, inputDeps);
            
            // Swap buffers
            var temp = _grid;
            _grid = _nextGrid;
            _nextGrid = temp;
            
            _isDirty = false;
            return handle;
        }
        
        public void Dispose()
        {
            if (_grid.IsCreated) _grid.Dispose();
            if (_nextGrid.IsCreated) _nextGrid.Dispose();
        }
        
        private bool IsValid(int x, int y, int z)
        {
            return x >= 0 && x < GridSize && y >= 0 && y < GridSize && z >= 0 && z < GridSize;
        }
        
        private int GetIndex(int x, int y, int z)
        {
            return x + y * GridSize + z * GridSize * GridSize;
        }
        
        // Query methods for gameplay systems
        public bool IsBreathable(int x, int y, int z)
        {
            var v = GetVoxel(x, y, z);
            return v.oxygen > 0.15f && v.toxicity < 0.3f && v.coConcentration < 0.01f;
        }
        
        public bool IsToxic(int x, int y, int z)
        {
            var v = GetVoxel(x, y, z);
            return v.toxicity > 0.2f || v.coConcentration > 0.005f;
        }
        
        public float GetTemperature(int x, int y, int z)
        {
            return GetVoxel(x, y, z).temperature;
        }
        
        public float GetOxygenLevel(int x, int y, int z)
        {
            return GetVoxel(x, y, z).oxygen;
        }
    }
}
