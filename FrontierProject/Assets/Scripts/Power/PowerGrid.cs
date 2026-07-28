using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Frontier.Power
{
    /// <summary>
    /// Represents a power network node (generator, consumer, or junction).
    /// </summary>
    public struct PowerNode
    {
        public int Id;
        public NodeType Type;
        public float Capacity;      // Max power output/storage
        public float CurrentLoad;   // Current power draw
        public float StoredEnergy;  // For batteries
        public bool Active;
        public int ConnectedTo;     // Parent node ID (-1 if root)
        public List<int> Children;  // Child node IDs
    }

    public enum NodeType
    {
        Generator,      // Produces power
        Consumer,       // Uses power
        Battery,        // Stores power
        Junction,       // Distributes power
        SolarPanel,     // Weather-dependent generator
        WindTurbine,    // Wind-dependent generator
        FusionReactor,  // High-output generator
        AnomalySiphon   // Exotic power source
    }

    /// <summary>
    /// Manages the global power grid with generation, distribution, and failure cascades.
    /// </summary>
    public class PowerGrid
    {
        private Dictionary<int, PowerNode> _nodes = new Dictionary<int, PowerNode>();
        private Dictionary<int, List<int>> _connections = new Dictionary<int, List<int>>();
        private int _nextNodeId = 0;
        
        // Global stats
        public float TotalGeneration { get; private set; }
        public float TotalConsumption { get; private set; }
        public float TotalStorage { get; private set; }
        public bool GridStable => TotalGeneration >= TotalConsumption;

        /// <summary>
        /// Add a power node to the grid.
        /// </summary>
        public int AddNode(NodeType type, float capacity, Vector3 position)
        {
            int id = _nextNodeId++;
            
            var node = new PowerNode
            {
                Id = id,
                Type = type,
                Capacity = capacity,
                CurrentLoad = 0,
                StoredEnergy = type == NodeType.Battery ? capacity : 0,
                Active = true,
                ConnectedTo = -1,
                Children = new List<int>()
            };

            _nodes[id] = node;
            _connections[id] = new List<int>();

            return id;
        }

        /// <summary>
        /// Connect two nodes with a power line.
        /// </summary>
        public void ConnectNodes(int nodeIdA, int nodeIdB)
        {
            if (!_nodes.ContainsKey(nodeIdA) || !_nodes.ContainsKey(nodeIdB))
                return;

            if (!_connections[nodeIdA].Contains(nodeIdB))
                _connections[nodeIdA].Add(nodeIdB);
            
            if (!_connections[nodeIdB].Contains(nodeIdA))
                _connections[nodeIdB].Add(nodeIdA);
        }

        /// <summary>
        /// Disconnect a node from the grid.
        /// </summary>
        public void DisconnectNode(int nodeId)
        {
            if (!_connections.ContainsKey(nodeId))
                return;

            foreach (var connectedId in _connections[nodeId])
            {
                if (_connections.ContainsKey(connectedId))
                    _connections[connectedId].Remove(nodeId);
            }

            _connections[nodeId].Clear();
        }

        /// <summary>
        /// Remove a node from the grid.
        /// </summary>
        public void RemoveNode(int nodeId)
        {
            DisconnectNode(nodeId);
            _nodes.Remove(nodeId);
            _connections.Remove(nodeId);
        }

        /// <summary>
        /// Set power consumption for a consumer node.
        /// </summary>
        public void SetConsumption(int nodeId, float consumption)
        {
            if (!_nodes.ContainsKey(nodeId))
                return;

            var node = _nodes[nodeId];
            if (node.Type != NodeType.Consumer)
                return;

            node.CurrentLoad = Mathf.Min(consumption, node.Capacity);
            _nodes[nodeId] = node;
        }

        /// <summary>
        /// Generate power for a generator node.
        /// </summary>
        public void GeneratePower(int nodeId, float efficiency = 1.0f)
        {
            if (!_nodes.ContainsKey(nodeId))
                return;

            var node = _nodes[nodeId];
            if (!IsGenerator(node.Type))
                return;

            // Apply efficiency modifiers (weather, damage, etc.)
            float actualOutput = node.Capacity * efficiency;
            node.CurrentLoad = -actualOutput; // Negative = generating
            _nodes[nodeId] = node;
        }

        /// <summary>
        /// Update solar panel output based on time and weather.
        /// </summary>
        public void UpdateSolarPanel(int nodeId, float sunAngle, bool isNight, bool isStormy)
        {
            if (!_nodes.ContainsKey(nodeId))
                return;

            var node = _nodes[nodeId];
            if (node.Type != NodeType.SolarPanel)
                return;

            float efficiency = 1.0f;

            if (isNight)
                efficiency = 0f;
            else
            {
                // Peak at noon (angle = 90 degrees)
                efficiency = Mathf.Sin(sunAngle * Mathf.Deg2Rad);
                
                if (isStormy)
                    efficiency *= 0.3f; // Cloud cover
            }

            node.CurrentLoad = -node.Capacity * efficiency;
            _nodes[nodeId] = node;
        }

        /// <summary>
        /// Update wind turbine output based on wind speed.
        /// </summary>
        public void UpdateWindTurbine(int nodeId, float windSpeed)
        {
            if (!_nodes.ContainsKey(nodeId))
                return;

            var node = _nodes[nodeId];
            if (node.Type != NodeType.WindTurbine)
                return;

            // Optimal wind speed: 10-25 m/s
            float efficiency = 0f;
            if (windSpeed >= 3f && windSpeed <= 30f)
            {
                efficiency = Mathf.InverseLerp(3f, 15f, windSpeed);
                if (windSpeed > 15f)
                    efficiency = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(15f, 30f, windSpeed));
            }

            node.CurrentLoad = -node.Capacity * efficiency;
            _nodes[nodeId] = node;
        }

        /// <summary>
        /// Charge or discharge a battery.
        /// </summary>
        public void UpdateBattery(int nodeId, float deltaTime)
        {
            if (!_nodes.ContainsKey(nodeId))
                return;

            var node = _nodes[nodeId];
            if (node.Type != NodeType.Battery)
                return;

            // Determine net flow
            float netFlow = -node.CurrentLoad; // Positive = charging, negative = discharging

            if (netFlow > 0)
            {
                // Charging
                float chargeAmount = netFlow * deltaTime;
                node.StoredEnergy = Mathf.Min(node.StoredEnergy + chargeAmount, node.Capacity);
            }
            else if (netFlow < 0 && node.StoredEnergy > 0)
            {
                // Discharging
                float dischargeAmount = Mathf.Abs(netFlow) * deltaTime;
                node.StoredEnergy = Mathf.Max(node.StoredEnergy - dischargeAmount, 0);
                
                // If battery depleted, reduce output
                if (node.StoredEnergy <= 0)
                    node.CurrentLoad = 0;
            }

            _nodes[nodeId] = node;
        }

        /// <summary>
        /// Calculate total grid statistics.
        /// </summary>
        public void RecalculateGrid()
        {
            TotalGeneration = 0;
            TotalConsumption = 0;
            TotalStorage = 0;

            foreach (var kvp in _nodes)
            {
                var node = kvp.Value;
                
                if (node.CurrentLoad < 0)
                    TotalGeneration += Mathf.Abs(node.CurrentLoad);
                else
                    TotalConsumption += node.CurrentLoad;

                if (node.Type == NodeType.Battery)
                    TotalStorage += node.StoredEnergy;
            }
        }

        /// <summary>
        /// Simulate power cascade failure.
        /// </summary>
        public List<int> SimulateCascadeFailure(int failedNodeId)
        {
            var affectedNodes = new List<int>();
            
            if (!_nodes.ContainsKey(failedNodeId))
                return affectedNodes;

            // Mark the failed node
            var failedNode = _nodes[failedNodeId];
            failedNode.Active = false;
            _nodes[failedNodeId] = failedNode;
            affectedNodes.Add(failedNodeId);

            // Propagate to children
            PropagateFailure(failedNodeId, affectedNodes);

            return affectedNodes;
        }

        private void PropagateFailure(int nodeId, List<int> affected)
        {
            if (!_connections.ContainsKey(nodeId))
                return;

            foreach (var connectedId in _connections[nodeId])
            {
                if (affected.Contains(connectedId))
                    continue;

                var node = _nodes[connectedId];
                
                // Check if this node has alternative power sources
                bool hasAlternative = false;
                foreach (var otherConnection in _connections[connectedId])
                {
                    if (otherConnection != nodeId && 
                        _nodes.ContainsKey(otherConnection) && 
                        _nodes[otherConnection].Active &&
                        IsGenerator(_nodes[otherConnection].Type))
                    {
                        hasAlternative = true;
                        break;
                    }
                }

                if (!hasAlternative && node.Type == NodeType.Consumer)
                {
                    node.Active = false;
                    _nodes[connectedId] = node;
                    affected.Add(connectedId);
                    
                    // Continue propagation
                    PropagateFailure(connectedId, affected);
                }
            }
        }

        /// <summary>
        /// Handle EMP event - disable all electronics.
        /// </summary>
        public List<int> ApplyEMP(float3 epicenter, float radius)
        {
            var disabledNodes = new List<int>();

            foreach (var kvp in _nodes)
            {
                // Get node position (would need to store it - simplified here)
                // In production, would check distance to epicenter
                
                var node = kvp.Value;
                if (node.Type != NodeType.Junction) // Junctions are passive
                {
                    node.Active = false;
                    node.CurrentLoad = 0;
                    _nodes[kvp.Key] = node;
                    disabledNodes.Add(kvp.Key);
                }
            }

            return disabledNodes;
        }

        private bool IsGenerator(NodeType type)
        {
            return type == NodeType.Generator ||
                   type == NodeType.SolarPanel ||
                   type == NodeType.WindTurbine ||
                   type == NodeType.FusionReactor ||
                   type == NodeType.AnomalySiphon;
        }

        /// <summary>
        /// Get priority-sorted list of consumers for load shedding.
        /// </summary>
        public List<int> GetConsumersByPriority()
        {
            var consumers = new List<int>();
            
            foreach (var kvp in _nodes)
            {
                if (kvp.Value.Type == NodeType.Consumer)
                    consumers.Add(kvp.Key);
            }

            // Sort by priority (lower current load = lower priority = shed first)
            consumers.Sort((a, b) => 
            {
                float loadA = _nodes[a].CurrentLoad;
                float loadB = _nodes[b].CurrentLoad;
                return loadA.CompareTo(loadB);
            });

            return consumers;
        }

        /// <summary>
        /// Perform load shedding to stabilize the grid.
        /// </summary>
        public List<int> PerformLoadShedding()
        {
            var shedNodes = new List<int>();
            
            if (GridStable)
                return shedNodes;

            var consumers = GetConsumersByPriority();
            float excessLoad = TotalConsumption - TotalGeneration;

            foreach (var consumerId in consumers)
            {
                if (excessLoad <= 0)
                    break;

                var node = _nodes[consumerId];
                excessLoad -= node.CurrentLoad;
                node.CurrentLoad = 0;
                node.Active = false;
                _nodes[consumerId] = node;
                shedNodes.Add(consumerId);
            }

            RecalculateGrid();
            return shedNodes;
        }

        public void Dispose()
        {
            _nodes.Clear();
            _connections.Clear();
        }
    }
}
