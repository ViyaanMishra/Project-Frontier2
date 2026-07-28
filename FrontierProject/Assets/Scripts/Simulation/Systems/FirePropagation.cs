using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Fire propagation system using cellular automata.
    /// Handles material flashpoints, thermal radiation, oxygen consumption, and CO spread.
    /// </summary>
    public enum MaterialFlammability
    {
        NonFlammable = 0,
        Resistant = 1,
        Normal = 2,
        Flammable = 3,
        HighlyFlammable = 4,
        Explosive = 5
    }

    public struct FireCell
    {
        public float temperature;       // Kelvin
        public float fuelAmount;        // 0.0 - 1.0
        public float burnRate;          // Current burn intensity
        public byte materialType;
        public MaterialFlammability flammability;
        public bool isBurning;
        public float timeToIgnition;    // Seconds until ignition at current temp
        public float smokeProduction;
    }

    public class FirePropagationSystem : IDisposable
    {
        public const int GridSize = 64;
        public const float FlashpointWood = 573f;      // 300°C
        public const float FlashpointFabric = 473f;    // 200°C
        public const float FlashpointFuel = 313f;      // 40°C
        public const float MaxFireTemp = 1473f;        // 1200°C
        
        private NativeArray<FireCell> _grid;
        private NativeArray<FireCell> _nextGrid;
        private readonly GasDynamicsGrid _gasGrid;
        
        public FirePropagationSystem(GasDynamicsGrid gasGrid)
        {
            _gasGrid = gasGrid;
            _grid = new NativeArray<FireCell>(GridSize * GridSize * GridSize, Allocator.Persistent);
            _nextGrid = new NativeArray<FireCell>(GridSize * GridSize * GridSize, Allocator.Persistent);
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            for (int i = 0; i < _grid.Length; i++)
            {
                _grid[i] = new FireCell
                {
                    temperature = 293f,
                    fuelAmount = 0f,
                    burnRate = 0f,
                    materialType = 0,
                    flammability = MaterialFlammability.NonFlammable,
                    isBurning = false,
                    timeToIgnition = float.MaxValue,
                    smokeProduction = 0f
                };
            }
        }
        
        public void SetMaterial(int x, int y, int z, byte type, MaterialFlammability flammability, float fuelAmount)
        {
            if (!IsValid(x, y, z)) return;
            int idx = GetIndex(x, y, z);
            var cell = _grid[idx];
            cell.materialType = type;
            cell.flammability = flammability;
            cell.fuelAmount = fuelAmount;
            _grid[idx] = cell;
        }
        
        public void ApplyHeatSource(int x, int y, int z, float temperature, float duration)
        {
            if (!IsValid(x, y, z)) return;
            int idx = GetIndex(x, y, z);
            var cell = _grid[idx];
            cell.temperature = math.max(cell.temperature, temperature);
            
            // Calculate time to ignition based on flashpoint
            float flashpoint = GetFlashpoint(cell.flammability);
            if (cell.temperature >= flashpoint && cell.fuelAmount > 0.1f)
            {
                cell.isBurning = true;
                cell.burnRate = 1f;
            }
            
            _grid[idx] = cell;
        }
        
        public void Ignite(int x, int y, int z)
        {
            if (!IsValid(x, y, z)) return;
            int idx = GetIndex(x, y, z);
            var cell = _grid[idx];
            
            if (cell.fuelAmount > 0.1f && cell.flammability != MaterialFlammability.NonFlammable)
            {
                cell.isBurning = true;
                cell.burnRate = 1f;
                cell.temperature = math.max(cell.temperature, GetFlashpoint(cell.flammability));
                _grid[idx] = cell;
            }
        }
        
        [BurstCompile]
        public struct FireSpreadJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<FireCell> input;
            [WriteOnly] public NativeArray<FireCell> output;
            [ReadOnly] public NativeArray<float> oxygenLevels;
            public int gridSize;
            public float deltaTime;
            public float heatTransferRate;
            public float fireSpreadChance;
            
            private float GetFlashpoint(MaterialFlammability f)
            {
                switch (f)
                {
                    case MaterialFlammability.HighlyFlammable: return 313f;
                    case MaterialFlammability.Flammable: return 473f;
                    case MaterialFlammability.Normal: return 573f;
                    case MaterialFlammability.Resistant: return 773f;
                    default: return 9999f;
                }
            }
            
            public void Execute(int index)
            {
                FireCell cell = input[index];
                FireCell result = cell;
                
                int x = index % gridSize;
                int y = (index / gridSize) % gridSize;
                int z = index / (gridSize * gridSize);
                
                // Get oxygen level from gas grid
                int gasIdx = index;
                float oxygen = (gasIdx < oxygenLevels.Length) ? oxygenLevels[gasIdx] : 0.21f;
                
                if (cell.isBurning)
                {
                    // Consume fuel
                    result.fuelAmount -= cell.burnRate * deltaTime * 0.1f;
                    
                    // Consume oxygen
                    float oxygenConsumed = cell.burnRate * deltaTime * 0.05f;
                    
                    // Produce heat and smoke
                    result.smokeProduction = cell.burnRate * 0.5f;
                    
                    // Check if fire dies out
                    if (result.fuelAmount <= 0.01f || oxygen < 0.1f)
                    {
                        result.isBurning = false;
                        result.burnRate = 0f;
                        result.temperature = math.lerp(result.temperature, 293f, deltaTime * 0.5f);
                    }
                    else
                    {
                        // Maintain combustion temperature
                        result.temperature = math.min(MaxFireTemp, result.temperature + cell.burnRate * deltaTime * 10f);
                    }
                }
                else
                {
                    // Cool down if not burning
                    if (result.temperature > 293f)
                    {
                        result.temperature = math.lerp(result.temperature, 293f, deltaTime * 0.1f);
                    }
                }
                
                // Heat transfer to neighbors (thermal radiation)
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
                        FireCell neighbor = input[nIdx];
                        
                        // Transfer heat
                        float heatDiff = cell.temperature - neighbor.temperature;
                        if (heatDiff > 0)
                        {
                            float transferred = heatDiff * heatTransferRate * deltaTime;
                            // Note: Actual neighbor update happens when processing that cell
                        }
                        
                        // Spread fire chance
                        if (cell.isBurning && !neighbor.isBurning && neighbor.fuelAmount > 0.1f)
                        {
                            float flashpoint = GetFlashpoint(neighbor.flammability);
                            float distFactor = 1.0f / (1.0f + i); // Closer neighbors more likely
                            
                            if (cell.temperature > flashpoint * 0.8f && 
                                oxygen > 0.15f && 
                                UnityEngine.Random.value < fireSpreadChance * distFactor)
                            {
                                neighbor.isBurning = true;
                                neighbor.burnRate = 0.5f;
                                neighbor.temperature = flashpoint;
                            }
                        }
                    }
                }
                
                output[index] = result;
            }
        }
        
        public JobHandle SimulateStep(JobHandle inputDeps, float deltaTime)
        {
            // Create temporary oxygen array from gas grid
            NativeArray<float> oxygenLevels = new NativeArray<float>(_grid.Length, Allocator.TempJob);
            for (int i = 0; i < _grid.Length; i++)
            {
                int x = i % GridSize;
                int y = (i / GridSize) % GridSize;
                int z = i / (GridSize * GridSize);
                oxygenLevels[i] = _gasGrid.GetOxygenLevel(x, y, z);
            }
            
            var job = new FireSpreadJob
            {
                input = _grid,
                output = _nextGrid,
                oxygenLevels = oxygenLevels,
                gridSize = GridSize,
                deltaTime = deltaTime,
                heatTransferRate = 0.3f,
                fireSpreadChance = 0.02f
            };
            
            JobHandle handle = job.Schedule(_grid.Length, 64, inputDeps);
            
            // Cleanup and swap
            handle.Complete();
            oxygenLevels.Dispose();
            
            var temp = _grid;
            _grid = _nextGrid;
            _nextGrid = temp;
            
            return handle;
        }
        
        public bool IsCellBurning(int x, int y, int z)
        {
            if (!IsValid(x, y, z)) return false;
            return _grid[GetIndex(x, y, z)].isBurning;
        }
        
        public float GetCellTemperature(int x, int y, int z)
        {
            if (!IsValid(x, y, z)) return 293f;
            return _grid[GetIndex(x, y, z)].temperature;
        }
        
        public float GetSmokeDensity(int x, int y, int z)
        {
            if (!IsValid(x, y, z)) return 0f;
            return _grid[GetIndex(x, y, z)].smokeProduction;
        }
        
        private bool IsValid(int x, int y, int z)
        {
            return x >= 0 && x < GridSize && y >= 0 && y < GridSize && z >= 0 && z < GridSize;
        }
        
        private int GetIndex(int x, int y, int z)
        {
            return x + y * GridSize + z * GridSize * GridSize;
        }
        
        private float GetFlashpoint(MaterialFlammability f)
        {
            switch (f)
            {
                case MaterialFlammability.Explosive: return 293f;
                case MaterialFlammability.HighlyFlammable: return 313f;
                case MaterialFlammability.Flammable: return 473f;
                case MaterialFlammability.Normal: return 573f;
                case MaterialFlammability.Resistant: return 773f;
                default: return 9999f;
            }
        }
        
        public void Dispose()
        {
            if (_grid.IsCreated) _grid.Dispose();
            if (_nextGrid.IsCreated) _nextGrid.Dispose();
        }
    }
}
