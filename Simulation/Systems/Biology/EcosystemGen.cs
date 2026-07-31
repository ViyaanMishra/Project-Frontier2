using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Biology
{
    /// <summary>
    /// Ecosystem simulation with trophic cascades, food web dynamics,
    /// predator-prey relationships, and ecological balance modeling.
    /// </summary>
    [Serializable]
    public struct Ecosystem : IComponentData
    {
        public int EcosystemID;
        public int RegionID;
        public EcosystemType Type;
        public float Area;
        
        // Energy flow
        public float SolarInput; // Energy entering system
        public float PrimaryProductivity; // Plant biomass production
        public float SecondaryProductivity; // Consumer biomass production
        public float DecompositionRate;
        public float NutrientCyclingRate;
        
        // Trophic structure
        public float BiomassProducers; // Plants/algae
        public float BiomassPrimaryConsumers; // Herbivores
        public float BiomassSecondaryConsumers; // Carnivores
        public float BiomassTertiaryConsumers; // Apex predators
        public float BiomassDecomposers;
        
        // Stability metrics
        public float BiodiversityIndex; // Shannon index
        public float Resilience; // Ability to recover from disturbance
        public float Stability; // Resistance to change
        public float CarryingCapacityUtilization; // 0-1
        
        // Health indicators
        public float EcosystemHealth; // 0-1 composite
        public float PollutionLevel; // 0-1
        public float HabitatFragmentation; // 0-1
        public float InvasiveSpeciesPressure; // 0-1
        
        // Temporal
        public int SuccessionStage; // Ecological succession
        public float SuccessionProgress; // 0-1
    }
    
    public enum EcosystemType
    {
        Forest,
        Grassland,
        Desert,
        Tundra,
        Freshwater,
        Marine,
        CoralReef,
        Wetland,
        Urban,
        Agricultural
    }
    
    [Serializable]
    public struct Species : IComponentData
    {
        public int SpeciesID;
        public string ScientificName;
        public string CommonName;
        public SpeciesType Type;
        public TrophicLevel TrophicLevel;
        
        // Population
        public int PopulationSize;
        public float PopulationDensity;
        public float BirthRate;
        public float DeathRate;
        public float GrowthRate;
        public float CarryingCapacity;
        
        // Life history
        public float Lifespan;
        public float AgeAtMaturity;
        public float ReproductiveRate;
        public int OffspringPerBirth;
        public float ParentalInvestment; // 0-1
        
        // Ecology
        public NativeArray<int> PreySpeciesIDs;
        public NativeArray<int> PredatorSpeciesIDs;
        public NativeArray<int> CompetitorSpeciesIDs;
        public NativeArray<int> SymbiontSpeciesIDs;
        
        // Traits
        public float BodySize; // kg
        public float MetabolicRate;
        public float Mobility; // 0-1
        public float Specialization; // 0-1, generalist to specialist
        public float Aggression; // 0-1
        
        // Status
        public ConservationStatus ConservationStatus;
        public bool IsInvasive;
        public bool IsEndemic;
        public bool IsKeystone; // Disproportionate ecosystem impact
        public float ExtinctionRisk; // 0-1
    }
    
    public enum SpeciesType
    {
        Plant,
        Fungus,
        Bacteria,
        Invertebrate,
        Fish,
        Amphibian,
        Reptile,
        Bird,
        Mammal
    }
    
    public enum TrophicLevel
    {
        Producer,         // Autotrophs
        PrimaryConsumer,  // Herbivores
        SecondaryConsumer,// Small carnivores
        TertiaryConsumer, // Large carnivores
        ApexPredator,     // Top of food chain
        Decomposer,       // Detritivores
        Omnivore          // Mixed diet
    }
    
    public enum ConservationStatus
    {
        LeastConcern,
        NearThreatened,
        Vulnerable,
        Endangered,
        CriticallyEndangered,
        ExtinctInWild,
        Extinct,
        DataDeficient
    }
    
    [Serializable]
    public struct FoodWeb : IComponentData
    {
        public int FoodWebID;
        public int EcosystemID;
        
        // Structure
        public int SpeciesCount;
        public int LinkCount; // Trophic links
        public float Connectance; // Links / possible links
        public float Complexity; // Weighted complexity
        
        // Energy flow
        public NativeArray<EnergyFlow> EnergyFlows;
        public float TotalEnergyThroughput;
        public float EnergyTransferEfficiency; // Typically ~10%
        
        // Network properties
        public float Robustness; // Resistance to species loss
        public float Modularity; // Compartmentalization
        public float Nestedness; // Hierarchical structure
        public int TrophicLevels; // Number of levels
        
        // Dynamics
        public float StabilityIndex; // 0-1
        public float CascadeRisk; // Risk of trophic cascade
        public NativeArray<int> KeystoneSpeciesIDs;
        
        // Perturbations
        public bool IsUnderStress;
        public float StressLevel; // 0-1
        public int RecentExtinctions;
    }
    
    [Serializable]
    public struct EnergyFlow
    {
        public int FromSpeciesID;
        public int ToSpeciesID;
        public float FlowRate; // Energy per time
        public float Efficiency; // Transfer efficiency
        public float ConsumptionRate; // How much is consumed
        public float Preference; // Predator preference 0-1
    }
    
    [Serializable]
    public struct PopulationDynamics : IComponentData
    {
        public int SpeciesID;
        public int RegionID;
        
        // Current state
        public int CurrentPopulation;
        public int PreviousPopulation;
        public float PopulationGrowthRate;
        
        // Demographics
        public int Juveniles;
        public int Adults;
        public int Seniors;
        public float SexRatio; // Male/Female
        
        // Vital rates
        public float Natality; // Birth rate
        public float Mortality; // Death rate
        public float Immigration;
        public float Emigration;
        
        // Density dependence
        public float DensityEffect; // Impact of crowding
        public float ResourceAvailability; // 0-1
        public float CompetitionIntensity; // 0-1
        
        // External factors
        public float PredationPressure; // 0-1
        public float DiseasePrevalence; // 0-1
        public float EnvironmentalStress; // 0-1
        public float HumanImpact; // 0-1
        
        // Projections
        public float ProjectedGrowthRate;
        public float TimeToCarryingCapacity;
        public float ExtinctionProbability; // 0-1
    }
    
    [Serializable]
    public struct PredatorPreyInteraction : IComponentData
    {
        public int InteractionID;
        public int PredatorSpeciesID;
        public int PreySpeciesID;
        
        // Interaction strength
        public float AttackRate; // Encounter success
        public float HandlingTime; // Time to consume
        public float ConversionEfficiency; // Prey to predator biomass
        public float FunctionalResponse; // Type I, II, or III
        
        // Dynamics
        public float PredationRate; // Current predation
        public float PreyAvailability; // 0-1
        public float PredatorSatiation; // 0-1, how full predators are
        public float PreyDefenseEffectiveness; // 0-1
        
        // Behavioral
        public float PredatorSearchEfficiency; // 0-1
        public float PreyVigilance; // 0-1, anti-predator behavior
        public float HabitatOverlap; // 0-1, spatial overlap
        
        // Population effects
        public float PreyMortalityFromPredation; // % of prey deaths
        public float PredatorDependenceOnPrey; // % of predator diet
        public bool IsRegulating; // Does this interaction regulate populations?
    }
    
    [Serializable]
    public struct HabitatPatch : IComponentData
    {
        public int PatchID;
        public int EcosystemID;
        public HabitatType Type;
        public float Area;
        public float2 Centroid;
        
        // Quality
        public float HabitatQuality; // 0-1
        public float ResourceAbundance; // 0-1
        public float ShelterAvailability; // 0-1
        public float DisturbanceLevel; // 0-1
        
        // Connectivity
        public float ConnectivityIndex; // 0-1, connection to other patches
        public NativeArray<int> AdjacentPatchIDs;
        public float EdgeEffect; // Impact of patch edges
        public float CoreArea; // Interior habitat
        
        // Occupancy
        public NativeArray<int> ResidentSpeciesIDs;
        public float SpeciesRichness;
        public float OccupancyRate; // 0-1
        
        // Dynamics
        public float SuccessionStage;
        public float DegradationRate;
        public float RestorationPotential; // 0-1
    }
    
    public enum HabitatType
    {
        Canopy,
        Understory,
        ForestFloor,
        OpenGrass,
        DenseVegetation,
        RockyOutcrop,
        WaterBody,
        Riparian,
        Cave,
        Burrow
    }
    
    public class EcosystemSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Calculate energy flow through food web
            // Update population dynamics
            // Process predator-prey interactions
            // Track species extinctions/colonizations
            // Calculate biodiversity indices
            // Model habitat changes
            // Detect trophic cascades
        }
    }
}
