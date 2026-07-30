using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.StoryGraph
{
    /// <summary>
    /// Core node type for the narrative graph.
    /// Supports branching, looping, conditional locking, and runtime injection.
    /// </summary>
    [Serializable]
    public struct StoryNode
    {
        public FixedString64Bytes Id;
        public FixedString512Bytes Title;
        public FixedString2048Bytes Content; // Dialogue or description
        
        public NativeArray<FixedString64Bytes> Conditions; // IDs of required conditions
        public NativeArray<FixedString64Bytes> Effects;    // IDs of effects to apply
        public NativeArray<StoryTransition> Transitions;   // Possible next nodes
        
        public NodeType Type;
        public int Priority;
        public float CooldownTicks;
        public bool IsOneTimeOnly;
        
        // Runtime state
        public bool IsCompleted;
        public double LastTriggeredTime;
        public int TriggerCount;
    }

    public enum NodeType
    {
        Dialogue,
        Event,
        QuestStart,
        QuestEnd,
        Cutscene,
        BranchPoint,
        Terminal
    }

    [Serializable]
    public struct StoryTransition
    {
        public FixedString64Bytes TargetNodeId;
        public FixedString64Bytes ConditionId; // Empty = always available
        public FixedString128Bytes Label;      // Text shown to player
        public int Weight;                     // For random selection
    }

    /// <summary>
    /// The central nervous system for scripted narrative events.
    /// Manages a Directed Acyclic Graph (DAG) of StoryNodes.
    /// </summary>
    public class StoryGraphEngine : IService
    {
        private NativeHashMap<FixedString64Bytes, StoryNode> _nodeRegistry;
        private NativeQueue<FixedString64Bytes> _activeNodeQueue;
        private StoryVariableStore _variableStore;
        
        public int Priority => 10; // High priority, runs before rendering

        public void Initialize()
        {
            _nodeRegistry = new NativeHashMap<FixedString64Bytes, StoryNode>(1024, Allocator.Persistent);
            _activeNodeQueue = new NativeQueue<FixedString64Bytes>(Allocator.Persistent);
            _variableStore = ServiceRegistry.Get<StoryVariableStore>();
            
            EventBus.Subscribe<StoryFlagSetEvent>(OnFlagSet);
        }

        public void Tick(double deltaTime)
        {
            // Process active nodes
            while (_activeNodeQueue.Count > 0)
            {
                var nodeId = _activeNodeQueue.Dequeue();
                if (_nodeRegistry.TryGetValue(nodeId, out var node))
                {
                    EvaluateNode(node);
                }
            }
        }

        public void Shutdown()
        {
            if (_nodeRegistry.IsCreated) _nodeRegistry.Dispose();
            if (_activeNodeQueue.IsCreated) _activeNodeQueue.Dispose();
        }

        /// <summary>
        /// Registers a new story node into the graph.
        /// </summary>
        public void RegisterNode(StoryNode node)
        {
            if (_nodeRegistry.ContainsKey(node.Id))
            {
                UnityEngine.Debug.LogWarning($"StoryNode {node.Id} already exists. Overwriting.");
            }
            _nodeRegistry[node.Id] = node;
        }

        /// <summary>
        /// Attempts to start a node. Returns true if conditions are met.
        /// </summary>
        public bool StartNode(FixedString64Bytes nodeId)
        {
            if (!_nodeRegistry.TryGetValue(nodeId, out var node))
            {
                UnityEngine.Debug.LogError($"StoryNode {nodeId} not found!");
                return false;
            }

            if (node.IsOneTimeOnly && node.IsCompleted) return false;
            if (node.CooldownTicks > 0 && (MasterClock.Instance.TotalTicks - node.LastTriggeredTime) < node.CooldownTicks) return false;

            // Check conditions
            bool conditionsMet = true;
            for (int i = 0; i < node.Conditions.Length; i++)
            {
                if (!_variableStore.EvaluateCondition(node.Conditions[i]))
                {
                    conditionsMet = false;
                    break;
                }
            }

            if (conditionsMet)
            {
                _activeNodeQueue.Enqueue(nodeId);
                return true;
            }

            return false;
        }

        private void EvaluateNode(StoryNode node)
        {
            UnityEngine.Debug.Log($"[Narrative] Executing Node: {node.Title}");
            
            // Apply effects
            for (int i = 0; i < node.Effects.Length; i++)
            {
                _variableStore.ExecuteEffect(node.Effects[i]);
            }

            node.IsCompleted = true;
            node.LastTriggeredTime = MasterClock.Instance.TotalTicks;
            node.TriggerCount++;
            _nodeRegistry[node.Id] = node;

            // Publish event for UI
            EventBus.Publish(new StoryNodeExecutedEvent { NodeId = node.Id, Type = node.Type });
        }

        private void OnFlagSet(StoryFlagSetEvent evt)
        {
            // Re-evaluate pending nodes when a flag changes
            // This could trigger chained reactions
        }

        public StoryNode GetNode(FixedString64Bytes id)
        {
            return _nodeRegistry.TryGetValue(id, out var node) ? node : default;
        }
    }

    #region Events
    public struct StoryNodeExecutedEvent : IEvent
    {
        public FixedString64Bytes NodeId;
        public NodeType Type;
    }
    #endregion
}
