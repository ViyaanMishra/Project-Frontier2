using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace FrontierProject.Narrative.Themes
{
    /// <summary>
    /// Advanced narrative theme system that defines overarching tonal, 
    /// stylistic, and philosophical frameworks for story content.
    /// Supports dynamic theme blending, contextual weighting, and player-aligned adaptation.
    /// </summary>
    
    [Serializable]
    public struct NarrativeTheme
    {
        public FixedString64Bytes ID;
        public FixedString128Bytes DisplayName;
        public FixedString512Bytes Description;
        
        // Core thematic axes (0-1 scale)
        public float HopeVsDespair;      // 0 = Despair, 1 = Hope
        public float OrderVsChaos;       // 0 = Chaos, 1 = Order
        public float IndividualVsCollective; // 0 = Collective, 1 = Individual
        public float TraditionVsProgress;    // 0 = Progress, 1 = Tradition
        public float NatureVsTechnology;     // 0 = Technology, 1 = Nature
        public float FreedomVsSecurity;      // 0 = Security, 1 = Freedom
        
        // Emotional palette weights
        public float WeightJoy;
        public float WeightSorrow;
        public float WeightAnger;
        public float WeightFear;
        public float WeightSurprise;
        public float WeightDisgust;
        public float WeightTrust;
        public float WeightAnticipation;
        
        // Philosophical tags
        public FixedString32Bytes PhilosophyTag1;
        public FixedString32Bytes PhilosophyTag2;
        public FixedString32Bytes PhilosophyTag3;
        
        // Visual/audio mood indicators
        public float ColorSaturationModifier;
        public float ColorTemperatureShift; // -1 cool, 1 warm
        public float MusicIntensityBaseline;
        public float AmbientDensity;
        
        // Narrative pacing preferences
        public float PacingSlowMomentWeight;
        public float PacingTensionBuildWeight;
        public float PacingClimaxWeight;
        public float PacingResolutionWeight;
        
        // Character archetype affinities
        public float AffinityHero;
        public float AffinityMentor;
        public float AffinityShadow;
        public float AffinityTrickster;
        public float AffinityGuardian;
        public float AffinityRebel;
        public float AffinityLover;
        public float AffinityCreator;
        
        // Dynamic flags
        public bool IsDominantTheme;
        public bool CanBlendWithOthers;
        public float BlendPriority;
        
        // Unlock conditions
        public FixedString64Bytes UnlockConditionID;
        public int MinimumStoryProgress;
    }

    [Serializable]
    public struct ThemeBlendState
    {
        public FixedString64Bytes PrimaryThemeID;
        public FixedString64Bytes SecondaryThemeID;
        public FixedString64Bytes TertiaryThemeID;
        
        public float PrimaryWeight;
        public float SecondaryWeight;
        public float TertiaryWeight;
        
        public float TransitionProgress; // 0-1 during theme shifts
        public float StabilityFactor;    // How resistant to change
        
        public double LastShiftTime;
        public int ShiftCount;
    }

    public struct ThemeComponent : IComponentData
    {
        public Entity OwnerEntity;
        public NarrativeTheme ActiveTheme;
        public ThemeBlendState BlendState;
        public DynamicBuffer<ThemeHistoryEntry> History;
        public DynamicBuffer<ActiveThemeModifier> Modifiers;
    }

    [Serializable]
    public struct ThemeHistoryEntry
    {
        public double Timestamp;
        public FixedString64Bytes ThemeID;
        public FixedString64Bytes TriggerSource;
        public float Intensity;
        public FixedString128Bytes ContextDescription;
    }

    [Serializable]
    public struct ActiveThemeModifier
    {
        public FixedString64Bytes ModifierID;
        public float DurationRemaining;
        public float Strength;
        public ThemeModifierType Type;
        public bool IsPermanent;
    }

    public enum ThemeModifierType
    {
        Amplify,
        Dampen,
        Invert,
        LockAxis,
        ForceBlend,
        TemporalShift,
        ContextualOverride
    }

    public class ThemeManagerSystem : SystemBase
    {
        private EntityQuery _themeQuery;
        private NativeHashMap<FixedString64Bytes, NarrativeTheme> _themeRegistry;
        private NativeList<FixedString64Bytes> _activeThemeIDs;
        
        protected override void OnCreate()
        {
            _themeQuery = GetEntityQuery(typeof(ThemeComponent));
            _themeRegistry = new NativeHashMap<FixedString64Bytes, NarrativeTheme>(100, Allocator.Persistent);
            _activeThemeIDs = new NativeList<FixedString64Bytes>(Allocator.Persistent);
            
            InitializeDefaultThemes();
        }
        
        protected override void OnDestroy()
        {
            _themeRegistry.Dispose();
            _activeThemeIDs.Dispose();
        }
        
        private void InitializeDefaultThemes()
        {
            // Register core archetypal themes
            RegisterTheme(CreateHopefulTheme());
            RegisterTheme(CreateDespairTheme());
            RegisterTheme(CreateOrderTheme());
            RegisterTheme(CreateChaosTheme());
            RegisterTheme(CreateRebellionTheme());
            RegisterTheme(CreateTraditionTheme());
            RegisterTheme(CreateNatureTheme());
            RegisterTheme(CreateTechnologicalTheme());
            RegisterTheme(CreateTragicTheme());
            RegisterTheme(CreateTriumphantTheme());
            RegisterTheme(CreateMysteryTheme());
            RegisterTheme(CreateRomanceTheme());
            RegisterTheme(CreateHorrorTheme());
            RegisterTheme(CreateComedyTheme());
        }
        
        private void RegisterTheme(NarrativeTheme theme)
        {
            if (!_themeRegistry.ContainsKey(theme.ID))
            {
                _themeRegistry.Add(theme.ID, theme);
                _activeThemeIDs.Add(theme.ID);
            }
        }
        
        private NarrativeTheme CreateHopefulTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_HOPEFUL",
                DisplayName = "Hopeful Dawn",
                Description = "A theme of optimism, renewal, and the triumph of the human spirit against adversity.",
                HopeVsDespair = 0.9f,
                OrderVsChaos = 0.6f,
                IndividualVsCollective = 0.5f,
                TraditionVsProgress = 0.4f,
                NatureVsTechnology = 0.5f,
                FreedomVsSecurity = 0.6f,
                WeightJoy = 0.7f,
                WeightSorrow = 0.2f,
                WeightAnger = 0.1f,
                WeightFear = 0.15f,
                WeightSurprise = 0.4f,
                WeightDisgust = 0.05f,
                WeightTrust = 0.8f,
                WeightAnticipation = 0.7f,
                PhilosophyTag1 = "Optimism",
                PhilosophyTag2 = "Resilience",
                PhilosophyTag3 = "Growth",
                ColorSaturationModifier = 0.2f,
                ColorTemperatureShift = 0.3f,
                MusicIntensityBaseline = 0.5f,
                AmbientDensity = 0.6f,
                PacingSlowMomentWeight = 0.3f,
                PacingTensionBuildWeight = 0.4f,
                PacingClimaxWeight = 0.5f,
                PacingResolutionWeight = 0.8f,
                AffinityHero = 0.9f,
                AffinityMentor = 0.7f,
                AffinityShadow = 0.2f,
                AffinityTrickster = 0.3f,
                AffinityGuardian = 0.6f,
                AffinityRebel = 0.5f,
                AffinityLover = 0.4f,
                AffinityCreator = 0.6f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 1.0f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        private NarrativeTheme CreateDespairTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_DESPAIR",
                DisplayName = "Shadow's Embrace",
                Description = "A dark exploration of loss, futility, and the crushing weight of existence.",
                HopeVsDespair = 0.1f,
                OrderVsChaos = 0.3f,
                IndividualVsCollective = 0.7f,
                TraditionVsProgress = 0.8f,
                NatureVsTechnology = 0.4f,
                FreedomVsSecurity = 0.2f,
                WeightJoy = 0.05f,
                WeightSorrow = 0.9f,
                WeightAnger = 0.4f,
                WeightFear = 0.7f,
                WeightSurprise = 0.2f,
                WeightDisgust = 0.5f,
                WeightTrust = 0.1f,
                WeightAnticipation = 0.3f,
                PhilosophyTag1 = "Nihilism",
                PhilosophyTag2 = "Existentialism",
                PhilosophyTag3 = "Tragedy",
                ColorSaturationModifier = -0.4f,
                ColorTemperatureShift = -0.5f,
                MusicIntensityBaseline = 0.3f,
                AmbientDensity = 0.8f,
                PacingSlowMomentWeight = 0.7f,
                PacingTensionBuildWeight = 0.6f,
                PacingClimaxWeight = 0.4f,
                PacingResolutionWeight = 0.2f,
                AffinityHero = 0.3f,
                AffinityMentor = 0.2f,
                AffinityShadow = 0.9f,
                AffinityTrickster = 0.4f,
                AffinityGuardian = 0.3f,
                AffinityRebel = 0.6f,
                AffinityLover = 0.5f,
                AffinityCreator = 0.2f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.8f,
                UnlockConditionID = "STORY_DARK_TURN",
                MinimumStoryProgress = 30
            };
        }
        
        private NarrativeTheme CreateOrderTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_ORDER",
                DisplayName = "Iron Protocol",
                Description = "Structure, hierarchy, and the belief that civilization requires absolute control.",
                HopeVsDespair = 0.5f,
                OrderVsChaos = 0.95f,
                IndividualVsCollective = 0.1f,
                TraditionVsProgress = 0.7f,
                NatureVsTechnology = 0.3f,
                FreedomVsSecurity = 0.15f,
                WeightJoy = 0.2f,
                WeightSorrow = 0.3f,
                WeightAnger = 0.3f,
                WeightFear = 0.4f,
                WeightSurprise = 0.1f,
                WeightDisgust = 0.4f,
                WeightTrust = 0.5f,
                WeightAnticipation = 0.4f,
                PhilosophyTag1 = "Authoritarianism",
                PhilosophyTag2 = "Structuralism",
                PhilosophyTag3 = "Duty",
                ColorSaturationModifier = -0.2f,
                ColorTemperatureShift = -0.3f,
                MusicIntensityBaseline = 0.4f,
                AmbientDensity = 0.5f,
                PacingSlowMomentWeight = 0.4f,
                PacingTensionBuildWeight = 0.5f,
                PacingClimaxWeight = 0.6f,
                PacingResolutionWeight = 0.5f,
                AffinityHero = 0.5f,
                AffinityMentor = 0.4f,
                AffinityShadow = 0.5f,
                AffinityTrickster = 0.1f,
                AffinityGuardian = 0.9f,
                AffinityRebel = 0.1f,
                AffinityLover = 0.2f,
                AffinityCreator = 0.4f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.7f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        private NarrativeTheme CreateChaosTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_CHAOS",
                DisplayName = "Anarchic Surge",
                Description = "The beautiful destruction of old systems and the raw energy of unbridled freedom.",
                HopeVsDespair = 0.4f,
                OrderVsChaos = 0.05f,
                IndividualVsCollective = 0.8f,
                TraditionVsProgress = 0.2f,
                NatureVsTechnology = 0.6f,
                FreedomVsSecurity = 0.95f,
                WeightJoy = 0.5f,
                WeightSorrow = 0.2f,
                WeightAnger = 0.7f,
                WeightFear = 0.3f,
                WeightSurprise = 0.8f,
                WeightDisgust = 0.3f,
                WeightTrust = 0.2f,
                WeightAnticipation = 0.6f,
                PhilosophyTag1 = "Anarchism",
                PhilosophyTag2 = "Absurdism",
                PhilosophyTag3 = "Revolution",
                ColorSaturationModifier = 0.4f,
                ColorTemperatureShift = 0.2f,
                MusicIntensityBaseline = 0.8f,
                AmbientDensity = 0.3f,
                PacingSlowMomentWeight = 0.1f,
                PacingTensionBuildWeight = 0.7f,
                PacingClimaxWeight = 0.9f,
                PacingResolutionWeight = 0.2f,
                AffinityHero = 0.4f,
                AffinityMentor = 0.1f,
                AffinityShadow = 0.6f,
                AffinityTrickster = 0.9f,
                AffinityGuardian = 0.1f,
                AffinityRebel = 0.95f,
                AffinityLover = 0.4f,
                AffinityCreator = 0.5f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.9f,
                UnlockConditionID = "FACTION_ANARCHIST_JOIN",
                MinimumStoryProgress = 20
            };
        }
        
        private NarrativeTheme CreateRebellionTheme() => CreateChaosTheme(); // Alias
        
        private NarrativeTheme CreateTraditionTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_TRADITION",
                DisplayName = "Ancestral Echoes",
                Description = "Wisdom passed through generations, honoring the old ways in a changing world.",
                HopeVsDespair = 0.5f,
                OrderVsChaos = 0.8f,
                IndividualVsCollective = 0.3f,
                TraditionVsProgress = 0.95f,
                NatureVsTechnology = 0.7f,
                FreedomVsSecurity = 0.4f,
                WeightJoy = 0.4f,
                WeightSorrow = 0.4f,
                WeightAnger = 0.2f,
                WeightFear = 0.3f,
                WeightSurprise = 0.2f,
                WeightDisgust = 0.2f,
                WeightTrust = 0.7f,
                WeightAnticipation = 0.4f,
                PhilosophyTag1 = "Conservatism",
                PhilosophyTag2 = "Heritage",
                PhilosophyTag3 = "Wisdom",
                ColorSaturationModifier = 0.1f,
                ColorTemperatureShift = 0.4f,
                MusicIntensityBaseline = 0.4f,
                AmbientDensity = 0.6f,
                PacingSlowMomentWeight = 0.6f,
                PacingTensionBuildWeight = 0.3f,
                PacingClimaxWeight = 0.4f,
                PacingResolutionWeight = 0.6f,
                AffinityHero = 0.5f,
                AffinityMentor = 0.9f,
                AffinityShadow = 0.3f,
                AffinityTrickster = 0.2f,
                AffinityGuardian = 0.8f,
                AffinityRebel = 0.2f,
                AffinityLover = 0.4f,
                AffinityCreator = 0.3f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.6f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        private NarrativeTheme CreateNatureTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_NATURE",
                DisplayName = "Wild Resurgence",
                Description = "The primal force of the natural world reclaiming its dominion.",
                HopeVsDespair = 0.6f,
                OrderVsChaos = 0.4f,
                IndividualVsCollective = 0.4f,
                TraditionVsProgress = 0.6f,
                NatureVsTechnology = 0.95f,
                FreedomVsSecurity = 0.7f,
                WeightJoy = 0.5f,
                WeightSorrow = 0.3f,
                WeightAnger = 0.4f,
                WeightFear = 0.4f,
                WeightSurprise = 0.5f,
                WeightDisgust = 0.2f,
                WeightTrust = 0.6f,
                WeightAnticipation = 0.5f,
                PhilosophyTag1 = "Primitivism",
                PhilosophyTag2 = "Ecology",
                PhilosophyTag3 = "Balance",
                ColorSaturationModifier = 0.3f,
                ColorTemperatureShift = 0.1f,
                MusicIntensityBaseline = 0.5f,
                AmbientDensity = 0.9f,
                PacingSlowMomentWeight = 0.5f,
                PacingTensionBuildWeight = 0.4f,
                PacingClimaxWeight = 0.5f,
                PacingResolutionWeight = 0.5f,
                AffinityHero = 0.5f,
                AffinityMentor = 0.6f,
                AffinityShadow = 0.4f,
                AffinityTrickster = 0.5f,
                AffinityGuardian = 0.7f,
                AffinityRebel = 0.6f,
                AffinityLover = 0.5f,
                AffinityCreator = 0.4f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.7f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        private NarrativeTheme CreateTechnologicalTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_TECHNOLOGY",
                DisplayName = "Synthetic Ascension",
                Description = "Humanity transcended through machinery, AI, and the promise of digital immortality.",
                HopeVsDespair = 0.5f,
                OrderVsChaos = 0.7f,
                IndividualVsCollective = 0.6f,
                TraditionVsProgress = 0.1f,
                NatureVsTechnology = 0.05f,
                FreedomVsSecurity = 0.5f,
                WeightJoy = 0.4f,
                WeightSorrow = 0.3f,
                WeightAnger = 0.3f,
                WeightFear = 0.5f,
                WeightSurprise = 0.6f,
                WeightDisgust = 0.3f,
                WeightTrust = 0.4f,
                WeightAnticipation = 0.7f,
                PhilosophyTag1 = "Transhumanism",
                PhilosophyTag2 = "Rationalism",
                PhilosophyTag3 = "Progress",
                ColorSaturationModifier = -0.1f,
                ColorTemperatureShift = -0.6f,
                MusicIntensityBaseline = 0.6f,
                AmbientDensity = 0.4f,
                PacingSlowMomentWeight = 0.3f,
                PacingTensionBuildWeight = 0.5f,
                PacingClimaxWeight = 0.7f,
                PacingResolutionWeight = 0.4f,
                AffinityHero = 0.5f,
                AffinityMentor = 0.5f,
                AffinityShadow = 0.5f,
                AffinityTrickster = 0.4f,
                AffinityGuardian = 0.5f,
                AffinityRebel = 0.4f,
                AffinityLover = 0.3f,
                AffinityCreator = 0.9f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.8f,
                UnlockConditionID = "TECH_TIER_3_UNLOCKED",
                MinimumStoryProgress = 40
            };
        }
        
        private NarrativeTheme CreateTragicTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_TRAGIC",
                DisplayName = "Fallen Grace",
                Description = "The inevitable downfall of noble souls, crushed by fate or fatal flaws.",
                HopeVsDespair = 0.2f,
                OrderVsChaos = 0.5f,
                IndividualVsCollective = 0.6f,
                TraditionVsProgress = 0.5f,
                NatureVsTechnology = 0.5f,
                FreedomVsSecurity = 0.4f,
                WeightJoy = 0.1f,
                WeightSorrow = 0.85f,
                WeightAnger = 0.3f,
                WeightFear = 0.5f,
                WeightSurprise = 0.3f,
                WeightDisgust = 0.2f,
                WeightTrust = 0.3f,
                WeightAnticipation = 0.4f,
                PhilosophyTag1 = "Fatalism",
                PhilosophyTag2 = "Hubris",
                PhilosophyTag3 = "Catharsis",
                ColorSaturationModifier = -0.3f,
                ColorTemperatureShift = -0.2f,
                MusicIntensityBaseline = 0.5f,
                AmbientDensity = 0.7f,
                PacingSlowMomentWeight = 0.5f,
                PacingTensionBuildWeight = 0.6f,
                PacingClimaxWeight = 0.8f,
                PacingResolutionWeight = 0.3f,
                AffinityHero = 0.7f,
                AffinityMentor = 0.5f,
                AffinityShadow = 0.7f,
                AffinityTrickster = 0.3f,
                AffinityGuardian = 0.5f,
                AffinityRebel = 0.4f,
                AffinityLover = 0.6f,
                AffinityCreator = 0.4f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.9f,
                UnlockConditionID = "CHARACTER_DEATH_MAJOR",
                MinimumStoryProgress = 50
            };
        }
        
        private NarrativeTheme CreateTriumphantTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_TRIUMPHANT",
                DisplayName = "Glory Eternal",
                Description = "The soaring victory of righteousness, the culmination of struggle into glory.",
                HopeVsDespair = 0.95f,
                OrderVsChaos = 0.7f,
                IndividualVsCollective = 0.5f,
                TraditionVsProgress = 0.5f,
                NatureVsTechnology = 0.5f,
                FreedomVsSecurity = 0.6f,
                WeightJoy = 0.9f,
                WeightSorrow = 0.1f,
                WeightAnger = 0.2f,
                WeightFear = 0.1f,
                WeightSurprise = 0.5f,
                WeightDisgust = 0.05f,
                WeightTrust = 0.85f,
                WeightAnticipation = 0.8f,
                PhilosophyTag1 = "Heroism",
                PhilosophyTag2 = "Victory",
                PhilosophyTag3 = "Legacy",
                ColorSaturationModifier = 0.4f,
                ColorTemperatureShift = 0.5f,
                MusicIntensityBaseline = 0.9f,
                AmbientDensity = 0.5f,
                PacingSlowMomentWeight = 0.2f,
                PacingTensionBuildWeight = 0.5f,
                PacingClimaxWeight = 0.95f,
                PacingResolutionWeight = 0.9f,
                AffinityHero = 0.95f,
                AffinityMentor = 0.6f,
                AffinityShadow = 0.1f,
                AffinityTrickster = 0.2f,
                AffinityGuardian = 0.7f,
                AffinityRebel = 0.5f,
                AffinityLover = 0.5f,
                AffinityCreator = 0.6f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 1.0f,
                UnlockConditionID = "FINAL_VICTORY",
                MinimumStoryProgress = 90
            };
        }
        
        private NarrativeTheme CreateMysteryTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_MYSTERY",
                DisplayName = "Veiled Truths",
                Description = "Hidden knowledge, unsolved puzzles, and the thrill of discovery.",
                HopeVsDespair = 0.5f,
                OrderVsChaos = 0.5f,
                IndividualVsCollective = 0.7f,
                TraditionVsProgress = 0.4f,
                NatureVsTechnology = 0.4f,
                FreedomVsSecurity = 0.5f,
                WeightJoy = 0.3f,
                WeightSorrow = 0.2f,
                WeightAnger = 0.2f,
                WeightFear = 0.5f,
                WeightSurprise = 0.9f,
                WeightDisgust = 0.2f,
                WeightTrust = 0.3f,
                WeightAnticipation = 0.85f,
                PhilosophyTag1 = "Epistemology",
                PhilosophyTag2 = "Curiosity",
                PhilosophyTag3 = "Revelation",
                ColorSaturationModifier = -0.2f,
                ColorTemperatureShift = -0.4f,
                MusicIntensityBaseline = 0.4f,
                AmbientDensity = 0.7f,
                PacingSlowMomentWeight = 0.4f,
                PacingTensionBuildWeight = 0.8f,
                PacingClimaxWeight = 0.7f,
                PacingResolutionWeight = 0.5f,
                AffinityHero = 0.6f,
                AffinityMentor = 0.5f,
                AffinityShadow = 0.6f,
                AffinityTrickster = 0.7f,
                AffinityGuardian = 0.4f,
                AffinityRebel = 0.5f,
                AffinityLover = 0.3f,
                AffinityCreator = 0.5f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.7f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        private NarrativeTheme CreateRomanceTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_ROMANCE",
                DisplayName = "Hearts Entwined",
                Description = "Love in all its forms: passionate, tragic, comedic, and transformative.",
                HopeVsDespair = 0.7f,
                OrderVsChaos = 0.4f,
                IndividualVsCollective = 0.6f,
                TraditionVsProgress = 0.5f,
                NatureVsTechnology = 0.5f,
                FreedomVsSecurity = 0.6f,
                WeightJoy = 0.7f,
                WeightSorrow = 0.4f,
                WeightAnger = 0.2f,
                WeightFear = 0.3f,
                WeightSurprise = 0.5f,
                WeightDisgust = 0.1f,
                WeightTrust = 0.7f,
                WeightAnticipation = 0.7f,
                PhilosophyTag1 = "Love",
                PhilosophyTag2 = "Connection",
                PhilosophyTag3 = "Sacrifice",
                ColorSaturationModifier = 0.3f,
                ColorTemperatureShift = 0.4f,
                MusicIntensityBaseline = 0.5f,
                AmbientDensity = 0.5f,
                PacingSlowMomentWeight = 0.6f,
                PacingTensionBuildWeight = 0.5f,
                PacingClimaxWeight = 0.6f,
                PacingResolutionWeight = 0.7f,
                AffinityHero = 0.6f,
                AffinityMentor = 0.4f,
                AffinityShadow = 0.4f,
                AffinityTrickster = 0.5f,
                AffinityGuardian = 0.5f,
                AffinityRebel = 0.5f,
                AffinityLover = 0.95f,
                AffinityCreator = 0.5f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.8f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        private NarrativeTheme CreateHorrorTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_HORROR",
                DisplayName = "Abyssal Gaze",
                Description = "Terror from the unknown, the monstrous, and the violation of natural law.",
                HopeVsDespair = 0.15f,
                OrderVsChaos = 0.2f,
                IndividualVsCollective = 0.8f,
                TraditionVsProgress = 0.6f,
                NatureVsTechnology = 0.5f,
                FreedomVsSecurity = 0.3f,
                WeightJoy = 0.02f,
                WeightSorrow = 0.4f,
                WeightAnger = 0.3f,
                WeightFear = 0.95f,
                WeightSurprise = 0.7f,
                WeightDisgust = 0.8f,
                WeightTrust = 0.1f,
                WeightAnticipation = 0.6f,
                PhilosophyTag1 = "Cosmic Horror",
                PhilosophyTag2 = "Body Horror",
                PhilosophyTag3 = "Madness",
                ColorSaturationModifier = -0.5f,
                ColorTemperatureShift = -0.7f,
                MusicIntensityBaseline = 0.3f,
                AmbientDensity = 0.9f,
                PacingSlowMomentWeight = 0.6f,
                PacingTensionBuildWeight = 0.8f,
                PacingClimaxWeight = 0.7f,
                PacingResolutionWeight = 0.1f,
                AffinityHero = 0.3f,
                AffinityMentor = 0.2f,
                AffinityShadow = 0.9f,
                AffinityTrickster = 0.5f,
                AffinityGuardian = 0.3f,
                AffinityRebel = 0.4f,
                AffinityLover = 0.3f,
                AffinityCreator = 0.3f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.9f,
                UnlockConditionID = "HORROR_ELEMENT_INTRODUCED",
                MinimumStoryProgress = 25
            };
        }
        
        private NarrativeTheme CreateComedyTheme()
        {
            return new NarrativeTheme
            {
                ID = "THEME_COMEDY",
                DisplayName = "Fool's Fortune",
                Description = "Laughter as liberation, the absurdity of existence embraced with joy.",
                HopeVsDespair = 0.8f,
                OrderVsChaos = 0.4f,
                IndividualVsCollective = 0.5f,
                TraditionVsProgress = 0.4f,
                NatureVsTechnology = 0.5f,
                FreedomVsSecurity = 0.7f,
                WeightJoy = 0.9f,
                WeightSorrow = 0.1f,
                WeightAnger = 0.1f,
                WeightFear = 0.1f,
                WeightSurprise = 0.7f,
                WeightDisgust = 0.1f,
                WeightTrust = 0.6f,
                WeightAnticipation = 0.6f,
                PhilosophyTag1 = "Absurdism",
                PhilosophyTag2 = "Satire",
                PhilosophyTag3 = "Liberation",
                ColorSaturationModifier = 0.5f,
                ColorTemperatureShift = 0.3f,
                MusicIntensityBaseline = 0.6f,
                AmbientDensity = 0.4f,
                PacingSlowMomentWeight = 0.3f,
                PacingTensionBuildWeight = 0.4f,
                PacingClimaxWeight = 0.6f,
                PacingResolutionWeight = 0.8f,
                AffinityHero = 0.5f,
                AffinityMentor = 0.4f,
                AffinityShadow = 0.2f,
                AffinityTrickster = 0.9f,
                AffinityGuardian = 0.4f,
                AffinityRebel = 0.6f,
                AffinityLover = 0.6f,
                AffinityCreator = 0.5f,
                IsDominantTheme = false,
                CanBlendWithOthers = true,
                BlendPriority = 0.6f,
                UnlockConditionID = "",
                MinimumStoryProgress = 0
            };
        }
        
        protected override void OnUpdate()
        {
            var themeRegistry = _themeRegistry;
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            Entities
                .WithAll<ThemeComponent>()
                .ForEach((ref ThemeComponent themeComp) =>
                {
                    // Update blend transitions
                    if (themeComp.BlendState.TransitionProgress < 1.0f && 
                        themeComp.BlendState.SecondaryWeight > 0)
                    {
                        themeComp.BlendState.TransitionProgress += (float)deltaTime * 0.5f;
                        if (themeComp.BlendState.TransitionProgress > 1.0f)
                            themeComp.BlendState.TransitionProgress = 1.0f;
                        
                        // Lerp weights during transition
                        float t = themeComp.BlendState.TransitionProgress;
                        themeComp.BlendState.PrimaryWeight = math.lerp(1.0f, 0.0f, t);
                        themeComp.BlendState.SecondaryWeight = math.lerp(0.0f, 1.0f, t);
                    }
                    
                    // Decay temporary modifiers
                    for (int i = themeComp.Modifiers.Length - 1; i >= 0; i--)
                    {
                        var modifier = themeComp.Modifiers[i];
                        if (!modifier.IsPermanent)
                        {
                            modifier.DurationRemaining -= (float)deltaTime;
                            if (modifier.DurationRemaining <= 0)
                            {
                                themeComp.Modifiers.RemoveAt(i);
                            }
                            else
                            {
                                themeComp.Modifiers[i] = modifier;
                            }
                        }
                    }
                    
                    // Calculate stability factor based on history
                    if (themeComp.History.Length > 10)
                    {
                        int recentShifts = 0;
                        double currentTime = SystemAPI.Time.ElapsedTime;
                        for (int i = themeComp.History.Length - 1; i >= 0; i--)
                        {
                            if (currentTime - themeComp.History[i].Timestamp < 60.0) // Last minute
                                recentShifts++;
                            else
                                break;
                        }
                        themeComp.BlendState.StabilityFactor = math.clamp(1.0f - (recentShifts * 0.1f), 0.2f, 1.0f);
                    }
                }).WithoutBurst().Run();
        }
        
        public void SetPrimaryTheme(Entity entity, FixedString64Bytes themeID)
        {
            if (!EntityManager.Exists(entity)) return;
            if (!_themeRegistry.ContainsKey(themeID)) return;
            
            var themeComp = EntityManager.GetComponentData<ThemeComponent>(entity);
            var newTheme = _themeRegistry[themeID];
            
            // Record history
            var entry = new ThemeHistoryEntry
            {
                Timestamp = SystemAPI.Time.ElapsedTime,
                ThemeID = themeID,
                TriggerSource = "MANUAL_SET",
                Intensity = 1.0f,
                ContextDescription = "Direct theme assignment"
            };
            themeComp.History.Add(entry);
            
            themeComp.ActiveTheme = newTheme;
            themeComp.BlendState.PrimaryThemeID = themeID;
            themeComp.BlendState.PrimaryWeight = 1.0f;
            themeComp.BlendState.SecondaryWeight = 0.0f;
            themeComp.BlendState.TertiaryWeight = 0.0f;
            themeComp.BlendState.TransitionProgress = 0.0f;
            themeComp.BlendState.LastShiftTime = SystemAPI.Time.ElapsedTime;
            themeComp.BlendState.ShiftCount++;
            
            EntityManager.SetComponentData(entity, themeComp);
        }
        
        public void BlendToTheme(Entity entity, FixedString64Bytes newThemeID, float blendSpeed = 0.5f)
        {
            if (!EntityManager.Exists(entity)) return;
            if (!_themeRegistry.ContainsKey(newThemeID)) return;
            
            var themeComp = EntityManager.GetComponentData<ThemeComponent>(entity);
            
            // Shift current primary to secondary, new theme becomes primary
            themeComp.BlendState.TertiaryThemeID = themeComp.BlendState.SecondaryThemeID;
            themeComp.BlendState.TertiaryWeight = themeComp.BlendState.SecondaryWeight;
            
            themeComp.BlendState.SecondaryThemeID = themeComp.BlendState.PrimaryThemeID;
            themeComp.BlendState.SecondaryWeight = themeComp.BlendState.PrimaryWeight;
            
            themeComp.BlendState.PrimaryThemeID = newThemeID;
            themeComp.BlendState.PrimaryWeight = 0.0f;
            themeComp.BlendState.TransitionProgress = 0.0f;
            themeComp.BlendState.LastShiftTime = SystemAPI.Time.ElapsedTime;
            themeComp.BlendState.ShiftCount++;
            
            // Record history
            var entry = new ThemeHistoryEntry
            {
                Timestamp = SystemAPI.Time.ElapsedTime,
                ThemeID = newThemeID,
                TriggerSource = "BLEND_TRANSITION",
                Intensity = blendSpeed,
                ContextDescription = $"Blending from {themeComp.BlendState.SecondaryThemeID}"
            };
            themeComp.History.Add(entry);
            
            EntityManager.SetComponentData(entity, themeComp);
        }
        
        public void ApplyThemeModifier(Entity entity, ActiveThemeModifier modifier)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var themeComp = EntityManager.GetComponentData<ThemeComponent>(entity);
            themeComp.Modifiers.Add(modifier);
            EntityManager.SetComponentData(entity, themeComp);
        }
        
        public NarrativeTheme GetBlendedTheme(Entity entity)
        {
            if (!EntityManager.Exists(entity))
                return new NarrativeTheme { ID = "DEFAULT" };
            
            var themeComp = EntityManager.GetComponentData<ThemeComponent>(entity);
            
            if (!_themeRegistry.ContainsKey(themeComp.BlendState.PrimaryThemeID))
                return themeComp.ActiveTheme;
            
            var primary = _themeRegistry[themeComp.BlendState.PrimaryThemeID];
            
            if (string.IsNullOrEmpty(themeComp.BlendState.SecondaryThemeID.ToString()) ||
                !_themeRegistry.ContainsKey(themeComp.BlendState.SecondaryThemeID))
                return primary;
            
            var secondary = _themeRegistry[themeComp.BlendState.SecondaryThemeID];
            
            // Blend primary and secondary
            float p = themeComp.BlendState.PrimaryWeight;
            float s = themeComp.BlendState.SecondaryWeight;
            float total = p + s;
            if (total > 0)
            {
                p /= total;
                s /= total;
            }
            
            return BlendThemes(primary, secondary, p, s);
        }
        
        private NarrativeTheme BlendThemes(NarrativeTheme a, NarrativeTheme b, float weightA, float weightB)
        {
            return new NarrativeTheme
            {
                ID = a.ID,
                DisplayName = a.DisplayName,
                Description = a.Description,
                HopeVsDespair = math.lerp(a.HopeVsDespair, b.HopeVsDespair, weightB),
                OrderVsChaos = math.lerp(a.OrderVsChaos, b.OrderVsChaos, weightB),
                IndividualVsCollective = math.lerp(a.IndividualVsCollective, b.IndividualVsCollective, weightB),
                TraditionVsProgress = math.lerp(a.TraditionVsProgress, b.TraditionVsProgress, weightB),
                NatureVsTechnology = math.lerp(a.NatureVsTechnology, b.NatureVsTechnology, weightB),
                FreedomVsSecurity = math.lerp(a.FreedomVsSecurity, b.FreedomVsSecurity, weightB),
                WeightJoy = math.lerp(a.WeightJoy, b.WeightJoy, weightB),
                WeightSorrow = math.lerp(a.WeightSorrow, b.WeightSorrow, weightB),
                WeightAnger = math.lerp(a.WeightAnger, b.WeightAnger, weightB),
                WeightFear = math.lerp(a.WeightFear, b.WeightFear, weightB),
                WeightSurprise = math.lerp(a.WeightSurprise, b.WeightSurprise, weightB),
                WeightDisgust = math.lerp(a.WeightDisgust, b.WeightDisgust, weightB),
                WeightTrust = math.lerp(a.WeightTrust, b.WeightTrust, weightB),
                WeightAnticipation = math.lerp(a.WeightAnticipation, b.WeightAnticipation, weightB),
                PhilosophyTag1 = a.PhilosophyTag1,
                PhilosophyTag2 = a.PhilosophyTag2,
                PhilosophyTag3 = a.PhilosophyTag3,
                ColorSaturationModifier = math.lerp(a.ColorSaturationModifier, b.ColorSaturationModifier, weightB),
                ColorTemperatureShift = math.lerp(a.ColorTemperatureShift, b.ColorTemperatureShift, weightB),
                MusicIntensityBaseline = math.lerp(a.MusicIntensityBaseline, b.MusicIntensityBaseline, weightB),
                AmbientDensity = math.lerp(a.AmbientDensity, b.AmbientDensity, weightB),
                PacingSlowMomentWeight = math.lerp(a.PacingSlowMomentWeight, b.PacingSlowMomentWeight, weightB),
                PacingTensionBuildWeight = math.lerp(a.PacingTensionBuildWeight, b.PacingTensionBuildWeight, weightB),
                PacingClimaxWeight = math.lerp(a.PacingClimaxWeight, b.PacingClimaxWeight, weightB),
                PacingResolutionWeight = math.lerp(a.PacingResolutionWeight, b.PacingResolutionWeight, weightB),
                AffinityHero = math.lerp(a.AffinityHero, b.AffinityHero, weightB),
                AffinityMentor = math.lerp(a.AffinityMentor, b.AffinityMentor, weightB),
                AffinityShadow = math.lerp(a.AffinityShadow, b.AffinityShadow, weightB),
                AffinityTrickster = math.lerp(a.AffinityTrickster, b.AffinityTrickster, weightB),
                AffinityGuardian = math.lerp(a.AffinityGuardian, b.AffinityGuardian, weightB),
                AffinityRebel = math.lerp(a.AffinityRebel, b.AffinityRebel, weightB),
                AffinityLover = math.lerp(a.AffinityLover, b.AffinityLover, weightB),
                AffinityCreator = math.lerp(a.AffinityCreator, b.AffinityCreator, weightB),
                IsDominantTheme = a.IsDominantTheme,
                CanBlendWithOthers = true,
                BlendPriority = math.lerp(a.BlendPriority, b.BlendPriority, weightB),
                UnlockConditionID = a.UnlockConditionID,
                MinimumStoryProgress = math.min(a.MinimumStoryProgress, b.MinimumStoryProgress)
            };
        }
        
        public bool IsThemeUnlocked(FixedString64Bytes themeID, int storyProgress)
        {
            if (!_themeRegistry.ContainsKey(themeID)) return false;
            
            var theme = _themeRegistry[themeID];
            
            // Check progress requirement
            if (storyProgress < theme.MinimumStoryProgress)
                return false;
            
            // Check unlock condition (would integrate with StoryConditions system)
            if (!string.IsNullOrEmpty(theme.UnlockConditionID.ToString()))
            {
                // Placeholder: actual check would query StoryVariableStore
                return true;
            }
            
            return true;
        }
        
        public NativeArray<FixedString64Bytes> GetAllThemeIDs(Allocator allocator)
        {
            var result = new NativeArray<FixedString64Bytes>(_activeThemeIDs.Length, allocator);
            for (int i = 0; i < _activeThemeIDs.Length; i++)
            {
                result[i] = _activeThemeIDs[i];
            }
            return result;
        }
    }
}
