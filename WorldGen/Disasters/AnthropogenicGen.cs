using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Disasters
{
    /// <summary>
    /// Anthropogenic disaster simulation with nuclear accidents,
    /// industrial failures, technological cascades, and human-caused catastrophes.
    /// </summary>
    [Serializable]
    public struct NuclearFacility : IComponentData
    {
        public int FacilityID;
        public string FacilityName;
        public FacilityType Type;
        public int RegionID;
        public float2 Location;
        
        // Reactor specifications
        public int ReactorCount;
        public ReactorType ReactorDesign;
        public float TotalCapacity_MW;
        public float CurrentOutput_MW;
        public int OperationalYears;
        
        // Safety systems
        public float SafetySystemRedundancy; // 0-1
        public float ContainmentIntegrity; // 0-1
        public float CoolingSystemStatus; // 0-1 operational
        public float BackupPowerStatus; // 0-1 available
        public float PassiveSafetyLevel; // 0-1
        
        // Risk metrics
        public float CoreDamageFrequency; // Per reactor-year
        public float LargeReleaseFrequency; // Per reactor-year
        public float SeismicMargin; // g acceleration tolerance
        public float FloodProtection; // Meters above design basis
        
        // Fuel and waste
        public float FuelBurnup; // GWd/ton
        public float SpentFuelInventory; // Tons
        public float WasteStorageCapacity; // % full
        public bool HasDryCaskStorage;
        
        // Emergency preparedness
        public float EPZ_Radius; // km (Emergency Planning Zone)
        public int EvacuationRoutes;
        public float EvacuationTime_Estimated; // Hours
        public bool HasDistributionPlan; // Potassium iodide
        
        // Current state
        public FacilityOperationalState State;
        public float AnomalyLevel; // 0-1
        public int INES_Level; // 0-7 if accident occurring
        public bool IsInAccident;
    }
    
    public enum FacilityType
    {
        PowerPlant,
        ResearchReactor,
        FuelFabrication,
        ReprocessingPlant,
        WasteStorage,
        EnrichmentFacility,
        MedicalIsotope
    }
    
    public enum ReactorType
    {
        PWR,  // Pressurized Water Reactor
        BWR,  // Boiling Water Reactor
        PHWR, // Pressurized Heavy Water
        GCR,  // Gas Cooled Reactor
        LMFBR,// Liquid Metal Fast Breeder
        SMR,  // Small Modular Reactor
        HTGR, // High Temperature Gas
        RBMK  // Graphite-moderated (Chernobyl-type)
    }
    
    public enum FacilityOperationalState
    {
        NormalOperation,
        ReducedPower,
        ShutdownPlanned,
        ShutdownScram,
        Incident,
        Accident,
        SevereAccident,
        Meltdown
    }
    
    [Serializable]
    public struct NuclearAccident : IComponentData
    {
        public int AccidentID;
        public int FacilityID;
        public int RegionID;
        public AccidentInitiatingEvent InitiatingEvent;
        
        // Progression
        public AccidentPhase Phase;
        public float TimeSinceInitiation; // Hours
        public float CoreDamageProgress; // 0-1
        public float ContainmentStatus; // 0-1 integrity
        public bool IsMeltdown;
        public bool IsContainmentBreach;
        
        // Releases
        public float RadioactiveRelease_Total; // PBq
        public float Cesium137_Release; // PBq
        public float Iodine131_Release; // PBq
        public float NobleGases_Release; // PBq
        public float ReleaseDuration; // Hours ongoing
        public float2 PlumeDirection; // Wind direction
        
        // INES classification
        public int INES_Level; // 1-7
        public string INES_Description;
        
        // Countermeasures
        public bool IsEvacuationOrdered;
        public float EvacuationRadius; // km
        public bool IsShelteringOrdered;
        public bool IsKIDistributed; // Potassium iodide
        public bool IsFoodRestricted;
        public float DecontaminationProgress; // 0-1
        
        // Impacts
        public int AcuteRadiationSyndrome_Cases;
        public int RadiationExposure_High; // >100 mSv
        public int RadiationExposure_Medium; // 10-100 mSv
        public int Evacuees;
        public float LongTermCancerRisk; // Additional lifetime risk %
        public float EconomicDamage;
        public float ExclusionZone_Area; // km²
        
        // Temporal
        public int InitiationTick;
        public int ContainmentTick; // When controlled
        public int RecoveryTick; // When area habitable
    }
    
    public enum AccidentInitiatingEvent
    {
        LOCA,           // Loss of Coolant Accident
        SBO,            // Station Blackout
        Earthquake,
        Tsunami,
        Flood,
        Fire,
        AircraftImpact,
        Sabotage,
        HumanError,
        EquipmentFailure,
        CyberAttack
    }
    
    public enum AccidentPhase
    {
        Initiation,
        Early,          // First hours
        Intermediate,   // Days to weeks
        Late,           // Weeks to years
        Recovery,
        Remediation
    }
    
    [Serializable]
    public struct IndustrialFacility : IComponentData
    {
        public int FacilityID;
        public string FacilityName;
        public IndustryType Type;
        public int RegionID;
        public float2 Location;
        
        // Hazardous materials
        public NativeArray<int> StoredChemicals; // Chemical IDs
        public float TotalHazardousMass; // Tons
        public float MostHazardousChemical_ID;
        public float ToxicCloudPotential; // 0-1
        
        // Process hazards
        public float Pressure_Level; // Operating pressure bar
        public float Temperature_Level; // Operating temperature °C
        public float FlammabilityRisk; // 0-1
        public float ReactivityRisk; // 0-1
        public float ExplosionPotential; // 0-1 TNT equivalent
        
        // Safety
        public float SafetyInstrumentedSystem_SIL; // 1-4
        public float ReliefSystemCapacity; // 0-1 adequate
        public float FireProtectionLevel; // 0-1
        public float LeakDetectionCoverage; // 0-1
        
        // Risk assessment
        public float IndividualRisk; // Fatalities per year (nearby resident)
        public float SocietalRisk_FN; // F-N curve value
        public float DominoEffectPotential; // 0-1, can trigger nearby facilities
        public float VulnerablePopulation; // Within impact zone
        
        // Compliance
        public float RegulatoryCompliance; // 0-1
        public float InspectionScore; // Last inspection
        public int ViolationsPending;
        public float MaintenanceBacklog; // 0-1
        
        // Emergency
        public bool HasEmergencyPlan;
        public float EmergencyResponseCapability; // 0-1
        public float MutualAidAgreements; // 0-1
    }
    
    public enum IndustryType
    {
        ChemicalPlant,
        Refinery,
        Petrochemical,
        FertilizerPlant,
        Pharmaceutical,
        PulpAndPaper,
        FoodProcessing,
        MetalSmelting,
        Warehouse_Hazmat,
        LNGTerminal,
        AmmoniaStorage,
        ChlorineStorage
    }
    
    [Serializable]
    public struct IndustrialAccident : IComponentData
    {
        public int AccidentID;
        public int FacilityID;
        public int RegionID;
        public AccidentType Type;
        
        // Event characteristics
        public float ExplosionYield_TNT; // Tons TNT equivalent (if explosion)
        public float FireTemperature; // °C (if fire)
        public float ToxicRelease_Mass; // kg
        public float ReleaseDuration; // Hours
        
        // Dispersion
        public float2 DispersionDirection; // Wind/plume direction
        public float DispersionDistance; // km
        public float AffectedArea; // km²
        public float Concentration_Max; // ppm or mg/m³
        public float IDLH_Zone; // Immediately Dangerous to Life/Health radius
        
        // Escalation
        public bool IsEscalating;
        public int SecondaryFacilities_AtRisk;
        public float DominoProbability; // 0-1
        public bool IsMultiSiteIncident;
        
        // Response
        public bool IsContainmentActive;
        public float ContainmentProgress; // 0-1
        public int RespondersDeployed;
        public bool IsEvacuationOrdered;
        public float EvacuationRadius; // km
        
        // Impacts
        public int Fatalities_Immediate;
        public int Injuries;
        public int Hospitalizations;
        public int Evacuees;
        public float LongTermHealthImpact; // 0-1
        public float EnvironmentalDamage; // 0-1
        public float EconomicDamage;
        
        // Temporal
        public int OccurredTick;
        public int ControlledTick;
        public bool IsActive;
    }
    
    public enum AccidentType
    {
        Explosion,
        Fire_Pool,
        Fire_Jet,
        Fire_Ball,
        FlashFire,
        ToxicRelease,
        VaporCloudExplosion,
        BLEVE, // Boiling Liquid Expanding Vapor Explosion
        RunawayReaction,
        StructuralCollapse,
        MultiHazard
    }
    
    [Serializable]
    public struct TechnologicalCascade : IComponentData
    {
        public int CascadeID;
        public int InitiatingEventID;
        public CascadeType Type;
        
        // Propagation
        public NativeArray<int> AffectedSystems; // System IDs in order
        public int AffectedSystemCount;
        public float PropagationSpeed; // Systems per hour
        public float AmplificationFactor; // Each failure makes next worse
        
        // Critical infrastructure
        public float PowerGridStatus; // 0-1 operational
        public float WaterSystemStatus; // 0-1
        public float CommunicationsStatus; // 0-1
        public float TransportationStatus; // 0-1
        public float HealthcareStatus; // 0-1 capacity
        public float FinancialSystemStatus; // 0-1
        
        // Interdependencies
        public float Power_WaterDependency; // Water needs power
        public float Power_CommsDependency; // Comms needs power
        public float Transport_FuelDependency; // Transport needs fuel
        public float Healthcare_PowerDependency; // Healthcare needs power
        
        // Cascading effects
        public float EconomicDisruption; // 0-1
        public float SocialDisruption; // 0-1
        public float GovernmentFunctionLoss; // 0-1
        public float SupplyChainBreak; // 0-1
        
        // Recovery
        public float RestorationPriority_NativeArray; // Ordered list
        public float RestorationRate; // Systems per day
        public float FullRecoveryTime; // Days projected
    }
    
    public enum CascadeType
    {
        Infrastructure,   // Physical infrastructure failure
        Cyber,            // Digital/cyber cascade
        Financial,        // Economic/financial cascade
        SupplyChain,      // Logistics cascade
        Epidemiological,  // Disease spread cascade
        Information,      // Misinformation cascade
        Combined          // Multiple cascades interacting
    }
    
    [Serializable]
    public struct SpaceWeatherEvent : IComponentData
    {
        public int EventID;
        public SpaceWeatherType Type;
        
        // Solar parameters
        public float SolarFlareClass; // X-class multiplier (X1, X2, etc.)
        public float CME_Speed; // km/s (Coronal Mass Ejection)
        public float CME_Mass; // kg
        public float ProtonFlux; // pfu (particle flux units)
        
        // Geomagnetic
        public float Kp_Index; // 0-9 geomagnetic activity
        public float Dst_Index; // nT (ring current strength)
        public float AE_Index; // Auroral electrojet
        
        // Impacts
        public float GIC_Risk; // Geomagnetically Induced Currents 0-1
        public float SatelliteDamageRisk; // 0-1
        public float AstronautRadiationRisk; // 0-1
        public float AviationRadiationRisk; // 0-1
        public float GPSDegradation; // 0-1 accuracy loss
        public float HF_Blackout; // 0-1 radio disruption
        public float AuroraVisibility; // Latitude where visible
        
        // Grid impacts
        public float TransformerSaturationRisk; // 0-1
        public float BlackoutProbability; // 0-1
        public float EstimatedOutage_MW;
        
        // Temporal
        public int OnsetTick;
        public int PeakTick;
        public int RecoveryTick;
        public bool IsActive;
    }
    
    public enum SpaceWeatherType
    {
        SolarFlare,
        CME,
        SolarProtonEvent,
        CoronalHole,
        GeomagneticStorm,
        IonosphericStorm,
        RadiationBeltEnhancement
    }
    
    public class AnthropogenicDisasterSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Monitor nuclear facility status
            // Simulate accident progression
            // Model radioactive dispersion
            // Track industrial hazards
            // Process technological cascades
            // Calculate space weather impacts
            // Generate emergency responses
            // Assess long-term consequences
        }
    }
}
