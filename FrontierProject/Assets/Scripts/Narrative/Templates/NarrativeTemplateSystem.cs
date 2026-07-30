using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace FrontierProject.Narrative.Templates
{
    /// <summary>
    /// Advanced narrative template system for procedural story generation.
    /// Provides reusable story patterns, archetypal structures, and 
    /// parametric templates for dynamic content creation.
    /// </summary>

    [Serializable]
    public struct NarrativeTemplate
    {
        public FixedString64Bytes TemplateID;
        public FixedString128Bytes DisplayName;
        public FixedString512Bytes Description;
        
        // Template classification
        public TemplateCategory Category;
        public TemplateComplexity Complexity;
        public float EstimatedDuration;
        
        // Structural data
        public DynamicBuffer<TemplateSlot> Slots;
        public DynamicBuffer<TemplateConstraint> Constraints;
        public DynamicBuffer<TemplateVariant> Variants;
        
        // Archetypal resonance
        public DynamicBuffer<ArchetypeReference> Archetypes;
        
        // Generation parameters
        public GenerationMode DefaultMode;
        public float RandomnessFactor;
        public int MinimumPlayerLevel;
        
        // Quality metrics
        public float CoherenceScore;
        public float EngagementScore;
        public int TimesUsed;
        public float SuccessRate;
    }

    [Serializable]
    public enum TemplateCategory
    {
        // Story structures
        HeroJourney,
        Redemption,
        Tragedy,
        Comedy,
        Mystery,
        Romance,
        Horror,
        ComingOfAge,
        Revenge,
        Survival,
        Discovery,
        Transformation,
        
        // Scene types
        Introduction,
        Confrontation,
        Resolution,
        Twist,
        Revelation,
        Climax,
        FallingAction,
        
        // Character beats
        MeetCute,
        Betrayal,
        Sacrifice,
        Reunion,
        Confession,
        Realization,
        
        // Environmental
        Exploration,
        Puzzle,
        Chase,
        Battle,
        Negotiation,
        Escape
    }

    [Serializable]
    public enum TemplateComplexity
    {
        Simple,      // Single beat, few elements
        Standard,    // Multiple connected beats
        Complex,     // Interwoven subplots
        Epic,        // Multi-act structure
        Saga         // Campaign-spanning
    }

    [Serializable]
    public struct TemplateSlot
    {
        public FixedString64Bytes SlotID;
        public SlotType Type;
        public FixedString128Bytes Label;
        
        // Content requirements
        public DynamicBuffer<ContentType> AcceptedTypes;
        public int MinimumCount;
        public int MaximumCount;
        
        // Role in template
        public SlotRole Role;
        public float ImportanceWeight;
        
        // Default/fallback
        public FixedString64Bytes DefaultContentID;
        public bool IsRequired;
        
        // Relationships
        public DynamicBuffer<FixedString64Bytes> LinkedSlots;
        public FixedString64Bytes DependencySlotID;
    }

    [Serializable]
    public enum SlotType
    {
        Character,
        Location,
        Event,
        Item,
        Dialogue,
        Action,
        Emotion,
        Theme,
        Conflict,
        Resolution,
        Obstacle,
        Ally,
        Enemy,
        Mentor,
        McGuffin
    }

    [Serializable]
    public enum SlotRole
    {
        Protagonist,
        Antagonist,
        Support,
        Catalyst,
        Obstacle,
        Reward,
        Information,
        Transportation,
        Setting,
        Atmosphere,
        Symbol,
        Foreshadowing
    }

    [Serializable]
    public enum ContentType
    {
        NPC,
        Player,
        Faction,
        Building,
        Region,
        Quest,
        Item,
        Creature,
        Vehicle,
        Weather,
        TimeOfDay,
        Music,
        Cutscene
    }

    [Serializable]
    public struct TemplateConstraint
    {
        public FixedString64Bytes ConstraintID;
        public ConstraintType Type;
        
        // Constraint definition
        public FixedString64Bytes SubjectSlotID;
        public FixedString64Bytes ObjectSlotID;
        public FixedString512Bytes Expression;
        
        // Evaluation
        public ConstraintSeverity Severity;
        public bool IsHardConstraint;
        public float Weight;
        
        // Error handling
        public FixedString128Bytes ViolationMessage;
        public FixedString64Bytes FallbackTemplateID;
    }

    [Serializable]
    public enum ConstraintType
    {
        // Relationship constraints
        RequiresRelationship,
        ForbidsRelationship,
        MinimumRelationshipLevel,
        
        // Attribute constraints
        RequiresAttribute,
        ForbidsAttribute,
        AttributeRange,
        
        // State constraints
        RequiresState,
        ForbidsState,
        StateTransition,
        
        // Temporal constraints
        TimeOfDayRestriction,
        SequenceOrder,
        CooldownPeriod,
        
        // Spatial constraints
        LocationProximity,
        LocationExclusion,
        TravelTime,
        
        // Logical constraints
        MutualExclusion,
        RequiresAll,
        RequiresAny,
        IfThen,
        
        // Narrative constraints
        ToneConsistency,
        ThemeAlignment,
        PacingRequirement,
        DifficultyCurve
    }

    [Serializable]
    public enum ConstraintSeverity
    {
        Critical,  // Must be satisfied
        High,      // Strongly preferred
        Medium,    // Moderately preferred
        Low,       // Slightly preferred
        Optional   // Nice to have
    }

    [Serializable]
    public struct TemplateVariant
    {
        public FixedString64Bytes VariantID;
        public FixedString128Bytes VariantName;
        
        // Variation type
        public VariationType Type;
        public float ProbabilityWeight;
        
        // Modifications
        public DynamicBuffer<SlotModification> Modifications;
        public DynamicBuffer<ConstraintModification> ConstraintMods;
        
        // Conditions
        public DynamicBuffer<VariantCondition> Conditions;
        
        // Metadata
        public bool IsUnlocked;
        public FixedString64Bytes UnlockRequirement;
    }

    [Serializable]
    public enum VariationType
    {
        Substitution,    // Replace elements
        Addition,        // Add new elements
        Removal,         // Remove elements
        Reordering,      // Change sequence
        Intensification, // Increase intensity
        Mitigation,      // Decrease intensity
        Inversion,       // Reverse roles/outcomes
        Fusion,          // Combine with other template
        Branching        // Create alternate path
    }

    [Serializable]
    public struct SlotModification
    {
        public FixedString64Bytes TargetSlotID;
        public ModificationOperation Operation;
        public FixedString64Bytes NewContentID;
        public float ValueDelta;
        public FixedString512Bytes ScriptedChange;
    }

    [Serializable]
    public enum ModificationOperation
    {
        Replace,
        Add,
        Remove,
        Modify,
        Swap,
        Duplicate,
        Randomize
    }

    [Serializable]
    public struct ConstraintModification
    {
        public FixedString64Bytes TargetConstraintID;
        public ConstraintModOperation Operation;
        public float NewWeight;
        public bool NewIsRequired;
    }

    [Serializable]
    public enum ConstraintModOperation
    {
        Tighten,
        Relax,
        Invert,
        Remove,
        Add
    }

    [Serializable]
    public struct VariantCondition
    {
        public FixedString64Bytes ConditionID;
        public FixedString64Bytes VariableID;
        public ComparisonOperator Operator;
        public float ThresholdValue;
        
        public bool IsMet;
        public double LastCheckTime;
    }

    [Serializable]
    public struct ArchetypeReference
    {
        public FixedString64Bytes ArchetypeID;
        public string ArchetypeName;
        public float ResonanceStrength;
        public FixedString64Bytes AssociatedSlotID;
        
        // Jungian archetypes support
        public JungianArchetype JungianType;
        public CampbellStage CampbellStage;
    }

    [Serializable]
    public enum JungianArchetype
    {
        Self,
        Shadow,
        Anima,
        Animus,
        Persona,
        Hero,
        Mentor,
        Trickster,
        Child,
        Mother,
        Father,
        WiseOldMan,
        GreatMother,
        ShadowSelf
    }

    [Serializable]
    public enum CampbellStage
    {
        OrdinaryWorld,
        CallToAdventure,
        RefusalOfCall,
        MeetingMentor,
        CrossingThreshold,
        TestsAlliesEnemies,
        ApproachInmostCave,
        Ordeal,
        Reward,
        RoadBack,
        Resurrection,
        ReturnWithElixir
    }

    [Serializable]
    public enum GenerationMode
    {
        Deterministic,   // Fixed output
        WeightedRandom,  // Probability-based
        Adaptive,        // Player-state aware
        Reactive,        // Context-sensitive
        Emergent,        // System-driven
        Collaborative    // Player-input guided
    }

    public struct TemplateComponent : IComponentData
    {
        public Entity OwnerEntity;
        public DynamicBuffer<ActiveTemplateInstance> ActiveInstances;
        public DynamicBuffer<GeneratedContent> GeneratedContent;
        
        public TemplateMetrics Metrics;
        public TemplateGenerationState GenerationState;
    }

    [Serializable]
    public struct ActiveTemplateInstance
    {
        public FixedString64Bytes InstanceID;
        public FixedString64Bytes TemplateID;
        public FixedString64Bytes VariantID;
        
        public DynamicBuffer<FilledSlot> FilledSlots;
        public DynamicBuffer<ActiveConstraint> ActiveConstraints;
        
        public InstantiationMode Mode;
        public float CompletionProgress;
        public GenerationQuality Quality;
        
        public double StartTime;
        public double ExpectedEndTime;
    }

    [Serializable]
    public struct FilledSlot
    {
        public FixedString64Bytes SlotID;
        public FixedString64Bytes ContentID;
        public Entity ContentEntity;
        public float FillQuality;
        public double FillTime;
    }

    [Serializable]
    public struct ActiveConstraint
    {
        public FixedString64Bytes ConstraintID;
        public bool IsSatisfied;
        public float SatisfactionLevel;
        public FixedString128Bytes FailureReason;
    }

    [Serializable]
    public struct GeneratedContent
    {
        public FixedString64Bytes ContentID;
        public ContentType Type;
        public Entity Entity;
        public FixedString64Bytes SourceTemplateID;
        public FixedString64Bytes SourceSlotID;
        
        public double GenerationTime;
        public float QualityScore;
    }

    [Serializable]
    public enum InstantiationMode
    {
        Immediate,
        Gradual,
        OnDemand,
        Deferred,
        Streaming
    }

    [Serializable]
    public enum GenerationQuality
    {
        Failed,
        Poor,
        Acceptable,
        Good,
        Excellent,
        Masterpiece
    }

    [Serializable]
    public struct TemplateMetrics
    {
        public int TemplatesUsed;
        public int InstancesGenerated;
        public int ConstraintsSatisfied;
        public int ConstraintsViolated;
        public int VariantsUnlocked;
        
        public float AverageQuality;
        public float AverageCompletionTime;
        public float PlayerSatisfactionEstimate;
        
        public DynamicBuffer<TemplateUsageCount> UsageStats;
    }

    [Serializable]
    public struct TemplateUsageCount
    {
        public FixedString64Bytes TemplateID;
        public int UsageCount;
        public float AverageRating;
    }

    [Serializable]
    public struct TemplateGenerationState
    {
        public FixedString64Bytes CurrentTemplateID;
        public int CurrentSlotIndex;
        public int TotalSlots;
        
        public bool IsGenerating;
        public bool HasErrors;
        public DynamicBuffer<GenerationError> Errors;
        
        public float ProgressPercentage;
    }

    [Serializable]
    public struct GenerationError
    {
        public FixedString64Bytes ErrorID;
        public FixedString128Bytes Message;
        public FixedString64Bytes SlotID;
        public FixedString64Bytes ConstraintID;
        public ErrorSeverity Severity;
        public double Timestamp;
    }

    [Serializable]
    public enum ErrorSeverity
    {
        Warning,
        Error,
        Critical,
        Fatal
    }

    public class NarrativeTemplateSystem : SystemBase
    {
        private NativeHashMap<FixedString64Bytes, NarrativeTemplate> _templateRegistry;
        private NativeList<NarrativeTemplate> _registeredTemplates;
        
        protected override void OnCreate()
        {
            _templateRegistry = new NativeHashMap<FixedString64Bytes, NarrativeTemplate>(500, Allocator.Persistent);
            _registeredTemplates = new NativeList<NarrativeTemplate>(Allocator.Persistent);
            
            InitializeDefaultTemplates();
        }
        
        protected override void OnDestroy()
        {
            _templateRegistry.Dispose();
            _registeredTemplates.Dispose();
        }
        
        private void InitializeDefaultTemplates()
        {
            // Register core narrative templates
            RegisterTemplate(CreateHeroJourneyTemplate());
            RegisterTemplate(CreateRedemptionArcTemplate());
            RegisterTemplate(CreateTragedyTemplate());
            RegisterTemplate(CreateMysteryTemplate());
            RegisterTemplate(CreateRomanceTemplate());
            RegisterTemplate(CreateRevengeTemplate());
            RegisterTemplate(CreateSurvivalTemplate());
            RegisterTemplate(CreateDiscoveryTemplate());
            RegisterTemplate(createBetrayalTemplate());
            RegisterTemplate(CreateSacrificeTemplate());
        }
        
        private void RegisterTemplate(NarrativeTemplate template)
        {
            if (!_templateRegistry.ContainsKey(template.TemplateID))
            {
                _templateRegistry.Add(template.TemplateID, template);
                _registeredTemplates.Add(template);
            }
        }
        
        private NarrativeTemplate CreateHeroJourneyTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_HERO_JOURNEY",
                DisplayName = "The Hero's Journey",
                Description = "Classic monomyth structure following Campbell's stages",
                Category = TemplateCategory.HeroJourney,
                Complexity = TemplateComplexity.Epic,
                EstimatedDuration = 120.0f,
                DefaultMode = GenerationMode.Adaptive,
                RandomnessFactor = 0.3f,
                MinimumPlayerLevel = 1,
                CoherenceScore = 0.95f,
                EngagementScore = 0.9f,
                TimesUsed = 0,
                SuccessRate = 0.92f
            };
        }
        
        private NarrativeTemplate CreateRedemptionArcTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_REDEMPTION",
                DisplayName = "Path to Redemption",
                Description = "Fallen character seeks and achieves redemption",
                Category = TemplateCategory.Redemption,
                Complexity = TemplateComplexity.Complex,
                EstimatedDuration = 90.0f,
                DefaultMode = GenerationMode.Adaptive,
                RandomnessFactor = 0.4f,
                MinimumPlayerLevel = 5,
                CoherenceScore = 0.88f,
                EngagementScore = 0.85f,
                TimesUsed = 0,
                SuccessRate = 0.87f
            };
        }
        
        private NarrativeTemplate CreateTragedyTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_TRAGEDY",
                DisplayName = "Tragic Downfall",
                Description = "Noble protagonist falls due to fatal flaw",
                Category = TemplateCategory.Tragedy,
                Complexity = TemplateComplexity.Complex,
                EstimatedDuration = 75.0f,
                DefaultMode = GenerationMode.Deterministic,
                RandomnessFactor = 0.2f,
                MinimumPlayerLevel = 10,
                CoherenceScore = 0.92f,
                EngagementScore = 0.8f,
                TimesUsed = 0,
                SuccessRate = 0.85f
            };
        }
        
        private NarrativeTemplate CreateMysteryTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_MYSTERY",
                DisplayName = "Unraveling Mystery",
                Description = "Progressive revelation of hidden truth",
                Category = TemplateCategory.Mystery,
                Complexity = TemplateComplexity.Complex,
                EstimatedDuration = 60.0f,
                DefaultMode = GenerationMode.Reactive,
                RandomnessFactor = 0.5f,
                MinimumPlayerLevel = 3,
                CoherenceScore = 0.9f,
                EngagementScore = 0.88f,
                TimesUsed = 0,
                SuccessRate = 0.89f
            };
        }
        
        private NarrativeTemplate CreateRomanceTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_ROMANCE",
                DisplayName = "Stars Align",
                Description = "Development of romantic relationship through obstacles",
                Category = TemplateCategory.Romance,
                Complexity = TemplateComplexity.Standard,
                EstimatedDuration = 80.0f,
                DefaultMode = GenerationMode.Adaptive,
                RandomnessFactor = 0.4f,
                MinimumPlayerLevel = 1,
                CoherenceScore = 0.85f,
                EngagementScore = 0.82f,
                TimesUsed = 0,
                SuccessRate = 0.8f
            };
        }
        
        private NarrativeTemplate CreateRevengeTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_REVENGE",
                DisplayName = "Vengeance Path",
                Description = "Quest for revenge with moral complications",
                Category = TemplateCategory.Revenge,
                Complexity = TemplateComplexity.Complex,
                EstimatedDuration = 70.0f,
                DefaultMode = GenerationMode.Adaptive,
                RandomnessFactor = 0.35f,
                MinimumPlayerLevel = 8,
                CoherenceScore = 0.87f,
                EngagementScore = 0.86f,
                TimesUsed = 0,
                SuccessRate = 0.84f
            };
        }
        
        private NarrativeTemplate CreateSurvivalTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_SURVIVAL",
                DisplayName = "Against All Odds",
                Description = "Struggle to survive extreme circumstances",
                Category = TemplateCategory.Survival,
                Complexity = TemplateComplexity.Standard,
                EstimatedDuration = 45.0f,
                DefaultMode = GenerationMode.Reactive,
                RandomnessFactor = 0.6f,
                MinimumPlayerLevel = 1,
                CoherenceScore = 0.82f,
                EngagementScore = 0.88f,
                TimesUsed = 0,
                SuccessRate = 0.9f
            };
        }
        
        private NarrativeTemplate CreateDiscoveryTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_DISCOVERY",
                DisplayName = "Hidden Truth",
                Description = "Journey of discovery leading to paradigm shift",
                Category = TemplateCategory.Discovery,
                Complexity = TemplateComplexity.Complex,
                EstimatedDuration = 65.0f,
                DefaultMode = GenerationMode.Adaptive,
                RandomnessFactor = 0.45f,
                MinimumPlayerLevel = 5,
                CoherenceScore = 0.89f,
                EngagementScore = 0.87f,
                TimesUsed = 0,
                SuccessRate = 0.88f
            };
        }
        
        private NarrativeTemplate createBetrayalTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_BETRAYAL",
                DisplayName = "Trust Broken",
                Description = "Shocking betrayal from unexpected source",
                Category = TemplateCategory.ComingOfAge,
                Complexity = TemplateComplexity.Standard,
                EstimatedDuration = 40.0f,
                DefaultMode = GenerationMode.WeightedRandom,
                RandomnessFactor = 0.5f,
                MinimumPlayerLevel = 3,
                CoherenceScore = 0.84f,
                EngagementScore = 0.9f,
                TimesUsed = 0,
                SuccessRate = 0.86f
            };
        }
        
        private NarrativeTemplate CreateSacrificeTemplate()
        {
            return new NarrativeTemplate
            {
                TemplateID = "TMPL_SACRIFICE",
                DisplayName = "Ultimate Price",
                Description = "Meaningful sacrifice for greater good",
                Category = TemplateCategory.Transformation,
                Complexity = TemplateComplexity.Standard,
                EstimatedDuration = 35.0f,
                DefaultMode = GenerationMode.Deterministic,
                RandomnessFactor = 0.25f,
                MinimumPlayerLevel = 10,
                CoherenceScore = 0.91f,
                EngagementScore = 0.89f,
                TimesUsed = 0,
                SuccessRate = 0.93f
            };
        }
        
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var currentTime = SystemAPI.Time.ElapsedTime;
            
            Entities
                .WithAll<TemplateComponent>()
                .ForEach((ref TemplateComponent tmplComp) =>
                {
                    // Update active template instances
                    for (int i = 0; i < tmplComp.ActiveInstances.Length; i++)
                    {
                        var instance = tmplComp.ActiveInstances[i];
                        
                        // Update completion progress based on filled slots
                        if (instance.FilledSlots.Length > 0)
                        {
                            // Calculate progress
                            // Would check constraint satisfaction
                        }
                        
                        tmplComp.ActiveInstances[i] = instance;
                    }
                    
                    // Check for generation errors
                    if (tmplComp.GenerationState.IsGenerating)
                    {
                        // Validate current slot filling
                        // Check constraints
                    }
                    
                }).WithoutBurst().Run();
        }
        
        public ActiveTemplateInstance InstantiateTemplate(Entity entity, FixedString64Bytes templateID, 
                                                          GenerationMode mode = GenerationMode.Adaptive)
        {
            if (!EntityManager.Exists(entity)) 
                return new ActiveTemplateInstance();
            
            if (!_templateRegistry.ContainsKey(templateID))
                return new ActiveTemplateInstance();
            
            var template = _templateRegistry[templateID];
            var tmplComp = EntityManager.GetComponentData<TemplateComponent>(entity);
            
            var instance = new ActiveTemplateInstance
            {
                InstanceID = $"INST_{templateID}_{currentTime()}",
                TemplateID = templateID,
                VariantID = "",
                Mode = mode,
                CompletionProgress = 0,
                Quality = GenerationQuality.Acceptable,
                StartTime = SystemAPI.Time.ElapsedTime,
                ExpectedEndTime = SystemAPI.Time.ElapsedTime + template.EstimatedDuration
            };
            
            // Initialize slots
            for (int i = 0; i < template.Slots.Length; i++)
            {
                // Would fill slots based on mode
            }
            
            tmplComp.ActiveInstances.Add(instance);
            tmplComp.Metrics.TemplatesUsed++;
            tmplComp.Metrics.InstancesGenerated++;
            
            EntityManager.SetComponentData(entity, tmplComp);
            
            return instance;
        }
        
        public void RegisterTemplate(NarrativeTemplate template)
        {
            if (!_templateRegistry.ContainsKey(template.TemplateID))
            {
                _templateRegistry.Add(template.TemplateID, template);
                _registeredTemplates.Add(template);
            }
        }
        
        public NativeArray<FixedString64Bytes> GetAllTemplateIDs(Allocator allocator)
        {
            var result = new NativeArray<FixedString64Bytes>(_templateRegistry.Count(), allocator);
            int index = 0;
            
            foreach (var kvp in _templateRegistry)
            {
                result[index++] = kvp.Key;
            }
            
            return result;
        }
        
        public float CalculateTemplateFit(NarrativeTemplate template, Entity contextEntity)
        {
            // Evaluate how well a template fits current game state
            float fitScore = 0.5f;
            
            // Factor in:
            // - Player level vs minimum requirement
            // - Current theme alignment
            // - Recent template usage (avoid repetition)
            // - Available characters/locations
            // - Narrative pacing needs
            
            return math.clamp(fitScore, 0.0f, 1.0f);
        }
    }
}
