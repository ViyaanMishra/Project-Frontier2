using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Structural integrity system using load-bearing graph analysis.
    /// Tracks stress distribution and progressive collapse.
    /// </summary>
    public struct StructuralNode
    {
        public int nodeId;
        public float3 position;
        public float loadBearingCapacity; // Max weight before failure
        public float currentLoad;         // Current weight/stress
        public float stressRatio;         // currentLoad / capacity
        public bool isDestroyed;
        public int connectedNodeCount;
        public byte materialTier;
    }

    public class StructuralIntegritySystem : IDisposable
    {
        public const int MaxNodes = 4096;
        public const float GravityConstant = 9.81f;
        
        private NativeList<StructuralNode> _nodes;
        private NativeMultiHashMap<int, int> _connections; // nodeId -> connected nodeIds
        private NativeArray<float> _stressValues;
        private bool _needsRecalculation;
        
        public int NodeCount => _nodes.Length;
        
        public StructuralIntegritySystem()
        {
            _nodes = new NativeList<StructuralNode>(MaxNodes, Allocator.Persistent);
            _connections = new NativeMultiHashMap<int, int>(MaxNodes * 4, Allocator.Persistent);
            _stressValues = new NativeArray<float>(MaxNodes, Allocator.Persistent);
        }
        
        public int AddNode(float3 position, float capacity, byte materialTier)
        {
            int nodeId = _nodes.Length;
            var node = new StructuralNode
            {
                nodeId = nodeId,
                position = position,
                loadBearingCapacity = capacity,
                currentLoad = 0f,
                stressRatio = 0f,
                isDestroyed = false,
                connectedNodeCount = 0,
                materialTier = materialTier
            };
            _nodes.Add(node);
            _needsRecalculation = true;
            return nodeId;
        }
        
        public void ConnectNodes(int nodeA, int nodeB)
        {
            if (nodeA < 0 || nodeA >= _nodes.Length || nodeB < 0 || nodeB >= _nodes.Length)
                return;
                
            _connections.Add(nodeA, nodeB);
            _connections.Add(nodeB, nodeA);
            
            var nodeAData = _nodes[nodeA];
            var nodeBData = _nodes[nodeB];
            nodeAData.connectedNodeCount++;
            nodeBData.connectedNodeCount++;
            _nodes[nodeA] = nodeAData;
            _nodes[nodeB] = nodeBData;
            
            _needsRecalculation = true;
        }
        
        public void ApplyLoad(int nodeId, float load)
        {
            if (nodeId < 0 || nodeId >= _nodes.Length) return;
            
            var node = _nodes[nodeId];
            node.currentLoad += load;
            node.stressRatio = node.currentLoad / math.max(0.001f, node.loadBearingCapacity);
            _nodes[nodeId] = node;
            _needsRecalculation = true;
        }
        
        public void DestroyNode(int nodeId)
        {
            if (nodeId < 0 || nodeId >= _nodes.Length) return;
            
            var node = _nodes[nodeId];
            node.isDestroyed = true;
            node.loadBearingCapacity = 0f;
            node.currentLoad = 0f;
            node.stressRatio = 1f;
            _nodes[nodeId] = node;
            
            // Redistribute load to connected nodes
            if (_connections.ContainsKey(nodeId))
            {
                var connections = _connections.GetValuesForKey(nodeId);
                foreach (var connectedId in connections)
                {
                    if (connectedId >= 0 && connectedId < _nodes.Length && !_nodes[connectedId].isDestroyed)
                    {
                        // Transfer portion of destroyed node's load
                        float transferLoad = node.currentLoad / math.max(1, node.connectedNodeCount);
                        ApplyLoad(connectedId, transferLoad);
                    }
                }
            }
            
            _needsRecalculation = true;
        }
        
        [BurstCompile]
        public struct StressCalculationJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<StructuralNode> nodes;
            [WriteOnly] public NativeArray<float> stressOutput;
            
            public void Execute(int index)
            {
                var node = nodes[index];
                if (node.isDestroyed)
                {
                    stressOutput[index] = 1f;
                    return;
                }
                
                float ratio = node.currentLoad / math.max(0.001f, node.loadBearingCapacity);
                stressOutput[index] = math.clamp(ratio, 0f, 2f);
            }
        }
        
        public JobHandle CalculateStresses(JobHandle inputDeps)
        {
            var job = new StressCalculationJob
            {
                nodes = _nodes.AsDeferredJobArray(),
                stressOutput = _stressValues
            };
            
            return job.Schedule(_nodes.Length, 64, inputDeps);
        }
        
        public void SimulateCollapse(float deltaTime)
        {
            // Check for nodes exceeding capacity
            for (int i = 0; i < _nodes.Length; i++)
            {
                var node = _nodes[i];
                if (!node.isDestroyed && node.stressRatio > 1.0f)
                {
                    // Chance of failure based on overload
                    float failChance = (node.stressRatio - 1.0f) * deltaTime * 2f;
                    if (UnityEngine.Random.value < failChance)
                    {
                        DestroyNode(i);
                    }
                }
            }
        }
        
        public bool IsNodeStable(int nodeId)
        {
            if (nodeId < 0 || nodeId >= _nodes.Length) return false;
            var node = _nodes[nodeId];
            return !node.isDestroyed && node.stressRatio < 0.8f;
        }
        
        public float GetStressRatio(int nodeId)
        {
            if (nodeId < 0 || nodeId >= _nodes.Length) return 0f;
            return _nodes[nodeId].stressRatio;
        }
        
        public NativeSlice<StructuralNode> GetNodes() => _nodes.AsSlice();
        
        public void Dispose()
        {
            if (_nodes.IsCreated) _nodes.Dispose();
            if (_connections.IsCreated) _connections.Dispose();
            if (_stressValues.IsCreated) _stressValues.Dispose();
        }
    }
}
