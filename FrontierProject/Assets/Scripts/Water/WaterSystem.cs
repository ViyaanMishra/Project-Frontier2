using System.Collections.Generic;
using UnityEngine;

namespace Frontier.Water
{
    /// <summary>
    /// Water source types.
    /// </summary>
    public enum WaterSourceType
    {
        Rain,           // Rain catchers
        Well,           // Ground water wells
        River,          // River/lake pumps
        IceMelt,        // Snow/ice melter
        Purified,       // Processed/treated water
        Contaminated    // Unsafe water
    }

    /// <summary>
    /// Water quality levels.
    /// </summary>
    public enum WaterQuality
    {
        Pure,           // Safe to drink, no penalties
        Clean,          // Safe but minor taste issues
        Murky,          // Risk of illness if consumed regularly
        Contaminated,   // High disease risk
        Toxic,          // Causes poisoning
        AnomalyTainted  // Exotic contamination effects
    }

    /// <summary>
    /// Represents a water storage tank or container.
    /// </summary>
    public struct WaterTank
    {
        public string Id;
        public float Capacity;          // Max liters
        public float CurrentAmount;     // Current liters
        public WaterQuality Quality;
        public float Temperature;       // Celsius
        public bool IsConnected;        // Connected to plumbing
        public List<string> ConnectedPipes; // Pipe IDs
    }

    /// <summary>
    /// Manages global water system including sources, purification, distribution, and consumption.
    /// </summary>
    public class WaterSystem
    {
        private Dictionary<string, WaterTank> _tanks = new Dictionary<string, WaterTank>();
        private Dictionary<string, WaterSource> _sources = new Dictionary<string, WaterSource>();
        private Dictionary<string, WaterPipe> _pipes = new Dictionary<string, WaterPipe>();
        
        // Global water stats
        public float TotalStored { get; private set; }
        public float TotalConsumption { get; private set; }
        public int ContaminationEvents { get; private set; }

        // Events
        public event Action<string, WaterQuality> OnWaterQualityChanged;
        public event Action<string> OnTankEmptied;
        public event Action<string> OnContaminationDetected;

        /// <summary>
        /// Add a water tank to the system.
        /// </summary>
        public string AddTank(float capacity, Vector3 position, WaterQuality initialQuality = WaterQuality.Pure)
        {
            string id = $"tank_{_tanks.Count}";
            
            var tank = new WaterTank
            {
                Id = id,
                Capacity = capacity,
                CurrentAmount = capacity * 0.5f, // Start half full
                Quality = initialQuality,
                Temperature = 20f,
                IsConnected = false,
                ConnectedPipes = new List<string>()
            };

            _tanks[id] = tank;
            RecalculateTotals();
            
            return id;
        }

        /// <summary>
        /// Add a water source (well, river pump, etc.).
        /// </summary>
        public string AddSource(WaterSourceType type, Vector3 position, float outputRate)
        {
            string id = $"source_{_sources.Count}";
            
            var source = new WaterSource
            {
                Id = id,
                Type = type,
                Position = position,
                OutputRate = outputRate, // Liters per minute
                Active = true,
                Quality = GetBaseQualityForType(type)
            };

            _sources[id] = source;
            return id;
        }

        /// <summary>
        /// Connect pipes between tanks/sources.
        /// </summary>
        public void ConnectPipe(string tankIdA, string tankIdB, float flowRate)
        {
            if (!_tanks.ContainsKey(tankIdA) || !_tanks.ContainsKey(tankIdB))
                return;

            string pipeId = $"pipe_{tankIdA}_{tankIdB}";
            
            var pipe = new WaterPipe
            {
                Id = pipeId,
                TankA = tankIdA,
                TankB = tankIdB,
                FlowRate = flowRate,
                IsBlocked = false,
                HasLeak = false
            };

            _pipes[pipeId] = pipe;

            // Mark tanks as connected
            var tankA = _tanks[tankIdA];
            var tankB = _tanks[tankIdB];
            
            if (!tankA.ConnectedPipes.Contains(pipeId))
                tankA.ConnectedPipes.Add(pipeId);
            if (!tankB.ConnectedPipes.Contains(pipeId))
                tankB.ConnectedPipes.Add(pipeId);

            _tanks[tankIdA] = tankA;
            _tanks[tankIdB] = tankB;
        }

