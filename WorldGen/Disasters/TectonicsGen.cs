using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Disasters
{
    /// <summary>
    /// Tectonic disaster simulation with plate dynamics, earthquake physics,
    /// fault mechanics, and seismic hazard modeling.
    /// </summary>
    [Serializable]
    public struct TectonicPlate : IComponentData
    {
        public int PlateID;
        public string PlateName;
        public PlateType Type;
        public float Area; // km²
        
        // Movement
        public float2 VelocityVector; // Direction and speed (cm/year)
        public float RotationRate; // Degrees/million years
        public float2 EulerPole; // Rotation axis
        
        // Boundaries
        public NativeArray<int> BoundarySegmentIDs;
        public int AdjacentPlateCount;
        public float BoundaryLength; // Total boundary km
        
        // Stress state
        public float3 StressTensor; // Principal stresses
        public float StrainAccumulation; // Energy stored
        public float LastReleaseTime; // Time since last major quake
        
        // Properties
        public float CrustThickness; // km
        public float LithosphereThickness; // km
        public float Density; // kg/m³
        public float Temperature_Gradient; // °C/km
    }
    
    public enum PlateType
    {
        Oceanic,
        Continental,
        Mixed
    }
    
    [Serializable]
    public struct FaultZone : IComponentData
    {
        public int FaultID;
        public string FaultName;
        public FaultType Type;
        public int PlateBoundaryA;
        public int PlateBoundaryB;
        
        // Geometry
        public float Length; // km
        public float Width; // km (seismogenic zone)
        public float Dip; // Degrees from horizontal
        public float Strike; // Degrees from north
        public float2 StartPoint;
        public float2 EndPoint;
        
        // Mechanics
        public float SlipRate; // mm/year
        public float FrictionCoefficient; // 0-1
        public float Coupling; // Locked vs creeping 0-1
        public float StressDrop; // MPa during rupture
        
        // Seismic history
        public float LastMajorEvent; // Years ago
        public float RecurrenceInterval; // Average years between events
        public float TimeSinceLastEvent; // Years
        public float ProbabilityOfRupture; // 0-1, next 30 years
        
        // Current state
        public float AccumulatedSlipDeficit; // Meters of unrelieved strain
        public float CurrentStressLevel; // 0-1 of failure threshold
        public bool IsLocked;
        public bool IsCreeping;
        public float AseismicSlip; // % slip without earthquakes
    }
    
    public enum FaultType
    {
        StrikeSlip_RightLateral,
        StrikeSlip_LeftLateral,
        Normal,
        Reverse,
        Thrust,
        Oblique,
        Detachment
    }
    
    [Serializable]
    public struct EarthquakeEvent : IComponentData
    {
        public int EventID;
        public int FaultID;
        public int RegionID;
        public float2 Epicenter;
        public float Depth; // km
        
        // Magnitude
        public float Magnitude_Mw; // Moment magnitude
        public float Magnitude_Ml; // Local (Richter)
        public float SeismicMoment; // N·m
        public float EnergyRelease; // Joules
        
        // Rupture characteristics
        public float RuptureLength; // km
        public float RuptureWidth; // km
        public float RuptureVelocity; // km/s
        public float RuptureDuration; // Seconds
        public float MaximumSlip; // Meters
        public float AverageSlip; // Meters
        
        // Ground motion
        public float PGA_Epicenter; // Peak ground acceleration (g)
        public float PGV_Epicenter; // Peak ground velocity (cm/s)
        public float MMI_Maximum; // Modified Mercalli Intensity (I-XII)
        public NativeArray<float> ShakeMap; // Spatial distribution
        
        // Impacts
        public int Fatalities;
        public int Injuries;
        public int Displaced;
        public float EconomicDamage; // USD
        public int BuildingsDestroyed;
        public int BuildingsDamaged;
        
        // Secondary effects
        public bool GeneratedTsunami;
        public float TsunamiHeight; // Meters
        public bool TriggeredLandslides;
        public int LandslideCount;
        public bool CausedLiquefaction;
        public float LiquefactionArea; // km²
        
        // Temporal
        public int OccurredTick;
        public bool IsMainshock;
        public int AftershockCount;
        public float MaxAftershockMagnitude;
    }
    
    [Serializable]
    public struct SeismicHazard : IComponentData
    {
        public int RegionID;
        
        // Probabilistic assessment
        public float PGA_475yr; // Peak ground acceleration, 475-year return period
        public float PGA_2475yr; // 2475-year return period
        public float SpectralAcceleration_Ss; // Short period (0.2s)
        public float SpectralAcceleration_S1; // Long period (1.0s)
        
        // Deterministic scenarios
        public float MCE_PGA; // Maximum Considered Earthquake
        public float DBE_PGA; // Design Basis Earthquake
        
        // Site conditions
        public SoilClass SiteClass;
        public float Vs30; // Shear wave velocity (m/s)
        public float BasinDepth; // Sediment basin depth (km)
        public float TopographicAmplification; // Hill/ridge effects
        
        // Hazard metrics
        public float AnnualExceedanceProbability; // For given PGA
        public float ReturnPeriod; // Years
        public float ExpectedLoss; // Annual average loss
        
        // Building vulnerability
        public float BuildingStockVulnerability; // 0-1
        public float LifelineVulnerability; // Infrastructure
        public float CriticalFacilityRisk; // Hospitals, etc.
    }
    
    public enum SoilClass
    {
        HardRock,      // Vs30 > 1500 m/s
        Rock,          // 760-1500 m/s
        DenseSoil,     // 360-760 m/s
        StiffSoil,     // 180-360 m/s
        SoftSoil,      // < 180 m/s
        Special        // Requires site-specific analysis
    }
    
    [Serializable]
    public struct VolcanicSystem : IComponentData
    {
        public int VolcanoID;
        public string VolcanoName;
        public VolcanoType Type;
        public float2 Location;
        public float Elevation; // Meters
        
        // Eruption history
        public int TotalEruptions;
        public int HistoricEruptions; // Recorded history
        public float LastEruptionTime; // Years ago
        public float LargestHistoricMagnitude; // VEI
        
        // Current state
        public VolcanoAlertLevel AlertLevel;
        public float UnrestLevel; // 0-1
        public bool IsErupting;
        public float EruptionStartDate;
        
        // Magma system
        public float MagmaChamberDepth; // km
        public float MagmaChamberVolume; // km³
        public float MagmaSupplyRate; // km³/year
        public float MagmaComposition; // Silica content %
        public float GasContent; // Volatiles %
        
        // Monitoring data
        public float SeismicityRate; // Earthquakes/day
        public float DeformationRate; // mm/day (inflation/deflation)
        public float GasEmissionRate; // tons/day SO2
        public float ThermalAnomaly; // Temperature increase
        
        // Hazards
        public float ExplosionPotential; // 0-1
        public float LavaFlowRisk; // 0-1
        public float PyroclasticFlowRisk; // 0-1
        public float LaharRisk; // 0-1
        public float AshFallRisk; // 0-1
        public float GasHazardRisk; // 0-1
    }
    
    public enum VolcanoType
    {
        Shield,
        Stratovolcano,
        CinderCone,
        LavaDome,
        Caldera,
        Fissure,
        Submarine,
        Supervolcano
    }
    
    public enum VolcanoAlertLevel
    {
        Normal,         // No unrest
        Advisory,       // Low-level unrest
        Watch,          // Escalating unrest
        Warning         // Eruption imminent or ongoing
    }
    
    public enum VEI // Volcanic Explosivity Index
    {
        VEI_0 = 0,      // Non-explosive
        VEI_1 = 1,      // Small
        VEI_2 = 2,      // Moderate
        VEI_3 = 3,      // Moderate-large
        VEI_4 = 4,      // Large
        VEI_5 = 5,      // Very large
        VEI_6 = 6,      // Huge
        VEI_7 = 7,      // Super-colossal
        VEI_8 = 8       // Mega-colossal
    }
    
    public class TectonicsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Calculate plate motions
            // Accumulate stress on faults
            // Check for earthquake triggers
            // Generate earthquake events
            // Calculate ground motion
            // Assess damage
            // Monitor volcanic unrest
            // Update hazard maps
        }
    }
    
    /// <summary>
    /// Helper methods for seismic calculations.
    /// </summary>
    public static class SeismicCalculator
    {
        public static float MagnitudeToEnergy(float magnitude)
        {
            // Log E = 4.8 + 1.5M (energy in Joules)
            return math.pow(10f, 4.8f + 1.5f * magnitude);
        }
        
        public static float EnergyToMagnitude(float energyJoules)
        {
            return (math.log10(energyJoules) - 4.8f) / 1.5f;
        }
        
        public static float CalculatePGA(float magnitude, float distance, float depth)
        {
            // Simplified attenuation relationship
            float r = math.sqrt(distance * distance + depth * depth);
            float pga = math.pow(10f, 0.5f * magnitude - math.log10(r) - 2.0f);
            return math.clamp(pga, 0.01f, 2.0f); // Cap at 2g
        }
        
        public static float CalculateRecurrenceProbability(float recurrenceInterval, float timeSinceLast)
        {
            // Poisson model: P = 1 - exp(-t/T)
            float t = timeSinceLast;
            float T = recurrenceInterval;
            if (T <= 0) return 0f;
            return 1f - math.exp(-t / T);
        }
        
        public static float MomentToMagnitude(float seismicMoment)
        {
            // Mw = (2/3) * log10(M0) - 6.07
            return (2f / 3f) * math.log10(seismicMoment) - 6.07f;
        }
    }
}
