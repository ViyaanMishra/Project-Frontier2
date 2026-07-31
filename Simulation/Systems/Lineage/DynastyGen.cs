using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Lineage
{
    /// <summary>
    /// Dynasty management system tracking bloodline prestige, 
    /// inheritance patterns, family rivalries, and legacy mechanics.
    /// </summary>
    [Serializable]
    public struct Dynasty : IComponentData
    {
        public int DynastyID;
        public string Name;
        public int FounderID;
        public int CurrentHeadID;
        public int GenerationCount;
        public int TotalMembers;
        public int LivingMembers;
        
        // Status metrics
        public float Prestige; // 0-1, historical reputation
        public float Honor; // 0-1, current standing
        public float Infamy; // 0-1, negative reputation
        public float PowerBase; // Combined political/military/economic power
        
        // Resources
        public float Treasury;
        public float LandHoldings;
        public float MilitaryRetainers;
        public float PoliticalAllies;
        
        // Bloodline quality
        public float GeneticQuality; // Average health/intelligence
        public float BloodlinePurity; // Inbreeding concerns
        public float VitalityIndex; // Fertility and longevity
        
        // Legacy
        public int YearsOfRule;
        public int TerritoriesControlled;
        public NativeArray<string> Achievements;
        public NativeArray<string> Scandals;
        public float HistoricalSignificance; // 0-1
        
        // Dynamics
        public float Cohesion; // Family unity 0-1
        public float InternalRivalry; // 0-1
        public float ExtinctionRisk; // 0-1
        public DynastyStatus Status;
    }
    
    public enum DynastyStatus
    {
        Rising,
        Established,
        Declining,
        Endangered,
        Extinct,
        Usurped,
        Exiled
    }
    
    [Serializable]
    public struct InheritanceEvent : IComponentData
    {
        public int EventID;
        public int DynastyID;
        public int DeceasedID;
        public int HeirID;
        public InheritanceType Type;
        
        // Assets transferred
        public float WealthTransferred;
        public int TitlesTransferred;
        public float LandTransferred;
        public NativeArray<int> ItemsTransferred;
        
        // Claims generated
        public NativeArray<SuccessionClaim> Claims;
        public bool IsContested;
        public float ContestationSeverity; // 0-1
        
        // Legitimacy
        public float HeirLegitimacy; // 0-1
        public float SuccessionStability; // 0-1
        public bool IsSmoothTransition;
        
        // Temporal
        public int OccurredTick;
        public int ResolutionTicks;
    }
    
    public enum InheritanceType
    {
        Primogeniture,      // Firstborn inherits all
        Partible,           // Divided among heirs
        Gavelkind,          // Equal division
        Ultimogeniture,     // Youngest inherits
        Tanistry,           // Chosen from extended family
        Elective,           // Elected by family council
        Meritocratic        // Most capable heir
    }
    
    [Serializable]
    public struct SuccessionClaim : IComponentData
    {
        public int ClaimID;
        public int ClaimantID;
        public int DynastyID;
        public ClaimType Type;
        public ClaimStrength Strength; // 0-1
        
        // Basis of claim
        public float GenealogicalProximity; // Closeness to last ruler
        public bool IsDirectLine; // Direct descendant
        public bool IsSeniorLine; // Senior branch of family
        public bool HasTestamentSupport; // Will mentions claimant
        public bool HasPopularSupport; // Public backs claim
        public bool HasMilitarySupport; // Army backs claim
        public bool HasReligiousSupport; // Church backs claim
        
        // Obstacles
        public bool IsIllegitimate;
        public bool IsAttainted; // Legally disqualified
        public bool IsForeign; // Non-native claimant
        public bool IsMinor; // Underage
        public bool IsIncapacitated;
        
        // Actions
        public bool IsActiveClaim; // Currently pressing claim
        public bool IsAtWar; // Fighting for claim
        public bool HasRenounced;
    }
    
    public enum ClaimType
    {
        Direct,         // Direct heir
        Collateral,     // Side branch
        Distant,        // Remote relation
        Marriage,       // Through marriage
        Conquest,       // By right of victory
        Election,       // Chosen by council
        Appointment,    // Named by predecessor
        Usurpation      // Seized power
    }
    
    public enum ClaimStrength
    {
        Unassailable,   // >0.9
        Strong,         // 0.7-0.9
        Moderate,       // 0.5-0.7
        Weak,           // 0.3-0.5
        Tenuous,        // 0.1-0.3
        Negligible      // <0.1
    }
    
    [Serializable]
    public struct FamilyRivalry : IComponentData
    {
        public int RivalryID;
        public int DynastyA_ID;
        public int DynastyB_ID;
        
        // Origins
        public RivalryOrigin Origin;
        public int OriginEventID;
        public string Grievance;
        
        // Intensity
        public float HatredLevel; // 0-1
        public float CompetitionLevel; // 0-1
        public float ViolenceLevel; // 0-1
        
        // Domains of conflict
        public bool CompetingForTerritory;
        public bool CompetingForPower;
        public bool CompetingForPrestige;
        public bool CompetingForResources;
        public bool IdeologicalConflict;
        public bool ReligiousConflict;
        public bool PersonalFeud;
        
        // History
        public int ConflictsCount;
        public int WarsFought;
        public int AssassinationAttempts;
        public float BetrayalCount;
        
        // Current state
        public bool IsAtWar;
        public bool HasTruce;
        public float TruceRemaining;
        public bool SeekingReconciliation;
    }
    
    public enum RivalryOrigin
    {
        TerritorialDispute,
        SuccessionConflict,
        Betrayal,
        Insult,
        Murder,
        ReligiousDifference,
        IdeologicalClash,
        ResourceCompetition,
        RomanceRivalry,
        AncientGrudge
    }
    
    [Serializable]
    public struct BloodlineTrait : IComponentData
    {
        public int TraitID;
        public string TraitName;
        public TraitType Type;
        public TraitInheritance InheritancePattern;
        
        // Effects
        public float HealthModifier;
        public float IntelligenceModifier;
        public float CharismaModifier;
        public float FertilityModifier;
        public float LongevityModifier;
        public float StressResistance;
        
        // Expression
        public float Penetrance; // % of carriers who express
        public float Expressivity; // Severity when expressed
        public int AgeOfOnset; // When trait manifests
        
        // Prevalence
        public float FrequencyInBloodline; // 0-1
        public int CarrierCount;
        public int AffectedCount;
        
        // Social perception
        public bool IsDesirable;
        public bool IsStigmatized;
        public float PrestigeBonus; // If positive trait
    }
    
    public enum TraitType
    {
        Physical,       // Appearance/abilities
        Cognitive,      // Mental faculties
        Personality,    // Behavioral tendencies
        Medical,        // Health conditions
        Talent,         // Special abilities
        Curse,          // Negative supernatural
        Blessing        // Positive supernatural
    }
    
    public enum TraitInheritance
    {
        AutosomalDominant,
        AutosomalRecessive,
        XLinkedDominant,
        XLinkedRecessive,
        YLinked,
        Mitochondrial,
        Polygenic,
        Epigenetic
    }
    
    [Serializable]
    public struct LegacyScore : IComponentData
    {
        public int DynastyID;
        
        // Component scores (0-1 each)
        public float PoliticalLegacy; // Governance impact
        public float MilitaryLegacy; // Conquests/defenses
        public float CulturalLegacy; // Arts/patronage
        public float EconomicLegacy; // Wealth creation
        public float ScientificLegacy; // Knowledge advancement
        public float ReligiousLegacy; // Faith impact
        public float ArchitecturalLegacy; // Buildings/monuments
        
        // Derived metrics
        public float OverallLegacy; // Weighted average
        public float LegacyDurability; // How long it will last
        public float HistoricalMemory; // How well remembered
        
        // Tangible remains
        public int MonumentsBuilt;
        public int InstitutionsFounded;
        public float WealthEndowment;
        public NativeArray<string> FamousWorks;
        
        // Reputation over time
        public float ContemporaryRating; // Rated by contemporaries
        public float HistoricalRating; // Rated by historians
        public float ModernRating; // Current perception
        public float RatingTrend; // Improving or declining
    }
    
    public class DynastySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Track dynasty metrics over time
            // Process inheritance events
            // Manage succession claims
            // Update family rivalries
            // Calculate bloodline trait propagation
            // Assess extinction risks
            // Generate legacy scores
        }
    }
}