        /// <summary>
        /// Add water to a tank from a source.
        /// </summary>
        public void AddWater(string tankId, float amount, WaterQuality quality)
        {
            if (!_tanks.ContainsKey(tankId))
                return;

            var tank = _tanks[tankId];
            
            // Mix quality if adding different quality water
            if (tank.CurrentAmount > 0 && tank.Quality != quality)
            {
                tank.Quality = MixWaterQualities(tank.Quality, quality, tank.CurrentAmount, amount);
            }
            else if (tank.CurrentAmount <= 0)
            {
                tank.Quality = quality;
            }

            tank.CurrentAmount = Mathf.Min(tank.CurrentAmount + amount, tank.Capacity);
            _tanks[tankId] = tank;

            RecalculateTotals();
        }

        /// <summary>
        /// Remove water from a tank (consumption).
        /// </summary>
        public bool RemoveWater(string tankId, float amount, out WaterQuality removedQuality)
        {
            removedQuality = WaterQuality.Pure;

            if (!_tanks.ContainsKey(tankId))
                return false;

            var tank = _tanks[tankId];
            
            if (tank.CurrentAmount < amount)
            {
                removedQuality = tank.Quality;
                return false; // Not enough water
            }

            tank.CurrentAmount -= amount;
            removedQuality = tank.Quality;
            _tanks[tankId] = tank;

            TotalConsumption += amount;
            RecalculateTotals();

            if (tank.CurrentAmount <= 0.1f)
                OnTankEmptied?.Invoke(tankId);

            return true;
        }

        /// <summary>
        /// Purify water in a tank.
        /// </summary>
        public void PurifyWater(string tankId, PurificationMethod method)
        {
            if (!_tanks.ContainsKey(tankId))
                return;

            var tank = _tanks[tankId];
            
            switch (method)
            {
                case PurificationMethod.Boiling:
                    // Takes time, uses fuel, improves by 2 levels
                    tank.Quality = ImproveQuality(tank.Quality, 2);
                    tank.Temperature = 100f; // Boiling hot
                    break;

                case PurificationMethod.Chemical:
                    // Fast, uses chemicals, improves by 1 level
                    tank.Quality = ImproveQuality(tank.Quality, 1);
                    break;

                case PurificationMethod.UV:
                    // Requires power, instant, improves by 2 levels
                    tank.Quality = ImproveQuality(tank.Quality, 2);
                    break;

                case PurificationMethod.Filter:
                    // Removes contaminants, improves by 1 level
                    tank.Quality = ImproveQuality(tank.Quality, 1);
                    break;
            }

            _tanks[tankId] = tank;
            OnWaterQualityChanged?.Invoke(tankId, tank.Quality);
        }

        /// <summary>
        /// Check for contamination spread through connected pipes.
        /// </summary>
        public void CheckContaminationSpread()
        {
            foreach (var kvp in _pipes)
            {
                var pipe = kvp.Value;
                if (pipe.IsBlocked || pipe.HasLeak)
                    continue;

                var tankA = _tanks[pipe.TankA];
                var tankB = _tanks[pipe.TankB];

                // If one tank is contaminated and other is clean, risk of spread
                if (IsContaminated(tankA.Quality) && !IsContaminated(tankB.Quality))
                {
                    // 10% chance per check
                    if (Random.value < 0.1f)
                    {
                        tankB.Quality = DegradeQuality(tankB.Quality, 1);
                        _tanks[pipe.TankB] = tankB;
                        OnContaminationDetected?.Invoke(pipe.TankB);
                        ContaminationEvents++;
                    }
                }
                else if (IsContaminated(tankB.Quality) && !IsContaminated(tankA.Quality))
                {
                    if (Random.value < 0.1f)
                    {
                        tankA.Quality = DegradeQuality(tankA.Quality, 1);
                        _tanks[pipe.TankA] = tankA;
                        OnContaminationDetected?.Invoke(pipe.TankA);
                        ContaminationEvents++;
                    }
                }
            }
        }

        /// <summary>
        /// Update water flow through pipes.
        /// </summary>
        public void UpdateFlow(float deltaTime)
        {
            foreach (var kvp in _pipes)
            {
                var pipe = kvp.Value;
                if (pipe.IsBlocked)
                    continue;

                var tankA = _tanks[pipe.TankA];
                var tankB = _tanks[pipe.TankB];

                // Equalize water levels based on flow rate
                float maxFlow = pipe.FlowRate * deltaTime;
                float difference = tankA.CurrentAmount - tankB.CurrentAmount;

                if (Mathf.Abs(difference) > 0.1f)
                {
                    float flow = Mathf.Min(maxFlow, Mathf.Abs(difference));
                    
                    if (difference > 0)
                    {
                        // Flow A -> B
                        RemoveWater(pipe.TankA, flow, out _);
                        AddWater(pipe.TankB, flow, tankA.Quality);
                    }
                    else
                    {
                        // Flow B -> A
                        RemoveWater(pipe.TankB, flow, out _);
                        AddWater(pipe.TankA, flow, tankB.Quality);
                    }
                }
            }
        }

