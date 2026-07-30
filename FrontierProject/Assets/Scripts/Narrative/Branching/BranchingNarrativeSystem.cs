using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace FrontierProject.Narrative.Branching
{
    /// <summary>
    /// Advanced branching narrative system with multi-dimensional choice tracking,
    /// consequence propagation, and dynamic pathfinding through story graphs.
    /// Supports parallel storylines, convergence points, and player agency metrics.
    /// </summary>

    [Serializable]
    public struct BranchNode
    {
        public FixedString64Bytes NodeID;
        public FixedString64Bytes ParentNodeID;
        public DynamicBuffer<FixedString64Bytes> ChildNodeIDs;
        
        public NodeType Type;
        public FixedString512Bytes ContentRef;
        
        // Branch metadata
        public int Depth;
        public int GlobalSequenceIndex;
        
        // Convergence info
        public bool IsConvergencePoint;
        public DynamicBuffer<FixedString64Bytes> ConvergingFromNodes;
        
        // Visibility/availability
        public bool IsVisible;
        public bool IsLocked;
        public FixedString64Bytes LockConditionID;
        
        // Statistical tracking
        public int TimesVisited;
        public float AverageChoiceWeight;
        public double FirstVisitTime;
        public double LastVisitTime;
    }

    [Serializable]
    public enum NodeType
    {
        Start,
        Dialogue,
        Choice,
        Event,
        Cutscene,
        Gameplay,
        Convergence,
        End
    }

    [Serializable]
    public struct StoryChoice
    {
        public FixedString64Bytes ChoiceID;
        public FixedString256Bytes DisplayText;
        public FixedString64Bytes TargetNodeID;
        
        // Requirements
        public DynamicBuffer<StoryRequirement> Requirements;
        
        // Consequences
        public DynamicBuffer<StoryConsequence> ImmediateConsequences;
        public DynamicBuffer<DelayedConsequence> DelayedConsequences;
        
        // Metadata
        public ChoiceType Classification;
        public float MoralWeight;      // -1 (evil) to 1 (good)
        public float PragmaticWeight;  // -1 (selfless) to 1 (selfish)
        public float RiskLevel;        // 0 (safe) to 1 (dangerous)
        public float RewardPotential;  // 0 (none) to 1 (major)
        
        // AI hints
        public float AILikelihood;     // Predicted player choice probability
        public FixedString128Bytes AIRationale;
        
        // Presentation
        public FixedString64Bytes VoiceOverTag;
        public float DisplayDuration;
        public bool RequiresConfirmation;
        public FixedString64Bytes WarningMessage;
    }

    [Serializable]
    public struct StoryRequirement
    {
        public FixedString64Bytes VariableID;
        public RequirementType Type;
        public ComparisonOperator Operator;
        public float ThresholdValue;
        public FixedString64Bytes ReferenceVariableID; // For variable-to-variable comparison
        public bool InvertResult;
    }

    [Serializable]
    public enum RequirementType
    {
        VariableValue,
        VariableExists,
        FlagSet,
        QuestCompleted,
        ItemOwned,
        RelationshipLevel,
        SkillCheck,
        TimeBased,
        LocationBased,
        PreviousChoiceMade,
        RandomChance
    }

    [Serializable]
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith
    }

    [Serializable]
    public struct StoryConsequence
    {
        public FixedString64Bytes ConsequenceID;
        public ConsequenceType Type;
        public ConsequenceTiming Timing;
        
        // Effect data
        public FixedString64Bytes TargetVariableID;
        public float ValueChange;
        public FixedString64Bytes EventToTrigger;
        public Entity TargetEntity;
        public FixedString64Bytes QuestID;
        
        // Propagation
        public bool PropagatesToRelatedNodes;
        public float PropagationRange;
        public FixedString64Bytes PropagationFilter;
        
        // Reversibility
        public bool IsReversible;
        public FixedString64Bytes ReversalConditionID;
        public double ExpirationTime;
    }

    [Serializable]
    public struct DelayedConsequence
    {
        public FixedString64Bytes ConsequenceID;
        public StoryConsequence BaseConsequence;
        
        public TriggerType TriggerCondition;
        public FixedString64Bytes TriggerVariableID;
        public float TriggerThreshold;
        public double DelayDuration;
        public double ScheduledTriggerTime;
        
        public bool HasTriggered;
        public double ActualTriggerTime;
    }

    [Serializable]
    public enum ConsequenceType
    {
        VariableModify,
        EventTrigger,
        QuestStart,
        QuestComplete,
        QuestFail,
        EntitySpawn,
        EntityDespawn,
        RelationshipChange,
        FactionReputation,
        UnlockContent,
        LockContent,
        NarrativeFlag,
        EnvironmentalChange,
        CharacterStateChange,
        CutscenePlay,
        AudioPlay,
        VFXPlay
    }

    [Serializable]
    public enum ConsequenceTiming
    {
        Immediate,
        EndOfScene,
        NextNode,
        Delayed,
        Conditional,
        Persistent
    }

    [Serializable]
    public enum TriggerType
    {
        TimeElapsed,
        VariableThreshold,
        NodeReached,
        QuestState,
        CombatStart,
        CombatEnd,
        LocationEnter,
        LocationExit,
        Interaction
    }

    [Serializable]
    public enum ChoiceType
    {
        Moral,
        Pragmatic,
        Emotional,
        Strategic,
        Tactical,
        Social,
        Economic,
        Survival,
        Exploration,
        Lore,
        Humor,
        Romance,
        Betrayal,
        Sacrifice
    }

    public struct BranchingComponent : IComponentData
    {
        public Entity OwnerEntity;
        public FixedString64Bytes CurrentNodeID;
        public FixedString64Bytes ActiveSagaID;
        
        public DynamicBuffer<VisitedNode> VisitedNodes;
        public DynamicBuffer<PendingConsequence> PendingConsequences;
        public DynamicBuffer<AvailableChoice> AvailableChoices;
        
        public BranchMetrics Metrics;
        public double CurrentNodeEnterTime;
    }

    [Serializable]
    public struct VisitedNode
    {
        public FixedString64Bytes NodeID;
        public double VisitTime;
        public FixedString64Bytes ChoiceMadeID;
        public float TimeSpent;
        public int VisitCount;
    }

    [Serializable]
    public struct PendingConsequence
    {
        public FixedString64Bytes ConsequenceID;
        public StoryConsequence Consequence;
        public double ScheduledTime;
        public bool IsReady;
    }

    [Serializable]
    public struct AvailableChoice
    {
        public StoryChoice Choice;
        public bool IsAvailable;
        public FixedString128Bytes UnavailableReason;
        public float CalculatedWeight;
    }

    [Serializable]
    public struct BranchMetrics
    {
        public int TotalNodesVisited;
        public int UniquePathsExplored;
        public int ChoicesMade;
        public int ChoicesDeferred;
        public int DeadEndsReached;
        public int ConvergencesPassed;
        
        public float AverageDecisionTime;
        public float MoralAlignmentTrend;    // Cumulative moral weight
        public float PragmatismTrend;        // Cumulative pragmatic weight
        public float RiskTakingIndex;        // Average risk level of choices
        public float ExplorationScore;       // How much of the graph explored
        
        public DynamicBuffer<FixedString64Bytes> PathSignature; // Hash of path taken
    }

    public class BranchingNarrativeSystem : SystemBase
    {
        private NativeHashMap<FixedString64Bytes, BranchNode> _nodeRegistry;
        private NativeHashMap<FixedString64Bytes, DynamicBuffer<StoryChoice>> _choiceRegistry;
        
        protected override void OnCreate()
        {
            _nodeRegistry = new NativeHashMap<FixedString64Bytes, BranchNode>(1000, Allocator.Persistent);
            _choiceRegistry = new NativeHashMap<FixedString64Bytes, DynamicBuffer<StoryChoice>>(1000, Allocator.Persistent);
        }
        
        protected override void OnDestroy()
        {
            _nodeRegistry.Dispose();
            // Note: DynamicBuffers need special handling in production
        }
        
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var currentTime = SystemAPI.Time.ElapsedTime;
            
            Entities
                .WithAll<BranchingComponent>()
                .ForEach((ref BranchingComponent branchComp) =>
                {
                    // Process pending consequences
                    for (int i = branchComp.PendingConsequences.Length - 1; i >= 0; i--)
                    {
                        var pending = branchComp.PendingConsequences[i];
                        
                        if (currentTime >= pending.ScheduledTime)
                        {
                            // Execute consequence (would integrate with StoryEffects)
                            ExecuteConsequence(pending.Consequence, ref branchComp);
                            branchComp.PendingConsequences.RemoveAt(i);
                        }
                    }
                    
                    // Update current node timing
                    if (!string.IsNullOrEmpty(branchComp.CurrentNodeID.ToString()))
                    {
                        // Could update time spent metrics here
                    }
                }).WithoutBurst().Run();
        }
        
        private void ExecuteConsequence(StoryConsequence consequence, ref BranchingComponent branchComp)
        {
            // Placeholder: actual execution would call StoryEffects system
            switch (consequence.Type)
            {
                case ConsequenceType.VariableModify:
                    // Modify story variable
                    break;
                case ConsequenceType.EventTrigger:
                    // Trigger narrative event
                    break;
                case ConsequenceType.QuestStart:
                    // Start quest via NarrativeQuestBridge
                    break;
                // ... handle all types
            }
        }
        
        public void RegisterNode(BranchNode node)
        {
            if (!_nodeRegistry.ContainsKey(node.NodeID))
            {
                _nodeRegistry.Add(node.NodeID, node);
            }
        }
        
        public void NavigateToNode(Entity entity, FixedString64Bytes targetNodeID, FixedString64Bytes choiceID = default)
        {
            if (!EntityManager.Exists(entity)) return;
            if (!_nodeRegistry.ContainsKey(targetNodeID)) return;
            
            var branchComp = EntityManager.GetComponentData<BranchingComponent>(entity);
            
            // Record visit to current node before leaving
            if (!string.IsNullOrEmpty(branchComp.CurrentNodeID.ToString()))
            {
                var visitedEntry = UpdateVisitedNode(branchComp.CurrentNodeID, choiceID, branchComp);
                branchComp.VisitedNodes.Add(visitedEntry);
            }
            
            // Update metrics
            branchComp.Metrics.TotalNodesVisited++;
            branchComp.Metrics.ChoicesMade++;
            
            // Set new current node
            branchComp.CurrentNodeID = targetNodeID;
            branchComp.CurrentNodeEnterTime = SystemAPI.Time.ElapsedTime;
            
            // Refresh available choices for new node
            RefreshAvailableChoices(ref branchComp);
            
            EntityManager.SetComponentData(entity, branchComp);
        }
        
        private VisitedNode UpdateVisitedNode(FixedString64Bytes nodeID, FixedString64Bytes choiceID, BranchingComponent branchComp)
        {
            double currentTime = SystemAPI.Time.ElapsedTime;
            float timeSpent = (float)(currentTime - branchComp.CurrentNodeEnterTime);
            
            // Check if already visited
            for (int i = 0; i < branchComp.VisitedNodes.Length; i++)
            {
                var visited = branchComp.VisitedNodes[i];
                if (visited.NodeID.Equals(nodeID))
                {
                    visited.VisitCount++;
                    visited.LastVisitTime = currentTime;
                    visited.TimeSpent += timeSpent;
                    if (!string.IsNullOrEmpty(choiceID.ToString()))
                        visited.ChoiceMadeID = choiceID;
                    return visited;
                }
            }
            
            // New visit
            return new VisitedNode
            {
                NodeID = nodeID,
                VisitTime = currentTime,
                ChoiceMadeID = choiceID,
                TimeSpent = timeSpent,
                VisitCount = 1
            };
        }
        
        private void RefreshAvailableChoices(ref BranchingComponent branchComp)
        {
            branchComp.AvailableChoices.Clear();
            
            if (!_nodeRegistry.ContainsKey(branchComp.CurrentNodeID))
                return;
            
            var currentNode = _nodeRegistry[branchComp.CurrentNodeID];
            
            // Would load choices from registry and evaluate requirements
            // This is a simplified placeholder
        }
        
        public NativeArray<AvailableChoice> GetAvailableChoices(Entity entity, Allocator allocator)
        {
            if (!EntityManager.Exists(entity))
                return new NativeArray<AvailableChoice>(0, allocator);
            
            var branchComp = EntityManager.GetComponentData<BranchingComponent>(entity);
            
            var result = new NativeArray<AvailableChoice>(branchComp.AvailableChoices.Length, allocator);
            for (int i = 0; i < branchComp.AvailableChoices.Length; i++)
            {
                result[i] = branchComp.AvailableChoices[i];
            }
            return result;
        }
        
        public float CalculatePathDivergence(Entity entityA, Entity entityB)
        {
            // Compare path signatures to determine how different two playthroughs are
            if (!EntityManager.Exists(entityA) || !EntityManager.Exists(entityB))
                return 0f;
            
            var compA = EntityManager.GetComponentData<BranchingComponent>(entityA);
            var compB = EntityManager.GetComponentData<BranchingComponent>(entityB);
            
            // Simplified: compare visited node sets
            int commonNodes = 0;
            for (int i = 0; i < compA.VisitedNodes.Length; i++)
            {
                for (int j = 0; j < compB.VisitedNodes.Length; j++)
                {
                    if (compA.VisitedNodes[i].NodeID.Equals(compB.VisitedNodes[j].NodeID))
                    {
                        commonNodes++;
                        break;
                    }
                }
            }
            
            int totalUnique = compA.VisitedNodes.Length + compB.VisitedNodes.Length - commonNodes;
            if (totalUnique == 0) return 0f;
            
            return 1f - ((float)commonNodes / totalUnique);
        }
        
        public void ScheduleDelayedConsequence(Entity entity, DelayedConsequence delayedConseq)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var branchComp = EntityManager.GetComponentData<BranchingComponent>(entity);
            
            var pending = new PendingConsequence
            {
                ConsequenceID = delayedConseq.ConsequenceID,
                Consequence = delayedConseq.BaseConsequence,
                ScheduledTime = SystemAPI.Time.ElapsedTime + delayedConseq.DelayDuration,
                IsReady = false
            };
            
            branchComp.PendingConsequences.Add(pending);
            EntityManager.SetComponentData(entity, branchComp);
        }
    }
}
