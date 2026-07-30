using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace FrontierProject.Narrative.Consequences
{
    /// <summary>
    /// Advanced consequence propagation system that tracks ripple effects
    /// of player choices across time, space, and narrative dimensions.
    /// Supports cascading consequences, butterfly effects, and emergent outcomes.
    /// </summary>

    [Serializable]
    public struct ConsequenceChain
    {
        public FixedString64Bytes ChainID;
        public FixedString64Bytes OriginChoiceID;
        public FixedString64Bytes OriginNodeID;
        
        public DynamicBuffer<ConsequenceLink> Links;
        
        // Propagation metadata
        public int Depth;
        public float TotalMagnitude;
        public ConsequencePropagationType PropagationType;
        
        // Temporal data
        public double InitiationTime;
        public double LastPropagationTime;
        public double ExpectedCompletionTime;
        
        // State
        public ChainState State;
        public bool IsStabilized;
        public float StabilizationProgress;
    }

    [Serializable]
    public struct ConsequenceLink
    {
        public int LinkIndex;
        public FixedString64Bytes SourceConsequenceID;
        public FixedString64Bytes TargetConsequenceID;
        
        public FixedString64Bytes TriggerCondition;
        public float PropagationStrength;
        public float DecayFactor;
        
        public ConsequenceTiming Timing;
        public double ScheduledTime;
        public double ActualTriggerTime;
        
        public bool HasPropagated;
        public bool WasBlocked;
        public FixedString64Bytes BlockReason;
    }

    [Serializable]
    public enum ConsequencePropagationType
    {
        Linear,      // Single path progression
        Branching,   // Multiple parallel paths
        Converging,  // Multiple sources to single target
        Cascading,   // Exponential spread
        Cyclic,      // Feedback loops
        Networked    // Complex web of connections
    }

    [Serializable]
    public enum ChainState
    {
        Dormant,
        Active,
        Propagating,
        Stabilizing,
        Completed,
        Collapsed,
        Paradoxical
    }

    [Serializable]
    public struct RippleEffect
    {
        public FixedString64Bytes EffectID;
        public FixedString64Bytes SourceChainID;
        
        // Affected domains
        public bool AffectsCharacters;
        public bool AffectsFactions;
        public bool AffectsEnvironment;
        public bool AffectsQuests;
        public bool AffectsEconomy;
        public bool AffectsPolitics;
        public bool AffectsWorldState;
        
        // Magnitude by domain
        public float CharacterImpact;
        public float FactionImpact;
        public float EnvironmentImpact;
        public float QuestImpact;
        public float EconomyImpact;
        public float PoliticsImpact;
        public float WorldStateImpact;
        
        // Spatial extent
        public float3 OriginPosition;
        public float PropagationRadius;
        public FixedString64Bytes AffectedZoneID;
        
        // Temporal extent
        public double StartTime;
        public double PeakTime;
        public double EndTime;
        public DurationType DurationClassification;
        
        // Reversibility
        public bool IsReversible;
        public float ReversalCost;
        public FixedString64Bytes ReversalMethod;
    }

    [Serializable]
    public enum DurationType
    {
        Instant,
        ShortTerm,    // Minutes to hours
        MediumTerm,   // Days to weeks
        LongTerm,     // Months to years
        Permanent,
        Cyclical
    }

    [Serializable]
    public struct ButterflyEffect
    {
        public FixedString64Bytes EffectID;
        public FixedString64Bytes TrivialOriginID; // The small cause
        public FixedString64Bytes MajorOutcomeID;  // The significant effect
        
        public int CausalDistance;  // Number of intermediate steps
        public float AmplificationFactor;
        
        public FixedString128Bytes CausalDescription;
        
        public double OriginTime;
        public double OutcomeTime;
        public double TimeToManifest;
        
        public bool WasPredictable;
        public float PredictabilityScore;
        
        // Classification
        public ButterflyType Classification;
        public FixedString64Bytes ThematicResonance;
    }

    [Serializable]
    public enum ButterflyType
    {
        Ironic,           // Opposite of intended outcome
        Poetic,           // Thematically appropriate
        Tragic,           // Unintended harm
        Fortuitous,       // Unintended benefit
        Catastrophic,     // Massive negative impact
        Transformative,   // Fundamental change
        Subtle,           // Barely noticeable but significant
        Delayed           // Long time before manifestation
    }

    [Serializable]
    public struct ConsequenceWeb
    {
        public DynamicBuffer<ConsequenceNode> Nodes;
        public DynamicBuffer<ConsequenceEdge> Edges;
        
        // Analysis data
        public int TotalNodes;
        public int TotalEdges;
        public float ConnectivityDensity;
        public int CriticalPathLength;
        
        // Central nodes (most connected)
        public DynamicBuffer<FixedString64Bytes> HubNodes;
        
        // Vulnerable points
        public DynamicBuffer<FixedString64Bytes> FragileNodes;
    }

    [Serializable]
    public struct ConsequenceNode
    {
        public FixedString64Bytes NodeID;
        public FixedString64Bytes ConsequenceID;
        
        public NodeType Type;
        public int IncomingEdges;
        public int OutgoingEdges;
        
        public float CentralityScore;
        public float InfluenceRadius;
        
        public bool IsCritical;
        public bool IsResolved;
    }

    [Serializable]
    public struct ConsequenceEdge
    {
        public FixedString64Bytes EdgeID;
        public FixedString64Bytes SourceNodeID;
        public FixedString64Bytes TargetNodeID;
        
        public EdgeType Type;
        public float Weight;
        public float Probability;
        
        public bool IsCausal;
        public bool IsCorrelational;
        
        public FixedString64Bytes MediatingVariable;
    }

    [Serializable]
    public enum EdgeType
    {
        DirectCause,
        ContributingFactor,
        EnablingCondition,
        PreventingFactor,
        SideEffect,
        FeedbackLoop,
        Correlation
    }

    public struct ConsequenceComponent : IComponentData
    {
        public Entity OwnerEntity;
        public DynamicBuffer<ActiveConsequenceChain> ActiveChains;
        public DynamicBuffer<PendingRippleEffect> PendingRipples;
        public DynamicBuffer<DetectedButterflyEffect> DetectedButterflies;
        
        public ConsequenceWeb GlobalWeb;
        public ConsequenceMetrics Metrics;
    }

    [Serializable]
    public struct ActiveConsequenceChain
    {
        public ConsequenceChain Chain;
        public int CurrentLinkIndex;
        public float CurrentMagnitude;
        public bool IsPlayerAware;
    }

    [Serializable]
    public struct PendingRippleEffect
    {
        public RippleEffect Effect;
        public double ScheduledStartTime;
        public bool ConditionsMet;
    }

    [Serializable]
    public struct DetectedButterflyEffect
    {
        public ButterflyEffect Effect;
        public double DetectionTime;
        public bool IsDocumented;
        public FixedString64Bytes DocumentationRef;
    }

    [Serializable]
    public struct ConsequenceMetrics
    {
        public int TotalConsequencesTriggered;
        public int ChainsInitiated;
        public int ChainsCompleted;
        public int ChainsCollapsed;
        public int ParadoxesDetected;
        
        public float AverageChainDepth;
        public float AveragePropagationSpeed;
        public float ButterflyEffectFrequency;
        
        public float PlayerAgencyScore;      // How much control player feels
        public float ChaosIndex;            // How unpredictable outcomes are
        public float NarrativeCoherence;    // How well consequences fit theme
    }

    public class ConsequencePropagationSystem : SystemBase
    {
        private NativeHashMap<FixedString64Bytes, ConsequenceChain> _chainRegistry;
        private NativeList<ButterflyEffect> _knownButterflyPatterns;
        
        protected override void OnCreate()
        {
            _chainRegistry = new NativeHashMap<FixedString64Bytes, ConsequenceChain>(500, Allocator.Persistent);
            _knownButterflyPatterns = new NativeList<ButterflyEffect>(Allocator.Persistent);
            
            InitializeButterflyPatterns();
        }
        
        protected override void OnDestroy()
        {
            _chainRegistry.Dispose();
            _knownButterflyPatterns.Dispose();
        }
        
        private void InitializeButterflyPatterns()
        {
            // Pre-define common butterfly effect patterns
            _knownButterflyPatterns.Add(new ButterflyEffect
            {
                EffectID = "BUTTERFLY_MERCY_CHAIN",
                TrivialOriginID = "SPARE_ENEMY",
                MajorOutcomeID = "ALLIANCE_FORMED",
                CausalDistance = 5,
                AmplificationFactor = 10.0f,
                CausalDescription = "Sparing a low-level enemy leads to them recruiting allies who later save the player",
                Classification = ButterflyType.Fortuitous,
                ThematicResonance = "COMPASSION"
            });
            
            _knownButterflyPatterns.Add(new ButterflyEffect
            {
                EffectID = "BUTTERFLY_GREED_DOWNFALL",
                TrivialOriginID = "TAKE_EXTRA_LOOT",
                MajorOutcomeID = "FACTION_BETRAYAL",
                CausalDistance = 7,
                AmplificationFactor = 15.0f,
                CausalDescription = "Taking extra resources triggers supply chain issues leading to faction collapse",
                Classification = ButterflyType.Catastrophic,
                ThematicResonance = "GREED"
            });
        }
        
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var currentTime = SystemAPI.Time.ElapsedTime;
            
            Entities
                .WithAll<ConsequenceComponent>()
                .ForEach((ref ConsequenceComponent consComp) =>
                {
                    // Update active chains
                    for (int i = 0; i < consComp.ActiveChains.Length; i++)
                    {
                        var activeChain = consComp.ActiveChains[i];
                        
                        if (activeChain.Chain.State == ChainState.Propagating)
                        {
                            // Check if next link should trigger
                            if (activeChain.CurrentLinkIndex < activeChain.Chain.Links.Length)
                            {
                                var link = activeChain.Chain.Links[activeChain.CurrentLinkIndex];
                                
                                if (currentTime >= link.ScheduledTime && !link.HasPropagated)
                                {
                                    // Trigger next consequence in chain
                                    activeChain = PropagateChain(activeChain, link, currentTime);
                                    consComp.ActiveChains[i] = activeChain;
                                }
                            }
                            else
                            {
                                // Chain completed
                                activeChain.Chain.State = ChainState.Completed;
                                consComp.ActiveChains[i] = activeChain;
                            }
                        }
                        
                        // Update stabilization progress
                        if (activeChain.Chain.State == ChainState.Stabilizing)
                        {
                            activeChain.Chain.StabilizationProgress += (float)deltaTime * 0.1f;
                            if (activeChain.Chain.StabilizationProgress >= 1.0f)
                            {
                                activeChain.Chain.IsStabilized = true;
                                activeChain.Chain.State = ChainState.Completed;
                            }
                            consComp.ActiveChains[i] = activeChain;
                        }
                    }
                    
                    // Process pending ripples
                    for (int i = consComp.PendingRipples.Length - 1; i >= 0; i--)
                    {
                        var ripple = consComp.PendingRipples[i];
                        
                        if (currentTime >= ripple.ScheduledStartTime && ripple.ConditionsMet)
                        {
                            // Activate ripple effect
                            ActivateRippleEffect(ripple.Effect, ref consComp);
                            consComp.PendingRipples.RemoveAt(i);
                        }
                    }
                    
                    // Detect emerging butterfly effects
                    DetectButterflyEffects(ref consComp, currentTime);
                    
                }).WithoutBurst().Run();
        }
        
        private ActiveConsequenceChain PropagateChain(ActiveConsequenceChain activeChain, 
                                                       ConsequenceLink link, double currentTime)
        {
            link.HasPropagated = true;
            link.ActualTriggerTime = currentTime;
            activeChain.Chain.Links[activeChain.CurrentLinkIndex] = link;
            
            // Apply consequence effect here (would integrate with StoryEffects)
            
            activeChain.CurrentLinkIndex++;
            activeChain.CurrentMagnitude *= link.PropagationStrength;
            activeChain.Chain.LastPropagationTime = currentTime;
            
            // Check for decay
            if (activeChain.CurrentMagnitude < 0.01f)
            {
                activeChain.Chain.State = ChainState.Collapsed;
            }
            
            return activeChain;
        }
        
        private void ActivateRippleEffect(RippleEffect effect, ref ConsequenceComponent consComp)
        {
            // Apply effects to affected domains
            // This would integrate with various game systems
            
            consComp.Metrics.TotalConsequencesTriggered++;
        }
        
        private void DetectButterflyEffects(ref ConsequenceComponent consComp, double currentTime)
        {
            // Analyze consequence patterns to detect butterfly effects
            // Compare against known patterns
            
            for (int i = 0; i < _knownButterflyPatterns.Length; i++)
            {
                var pattern = _knownButterflyPatterns[i];
                
                // Check if pattern matches current state
                // Simplified placeholder logic
                bool patternMatched = false;
                
                if (patternMatched)
                {
                    var detected = new DetectedButterflyEffect
                    {
                        Effect = pattern,
                        DetectionTime = currentTime,
                        IsDocumented = false
                    };
                    consComp.DetectedButterflies.Add(detected);
                    
                    consComp.Metrics.ButterflyEffectFrequency++;
                }
            }
        }
        
        public void InitiateConsequenceChain(Entity entity, ConsequenceChain newChain)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var consComp = EntityManager.GetComponentData<ConsequenceComponent>(entity);
            
            newChain.InitiationTime = SystemAPI.Time.ElapsedTime;
            newChain.State = ChainState.Active;
            
            var activeChain = new ActiveConsequenceChain
            {
                Chain = newChain,
                CurrentLinkIndex = 0,
                CurrentMagnitude = newChain.TotalMagnitude,
                IsPlayerAware = false
            };
            
            consComp.ActiveChains.Add(activeChain);
            consComp.Metrics.ChainsInitiated++;
            
            if (!_chainRegistry.ContainsKey(newChain.ChainID))
            {
                _chainRegistry.Add(newChain.ChainID, newChain);
            }
            
            EntityManager.SetComponentData(entity, consComp);
        }
        
        public void ScheduleRippleEffect(Entity entity, RippleEffect effect, double delay = 0.0)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var consComp = EntityManager.GetComponentData<ConsequenceComponent>(entity);
            
            var pending = new PendingRippleEffect
            {
                Effect = effect,
                ScheduledStartTime = SystemAPI.Time.ElapsedTime + delay,
                ConditionsMet = true // Would check actual conditions
            };
            
            consComp.PendingRipples.Add(pending);
            EntityManager.SetComponentData(entity, consComp);
        }
        
        public float CalculateNarrativeCoherence(Entity entity)
        {
            if (!EntityManager.Exists(entity)) return 0.5f;
            
            var consComp = EntityManager.GetComponentData<ConsequenceComponent>(entity);
            
            // Analyze how well consequences align with established themes
            // Higher coherence = more thematically consistent outcomes
            
            float coherence = 0.5f; // Base value
            
            // Factor in chain completion rate
            if (consComp.Metrics.ChainsInitiated > 0)
            {
                float completionRate = (float)consComp.Metrics.ChainsCompleted / 
                                      consComp.Metrics.ChainsInitiated;
                coherence += completionRate * 0.3f;
            }
            
            // Penalize paradoxes
            coherence -= consComp.Metrics.ParadoxesDetected * 0.1f;
            
            return math.clamp(coherence, 0.0f, 1.0f);
        }
        
        public NativeArray<ConsequenceChain> GetActiveChains(Entity entity, Allocator allocator)
        {
            if (!EntityManager.Exists(entity))
                return new NativeArray<ConsequenceChain>(0, allocator);
            
            var consComp = EntityManager.GetComponentData<ConsequenceComponent>(entity);
            
            var result = new NativeArray<ConsequenceChain>(consComp.ActiveChains.Length, allocator);
            for (int i = 0; i < consComp.ActiveChains.Length; i++)
            {
                result[i] = consComp.ActiveChains[i].Chain;
            }
            return result;
        }
    }
}
