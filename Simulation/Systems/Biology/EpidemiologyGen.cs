using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Biology
{
    /// <summary>
    /// Epidemiology system with SIR/SEIR disease modeling,
    /// transmission dynamics, intervention effects, and mutation tracking.
    /// </summary>
    [Serializable]
    public struct Disease : IComponentData
    {
        public int DiseaseID;
        public string Name;
        public DiseaseType Type;
        public PathogenClass PathogenClass;
        
        // Transmission
        public TransmissionMode TransmissionMode;
        public float BaseReproductionNumber; // R0
        public float EffectiveReproductionNumber; // Rt (current)
        public float TransmissionRate; // Beta
        public float ContactRate; // Contacts per time
        public float InfectiousDose; // Pathogens needed for infection
        
        // Progression
        public float IncubationPeriod; // Days from infection to symptoms
        public float LatentPeriod; // Days from infection to infectiousness
        public float InfectiousPeriod; // Duration of infectiousness
        public float RecoveryTime; // Average time to recover
        public float FatalityRate; // Case fatality ratio 0-1
        public float SeverityIndex; // 0-1 composite
        
        // Immunity
        public bool ConfersImmunity;
        public float ImmunityDuration; // Days immunity lasts
        public float ImmunityStrength; // 0-1
        public bool HasVaccine;
        public float VaccineEfficacy; // 0-1
        
        // Variants
        public int VariantCount;
        public NativeArray<int> VariantIDs;
        public float MutationRate;
        
        // Interventions
        public float TreatmentEfficacy; // 0-1
        public bool HasSpecificTreatment;
        public float QuarantineEffectiveness; // 0-1
    }
    
    public enum DiseaseType
    {
        Respiratory,
        Gastrointestinal,
        VectorBorne,
        Bloodborne,
        SexuallyTransmitted,
        Zoonotic,
        Waterborne,
        Airborne,
        Contact,
        Environmental
    }
    
    public enum PathogenClass
    {
        Virus,
        Bacteria,
        Fungus,
        Parasite,
        Prion,
        Protozoa,
        Helminth
    }
    
    public enum TransmissionMode
    {
        DirectContact,
        IndirectContact,
        Droplet,
        Airborne,
        FecalOral,
        VectorBorne,
        Vertical, // Parent to offspring
        Bloodborne,
        Sexual
    }
    
    [Serializable]
    public struct PopulationHealth : IComponentData
    {
        public int PopulationID;
        public int RegionID;
        public int TotalPopulation;
        
        // Compartment counts (SEIR model)
        public int Susceptible;
        public int Exposed; // Infected but not yet infectious
        public int Infectious;
        public int InfectiousSymptomatic;
        public int InfectiousAsymptomatic;
        public int Recovered;
        public int Deceased;
        public int Vaccinated;
        public int Immune; // From prior infection
        
        // Hospital capacity
        public int Hospitalized;
        public int InICU;
        public int OnVentilator;
        public int HospitalCapacity;
        public int ICUCapacity;
        public int VentilatorCapacity;
        public float HospitalOccupancy; // 0-1
        public float ICUOccupancy; // 0-1
        
        // Health metrics
        public float OverallHealthIndex; // 0-1
        public float MalnutritionRate; // 0-1
        public float ComorbidityPrevalence; // 0-1
        public float HealthcareAccess; // 0-1
        
        // Demographics affecting disease
        public float MedianAge;
        public float ElderlyProportion; // 65+
        public float ImmunocompromisedProportion; // 0-1
        
        // Temporal
        public int OutbreakStartTick;
        public int PeakInfectionTick;
        public int PeakInfectionCount;
    }
    
    [Serializable]
    public struct EpidemicState : IComponentData
    {
        public int EpidemicID;
        public int DiseaseID;
        public int RegionID;
        public EpidemicPhase Phase;
        
        // Current status
        public int TotalCases;
        public int ActiveCases;
        public int NewCasesToday;
        public int TotalDeaths;
        public int TotalRecovered;
        public float CaseFatalityRate; // Current CFR
        
        // Dynamics
        public float ReproductionNumber; // Rt
        public float DoublingTime; // Days
        public float GrowthRate; // % per day
        public float PositivityRate; // % tests positive
        
        // Trajectory
        public bool IsGrowing;
        public bool IsPeaked;
        public bool IsDeclining;
        public float PeakProjection;
        public int ProjectedPeakDay;
        public float TotalProjectedCases;
        public float TotalProjectedDeaths;
        
        // Response
        public float InterventionLevel; // 0-1, current measures
        public float ComplianceRate; // Population compliance 0-1
        public float TestingRate; // Tests per capita
        public float ContactTracingEffectiveness; // 0-1
        
        // Risk assessment
        public float OverwhelmRisk; // Healthcare overwhelm 0-1
        public float SpreadRisk; // 0-1
        public float MortalityRisk; // 0-1
        public RiskLevel RiskLevel;
    }
    
    public enum EpidemicPhase
    {
        PreEmergence,
        Sporadic,
        Clustered,
        CommunityTransmission,
        Epidemic,
        Pandemic,
        Declining,
        Controlled,
        Endemic,
        Eliminated
    }
    
    public enum RiskLevel
    {
        Minimal,
        Low,
        Moderate,
        High,
        Severe,
        Critical
    }
    
    [Serializable]
    public struct Intervention : IComponentData
    {
        public int InterventionID;
        public InterventionType Type;
        public int TargetRegionID;
        public int TargetDiseaseID;
        
        // Implementation
        public float Coverage; // % population affected 0-1
        public float Effectiveness; // 0-1
        public float Compliance; // 0-1
        public int StartTick;
        public int EndTick;
        public bool IsActive;
        
        // Costs
        public float EconomicCost;
        public float SocialCost; // 0-1
        public float PoliticalCost; // 0-1
        
        // Effects
        public float TransmissionReduction; // % reduction
        public float MortalityReduction; // % reduction
        public float R0Reduction; // Absolute reduction
        
        // Side effects
        public float MentalHealthImpact; // 0-1 negative
        public float EducationDisruption; // 0-1
        public float EconomicDisruption; // 0-1
    }
    
    public enum InterventionType
    {
        // Non-pharmaceutical
        SocialDistancing,
        Lockdown,
        SchoolClosure,
        WorkplaceClosure,
        MassGatheringBan,
        TravelRestriction,
        Quarantine,
        Isolation,
        MaskMandate,
        HandHygiene,
        VentilationImprovement,
        
        // Pharmaceutical
        Vaccination,
        AntiviralTreatment,
        AntibioticTreatment,
        Prophylaxis,
        
        // Surveillance
        Testing,
        ContactTracing,
        SymptomScreening,
        WastewaterSurveillance,
        
        // Communication
        PublicAwareness,
        RiskCommunication,
        MisinformationCounter
    }
    
    [Serializable]
    public struct DiseaseVariant : IComponentData
    {
        public int VariantID;
        public int ParentDiseaseID;
        public string VariantName;
        public int FirstDetectionTick;
        
        // Genetic changes
        public NativeArray<string> Mutations;
        public float GeneticDistance; // From original strain
        
        // Phenotypic changes
        public float TransmissibilityChange; // % change vs parent
        public float VirulenceChange; // % change
        public float ImmuneEvasion; // 0-1, escapes prior immunity
        public float VaccineEvasion; // 0-1
        public float DiagnosticEvasion; // 0-1, evades tests
        
        // Epidemiological impact
        public float RelativeFitness; // Competitive advantage
        public float GrowthAdvantage; // % faster spread
        public bool IsVariantOfConcern;
        public bool IsVariantOfInterest;
        
        // Spread
        public int DetectedCases;
        public int RegionsPresent;
        public float Frequency; // % of sequenced cases
    }
    
    [Serializable]
    public struct ContactNetwork : IComponentData
    {
        public int NetworkID;
        public int PopulationID;
        
        // Structure
        public int NodeCount; // Individuals
        public int EdgeCount; // Contacts
        public float AverageDegree; // Average contacts per person
        public float DegreeDistribution; // Variance in contacts
        
        // Network properties
        public float ClusteringCoefficient; // Friend-of-friend connections
        public float AveragePathLength; // Degrees of separation
        public float Modularity; // Community structure
        public float Assortativity; // Like-connects-to-like tendency
        
        // Heterogeneity
        public float SuperspreaderPotential; // 0-1
        public int SuperspreaderCount; // High-contact individuals
        public float IsolatedIndividuals; // Low-contact %
        
        // Dynamics
        public float ContactReduction; // % reduced from baseline
        public float NetworkAdaptation; // How network changes during epidemic
    }
    
    public class EpidemiologySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Run SEIR compartment model
            // Calculate transmission dynamics
            // Track disease progression
            // Model healthcare demand
            // Apply intervention effects
            // Simulate variant emergence
            // Update contact networks
            // Generate epidemic curves
        }
    }
    
    /// <summary>
    /// Helper methods for epidemiological calculations.
    /// </summary>
    public static class EpidemiologyCalculator
    {
        public static float CalculateR0(float transmissionRate, float infectiousPeriod, float susceptibleFraction)
        {
            return transmissionRate * infectiousPeriod * susceptibleFraction;
        }
        
        public static float CalculateHerdImmunityThreshold(float R0)
        {
            return 1f - (1f / R0);
        }
        
        public static float CalculateAttackRate(float R0, float initialSusceptible)
        {
            // Final size equation approximation
            if (R0 <= 1f) return 0f;
            
            float herdThreshold = CalculateHerdImmunityThreshold(R0);
            return math.min(1f, initialSusceptible * (1f - math.exp(-R0 * herdThreshold)));
        }
        
        public static int CalculatePeakInfections(int population, float R0, float initialInfected)
        {
            // Simplified peak estimation
            float herdThreshold = CalculateHerdImmunityThreshold(R0);
            float peakFraction = herdThreshold * math.log(R0);
            return (int)(population * math.min(1f, peakFraction));
        }
    }
}
