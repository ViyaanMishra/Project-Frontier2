using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Disasters
{
    /// <summary>
    /// Hydrological disaster simulation with flood modeling,
    /// tsunami propagation, dam failure analysis, and water dynamics.
    /// </summary>
    [Serializable]
    public struct FloodEvent : IComponentData
    {
        public int EventID;
        public int RegionID;
        public FloodType Type;
        public float2 AffectedArea_Centroid;
        public float AffectedArea_km2;
        
        // Water characteristics
        public float PeakDischarge; // m³/s
        public float PeakWaterLevel; // Meters above normal
        public float FlowVelocity_Max; // m/s
        public float InundationDepth_Avg; // Meters
        public float InundationDepth_Max; // Meters
        public float Duration_Hours;
        
        // Hydrology
        public float ReturnPeriod; // Years (100-year flood, etc.)
        public float Precipitation_Total; // mm triggering event
        public float Precipitation_Duration; // Hours
        public float SoilSaturation; // 0-1 before event
        public float SnowmeltContribution; // % of flow
        
        // Infrastructure
        public int LeveesBreached;
        public int DamsOvertopped;
        public float ChannelCapacity_Exceeded; // % over capacity
        public float DrainageSystemFailure; // 0-1
        
        // Impacts
        public int PopulationAffected;
        public int Evacuations;
        public int Fatalities;
        public int StructuresInundated;
        public int StructuresDestroyed;
        public float EconomicDamage;
        public float AgriculturalDamage;
        public float ContaminationLevel; // Water quality 0-1
        
        // Response
        public bool IsWarningIssued;
        public float WarningLeadTime; // Hours
        public int SheltersOpened;
        public float EmergencyResponseLevel; // 0-1
        
        // Temporal
        public int StartTick;
        public int PeakTick;
        public int RecessionTick;
        public bool IsActive;
    }
    
    public enum FloodType
    {
        FlashFlood,           // Rapid onset, short duration
        Riverine,             // River overflow
        Coastal,              // Storm surge, high tide
        Urban,                // Drainage overwhelm
        Pluvial,              // Rainfall flooding
        Fluvial,              // River channel overflow
        Lacustrine,           // Lake overflow
        DamBreak,             // Infrastructure failure
        IceJam,               // Ice blockage
        GlacialOutburst       // GLOF
    }
    
    [Serializable]
    public struct TsunamiEvent : IComponentData
    {
        public int EventID;
        public int SourceRegionID;
        public TsunamiSourceType SourceType;
        public int SourceEarthquakeID; // If earthquake-triggered
        
        // Source parameters
        public float2 SourceLocation;
        public float SourceDepth; // km (for earthquakes)
        public float Magnitude; // Source event magnitude
        public float SeafloorDisplacement; // Meters vertical
        public float RuptureArea; // km²
        
        // Wave characteristics
        public NativeArray<float> WaveHeights; // At different distances
        public float MaxWaveHeight_Source; // Meters at source
        public float MaxWaveHeight_Coast; // Meters at coast
        public float Wavelength_Avg; // km
        public float WavePeriod; // Minutes between waves
        public float PropagationSpeed; // km/h (deep water)
        
        // Propagation
        public float2 PropagationDirection;
        public float ArrivalTime_Minutes; // To nearest coast
        public NativeArray<float> ArrivalTimes; // At multiple locations
        public float EnergyDissipation; // 0-1, energy lost
        
        // Coastal impacts
        public float Runup_Max; // Meters above sea level
        public float InundationDistance; // Meters inland
        public float InundationArea; // km²
        public float CurrentVelocity_Max; // m/s
        
        // Damage
        public int Fatalities;
        public int Missing;
        public int Displaced;
        public int StructuresDestroyed;
        public float EconomicDamage;
        public float PortDamage; // Critical infrastructure
        public float NuclearPlantRisk; // 0-1
        
        // Warning
        public bool IsWarningIssued;
        public float WarningTime; // Minutes before arrival
        public int EvacuationCompliance; // % evacuated
        public bool IsFalseAlarm;
        
        // Temporal
        public int GenerationTick;
        public int LandfallTick;
        public int AllClearTick;
        public bool IsActive;
    }
    
    public enum TsunamiSourceType
    {
        Earthquake,
        Landslide_Submarine,
        Landslide_Coastal,
        VolcanicEruption,
        VolcanicCollapse,
        MeteoriteImpact,
        GlacierCalving,
        Experimental // Man-made
    }
    
    [Serializable]
    public struct DamStructure : IComponentData
    {
        public int DamID;
        public string DamName;
        public DamType Type;
        public int RegionID;
        public float2 Location;
        
        // Physical characteristics
        public float Height; // Meters
        public float Length; // Meters (crest)
        public float ReservoirCapacity; // Million cubic meters
        public float CurrentStorage; // % capacity
        public float CatchmentArea; // km²
        
        // Purpose
        public bool GeneratesHydroelectric;
        public float PowerCapacity_MW;
        public bool ProvidesIrrigation;
        public bool ProvidesFloodControl;
        public bool ProvidesWaterSupply;
        public int PopulationServed;
        
        // Safety metrics
        public float StructuralIntegrity; // 0-1
        public float Age_Years;
        public float DesignStandard; // 0-1, modern standards
        public float MaintenanceLevel; // 0-1
        public float RiskCategory; // 0-1, high to low hazard
        
        // Monitoring
        public float SeepageRate; // Liters/second
        public float UpliftPressure; // kPa
        public float DeformationRate; // mm/year
        public float CrackSeverity; // 0-1
        public float SpillwayCapacity; // m³/s
        
        // Failure risk
        public float FailureProbability; // Annual probability
        public float FailureMode_MostLikely;
        public float ConsequenceSeverity; // 0-1 if fails
        public float DownstreamPopulation; // At risk
        public float EconomicValueAtRisk;
        
        // Emergency
        public bool HasEmergencyPlan;
        public bool HasEarlyWarning;
        public float EvacuationTime_Required; // Hours
    }
    
    public enum DamType
    {
        Concrete_Gravity,
        Concrete_Arch,
        Concrete_Buttress,
        Embankment_Earth,
        Embankment_Rockfill,
        RollerCompactedConcrete,
        Composite
    }
    
    [Serializable]
    public struct WatershedState : IComponentData
    {
        public int WatershedID;
        public int RegionID;
        public float Area_km2;
        
        // Hydrology
        public float Streamflow_Current; // m³/s
        public float Streamflow_Normal; // m³/s baseline
        public float Baseflow; // Groundwater contribution
        public float SurfaceRunoff; // Direct runoff
        
        // Storage
        public float SoilMoisture; // 0-1 saturation
        public float GroundwaterLevel; // Meters depth
        public float Snowpack_SWE; // Snow water equivalent mm
        public float ReservoirStorage_Total; // % capacity
        
        // Conditions
        public float VegetationCover; // 0-1
        public float ImperviousSurface; // 0-1, urbanization
        public float ErosionRate; // tons/km²/year
        public float SedimentLoad; // tons/day
        
        // Quality
        public float WaterQuality_Index; // 0-1
        public float Turbidity; // NTU
        public float DissolvedOxygen; // mg/L
        public float NutrientLevel; // Eutrophication risk
        
        // Stress indicators
        public float FloodRisk; // 0-1
        public float DroughtRisk; // 0-1
        public float ContaminationRisk; // 0-1
        public float EcologicalHealth; // 0-1
    }
    
    [Serializable]
    public struct LandslideEvent : IComponentData
    {
        public int EventID;
        public int RegionID;
        public LandslideType Type;
        public float2 Location;
        
        // Characteristics
        public float Volume; // Cubic meters
        public float Area; // km²
        public float Depth_Avg; // Meters
        public float Velocity_Max; // m/s
        public float RunoutDistance; // Meters traveled
        public float DropHeight; // Vertical drop
        
        // Trigger
        public TriggerType Trigger;
        public float Precipitation_Antecedent; // mm prior rainfall
        public float SlopeAngle; // Degrees
        public float SoilSaturation; // 0-1
        public float EarthquakePGA; // If seismic trigger
        
        // Material
        public MaterialType MaterialType;
        public float Cohesion; // kPa
        public float FrictionAngle; // Degrees
        public float Permeability; // m/s
        
        // Impacts
        public int Fatalities;
        public int Injuries;
        public int StructuresBuried;
        public int StructuresDamaged;
        public float RoadLengthBlocked; // km
        public float EconomicDamage;
        public bool DammedRiver; // Created landslide lake
        public float LakeVolume; // If dammed river
        
        // Hazard assessment
        public float StabilityFactor; // Factor of safety
        public float RerunProbability; // 0-1, likelihood of recurrence
        public float DebrisFlowPotential; // 0-1
        
        // Temporal
        public int OccurredTick;
        public bool IsActive; // Still moving
    }
    
    public enum LandslideType
    {
        Rockfall,
        RockSlide,
        RockAvalanche,
        DebrisFlow,
        DebrisSlide,
        Mudflow,
        EarthSlide,
        Slump,
        Creep,
        LateralSpread
    }
    
    public enum TriggerType
    {
        HeavyRainfall,
        Earthquake,
        VolcanicActivity,
        Snowmelt,
        WaveErosion,
        HumanExcavation,
        Deforestation,
        Saturation,
        Unknown
    }
    
    public enum MaterialType
    {
        Bedrock,
        WeatheredRock,
        Colluvium,
        Clay,
        Silt,
        Sand,
        Gravel,
        Mixed,
        VolcanicAsh,
        GlacialTill
    }
    
    public class HydrologicalDisasterSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Model watershed hydrology
            // Calculate flood inundation
            // Simulate tsunami propagation
            // Monitor dam safety
            // Assess landslide susceptibility
            // Track water levels and flows
            // Generate warnings
            // Calculate impacts
        }
    }
    
    /// <summary>
    /// Helper methods for hydrological calculations.
    /// </summary>
    public static class HydroCalculator
    {
        public static float CalculateManningFlow(float area, float hydraulicRadius, float slope, float n)
        {
            // Manning's equation: Q = (1/n) * A * R^(2/3) * S^(1/2)
            return (1f / n) * area * math.pow(hydraulicRadius, 2f / 3f) * math.sqrt(slope);
        }
        
        public static float CalculateTsunamiSpeed(float waterDepth)
        {
            // Deep water wave speed: c = sqrt(g * h)
            float g = 9.81f;
            return math.sqrt(g * waterDepth) * 3.6f; // Convert to km/h
        }
        
        public static float CalculateReturnPeriodProbability(float returnPeriod, float years)
        {
            // P = 1 - (1 - 1/T)^n
            float annualProb = 1f / returnPeriod;
            return 1f - math.pow(1f - annualProb, years);
        }
        
        public static float CalculateFactorOfSafety(float cohesion, float frictionAngle, float slopeAngle, float unitWeight, float depth)
        {
            // Infinite slope stability: FS = (c' + (γ*z*cos²β)*tanφ') / (γ*z*sinβ*cosβ)
            float gamma = unitWeight * 9.81f / 1000f; // kN/m³
            float betaRad = slopeAngle * math.PI / 180f;
            float phiRad = frictionAngle * math.PI / 180f;
            
            float numerator = cohesion + (gamma * depth * math.cos(betaRad) * math.cos(betaRad)) * math.tan(phiRad);
            float denominator = gamma * depth * math.sin(betaRad) * math.cos(betaRad);
            
            if (denominator <= 0f) return 999f;
            return numerator / denominator;
        }
        
        public static float CalculatePeakDischarge(float precipitation_mm, float area_km2, float runoffCoefficient)
        {
            // Rational method: Q = CiA (simplified)
            // Convert: mm/hr * km² * coefficient = m³/s
            float intensity_mm_hr = precipitation_mm / 6f; // Assume 6-hour storm
            return runoffCoefficient * intensity_mm_hr * area_km2 / 3.6f;
        }
    }
}