        /// <summary>
        /// Use water for irrigation.
        /// </summary>
        public float GetWaterForIrrigation(string tankId, float requestedAmount)
        {
            if (!_tanks.ContainsKey(tankId))
                return 0f;

            var tank = _tanks[tankId];
            
            // Any quality works for irrigation
            float available = Mathf.Min(requestedAmount, tank.CurrentAmount);
            RemoveWater(tankId, available, out _);
            
            return available;
        }

        /// <summary>
        /// Use water for fire suppression.
        /// </summary>
        public float GetWaterForFireSuppression(string tankId, float requestedAmount)
        {
            if (!_tanks.ContainsKey(tankId))
                return 0f;

            var tank = _tanks[tankId];
            
            // Any quality works for fire suppression
            float available = Mathf.Min(requestedAmount, tank.CurrentAmount);
            RemoveWater(tankId, available, out _);
            
            return available;
        }

        /// <summary>
        /// Use water for industrial cooling.
        /// </summary>
        public bool ProvideCoolingWater(string tankId, float amount, float temperature)
        {
            if (!_tanks.ContainsKey(tankId))
                return false;

            var tank = _tanks[tankId];
            
            if (tank.CurrentAmount < amount)
                return false;

            // Remove heated water, add back cooled water
            RemoveWater(tankId, amount, out _);
            tank.Temperature = Mathf.Max(tank.Temperature - temperature * 0.1f, 10f);
            AddWater(tankId, amount, tank.Quality);
            
            return true;
        }

        private void RecalculateTotals()
        {
            TotalStored = 0;
            foreach (var tank in _tanks.Values)
            {
                TotalStored += tank.CurrentAmount;
            }
        }

        private WaterQuality GetBaseQualityForType(WaterSourceType type)
        {
            switch (type)
            {
                case WaterSourceType.Rain:
                    return WaterQuality.Clean;
                case WaterSourceType.Well:
                    return WaterQuality.Murky;
                case WaterSourceType.River:
                    return WaterQuality.Murky;
                case WaterSourceType.IceMelt:
                    return WaterQuality.Clean;
                case WaterSourceType.Purified:
                    return WaterQuality.Pure;
                case WaterSourceType.Contaminated:
                    return WaterQuality.Contaminated;
                default:
                    return WaterQuality.Murky;
            }
        }

        private WaterQuality MixWaterQualities(WaterQuality q1, WaterQuality q2, float amount1, float amount2)
        {
            // Simple averaging - worse quality dominates
            int q1Val = (int)q1;
            int q2Val = (int)q2;
            
            float total = amount1 + amount2;
            float weightedAvg = (q1Val * amount1 + q2Val * amount2) / total;
            
            return (WaterQuality)Mathf.RoundToInt(weightedAvg);
        }

        private WaterQuality ImproveQuality(WaterQuality quality, int levels)
        {
            int val = (int)quality - levels;
            return (WaterQuality)Mathf.Max(val, (int)WaterQuality.Pure);
        }

        private WaterQuality DegradeQuality(WaterQuality quality, int levels)
        {
            int val = (int)quality + levels;
            return (WaterQuality)Mathf.Min(val, (int)WaterQuality.AnomalyTainted);
        }

        private bool IsContaminated(WaterQuality quality)
        {
            return quality == WaterQuality.Contaminated || 
                   quality == WaterQuality.Toxic || 
                   quality == WaterQuality.AnomalyTainted;
        }

        public enum PurificationMethod
        {
            Boiling,
            Chemical,
            UV,
            Filter
        }
    }

    /// <summary>
    /// Water source definition.
    /// </summary>
    public struct WaterSource
    {
        public string Id;
        public WaterSourceType Type;
        public Vector3 Position;
        public float OutputRate;
        public bool Active;
        public WaterQuality Quality;
    }

    /// <summary>
    /// Water pipe connection.
    /// </summary>
    public struct WaterPipe
    {
        public string Id;
        public string TankA;
        public string TankB;
        public float FlowRate;
        public bool IsBlocked;
        public bool HasLeak;
    }
}
