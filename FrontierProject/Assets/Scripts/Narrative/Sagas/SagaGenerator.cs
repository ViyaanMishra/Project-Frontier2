using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.Sagas
{
    /// <summary>
    /// Generates dynamic saga content based on player actions and world state.
    /// Creates personalized narrative arcs that respond to emergent gameplay.
    /// </summary>
    public class SagaGenerator : IService
    {
        private NativeHashMap<FixedString64Bytes, SagaTemplate> _templates;
        private NativeList<SagaFragment> _availableFragments;
        private Random _rng;
        
        public int Priority => 7;

        public void Initialize()
        {
            _templates = new NativeHashMap<FixedString64Bytes, SagaTemplate>(16, Allocator.Persistent);
            _availableFragments = new NativeList<SagaFragment>(Allocator.Persistent);
            _rng = new Random(Xorshift32.SeedFromTime());
        }

        public void Tick(double deltaTime)
        {
            // Periodically evaluate if new sagas should be generated
        }

        public void Shutdown()
        {
            if (_templates.IsCreated) _templates.Dispose();
            if (_availableFragments.IsCreated) _availableFragments.Dispose();
        }

        /// <summary>
        /// Registers a saga template for procedural generation.
        /// </summary>
        public void RegisterTemplate(SagaTemplate template)
        {
            _templates[template.Id] = template;
        }

        /// <summary>
        /// Adds a narrative fragment to the available pool.
        /// </summary>
        public void AddFragment(SagaFragment fragment)
        {
            _availableFragments.Add(fragment);
        }

        /// <summary>
        /// Generates a new saga based on current game state and player history.
        /// </summary>
        public SagaDefinition GenerateSaga(FixedString64Bytes templateId, GenerationContext context)
        {
            if (!_templates.TryGetValue(templateId, out var template))
            {
                UnityEngine.Debug.LogError($"Template {templateId} not found!");
                return default;
            }

            var saga = new SagaDefinition
            {
                Id = GenerateUniqueId(),
                Title = template.BaseTitle,
                Description = template.BaseDescription,
                Tier = template.Tier,
                IsMainStory = template.IsMainStory,
                Chapters = GenerateChapters(template, context)
            };

            return saga;
        }

        /// <summary>
        /// Generates chapters by selecting and customizing fragments.
        /// </summary>
        private NativeArray<SagaChapter> GenerateChapters(SagaTemplate template, GenerationContext context)
        {
            var chapters = new NativeArray<SagaChapter>(template.ChapterCount, Allocator.Persistent);
            
            for (int i = 0; i < template.ChapterCount; i++)
            {
                var fragment = SelectFragmentForSlot(template, i, context);
                chapters[i] = InstantiateChapter(fragment, context);
            }

            return chapters;
        }

        /// <summary>
        /// Selects an appropriate narrative fragment for a chapter slot.
        /// </summary>
        private SagaFragment SelectFragmentForSlot(SagaTemplate template, int slotIndex, GenerationContext context)
        {
            // Filter fragments by compatibility
            var compatibleFragments = new NativeList<SagaFragment>(Allocator.Temp);
            
            for (int i = 0; i < _availableFragments.Length; i++)
            {
                var fragment = _availableFragments[i];
                if (IsFragmentCompatible(fragment, template, slotIndex, context))
                {
                    compatibleFragments.Add(fragment);
                }
            }

            if (compatibleFragments.Length == 0)
            {
                UnityEngine.Debug.LogWarning($"No compatible fragments for slot {slotIndex}!");
                compatibleFragments.Dispose();
                return default;
            }

            // Weight selection based on context
            var selected = WeightedSelect(compatibleFragments, context);
            compatibleFragments.Dispose();
            return selected;
        }

        /// <summary>
        /// Checks if a fragment is compatible with the current generation context.
        /// </summary>
        private bool IsFragmentCompatible(SagaFragment fragment, SagaTemplate template, int slotIndex, GenerationContext context)
        {
            // Check tier compatibility
            if (fragment.MinTier > template.Tier || fragment.MaxTier < template.Tier)
                return false;

            // Check faction requirements
            if (!fragment.RequiredFactions.IsEmpty() && !context.HasFaction(fragment.RequiredFactions))
                return false;

            // Check location requirements
            if (!fragment.RequiredBiomes.IsEmpty() && !context.HasBiome(fragment.RequiredBiomes))
                return false;

            // Check prerequisite fragments
            if (slotIndex > 0)
            {
                // Would check previous slot's fragment for continuity
            }

            return true;
        }

        /// <summary>
        /// Performs weighted random selection based on context relevance.
        /// </summary>
        private SagaFragment WeightedSelect(NativeList<SagaFragment> fragments, GenerationContext context)
        {
            int totalWeight = 0;
            var weights = new NativeArray<int>(fragments.Length, Allocator.Temp);

            for (int i = 0; i < fragments.Length; i++)
            {
                weights[i] = CalculateFragmentWeight(fragments[i], context);
                totalWeight += weights[i];
            }

            var roll = _rng.NextInt(totalWeight);
            int cumulative = 0;

            for (int i = 0; i < fragments.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    weights.Dispose();
                    return fragments[i];
                }
            }

            weights.Dispose();
            return fragments[fragments.Length - 1];
        }

        /// <summary>
        /// Calculates how relevant a fragment is to the current context.
        /// </summary>
        private int CalculateFragmentWeight(SagaFragment fragment, GenerationContext context)
        {
            int weight = fragment.BaseWeight;

            // Bonus for faction alignment
            if (context.PlayerFaction == fragment.PreferredFaction)
                weight += 20;

            // Bonus for recent events
            if (fragment.RelatedEvents.Length > 0)
            {
                foreach (var evt in fragment.RelatedEvents)
                {
                    if (context.RecentEvents.Contains(evt))
                        weight += 15;
                }
            }

            // Penalty for overused fragments
            weight -= fragment.UsageCount * 5;

            return Math.Max(1, weight);
        }

        /// <summary>
        /// Instantiates a chapter from a fragment with context-specific customization.
        /// </summary>
        private SagaChapter InstantiateChapter(SagaFragment fragment, GenerationContext context)
        {
            var chapter = new SagaChapter
            {
                Id = GenerateUniqueId(),
                Title = CustomizeText(fragment.BaseTitle, context),
                Description = CustomizeText(fragment.BaseDescription, context),
                RequiredNodes = fragment.RequiredNodeTemplates.ToArray(Allocator.Persistent),
                OptionalNodes = fragment.OptionalNodeTemplates.ToArray(Allocator.Persistent)
            };

            return chapter;
        }

        /// <summary>
        /// Customizes text templates with context-specific values.
        /// </summary>
        private FixedString512Bytes CustomizeText(FixedString512Bytes template, GenerationContext context)
        {
            // Replace placeholders like {FACTION}, {LOCATION}, {CHARACTER}
            var result = template;
            // Implementation would use string replacement
            return result;
        }

        /// <summary>
        /// Generates a unique identifier for saga elements.
        /// </summary>
        private FixedString64Bytes GenerateUniqueId()
        {
            return new FixedString64Bytes($"gen_{_rng.NextInt():X8}");
        }
    }

    [Serializable]
    public struct SagaTemplate
    {
        public FixedString64Bytes Id;
        public FixedString128Bytes BaseTitle;
        public FixedString512Bytes BaseDescription;
        public int ChapterCount;
        public SagaTier Tier;
        public bool IsMainStory;
        public NativeArray<FixedString64Bytes> RequiredFactions;
        public NativeArray<FixedString64Bytes> ForbiddenFactions;
    }

    [Serializable]
    public struct SagaFragment
    {
        public FixedString64Bytes Id;
        public FixedString512Bytes BaseTitle;
        public FixedString512Bytes BaseDescription;
        public SagaTier MinTier;
        public SagaTier MaxTier;
        public int BaseWeight;
        public int UsageCount;
        public NativeArray<FixedString64Bytes> RequiredFactions;
        public NativeArray<FixedString64Bytes> RequiredBiomes;
        public FixedString64Bytes PreferredFaction;
        public NativeArray<FixedString64Bytes> RelatedEvents;
        public NativeArray<FixedString64Bytes> RequiredNodeTemplates;
        public NativeArray<FixedString64Bytes> OptionalNodeTemplates;
    }

    public struct GenerationContext
    {
        public FixedString64Bytes PlayerFaction;
        public NativeArray<FixedString64Bytes> RecentEvents;
        public NativeArray<FixedString64Bytes> VisitedBiomes;
        public float DifficultyModifier;
        public int PlayerLevel;

        public bool HasFaction(NativeArray<FixedString64Bytes> factions)
        {
            for (int i = 0; i < factions.Length; i++)
            {
                if (PlayerFaction == factions[i])
                    return true;
            }
            return false;
        }

        public bool HasBiome(NativeArray<FixedString64Bytes> biomes)
        {
            for (int i = 0; i < biomes.Length; i++)
            {
                if (VisitedBiomes.Contains(biomes[i]))
                    return true;
            }
            return false;
        }
    }
}
