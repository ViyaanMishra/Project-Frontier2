using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Environment
{
    /// <summary>
    /// Climate system with carbon cycle, greenhouse gas dynamics,
    /// temperature modeling, and climate feedback loops.
    /// </summary>
    [Serializable]
    public struct ClimateSystem : IComponentData
    {
        public int ClimateRegionID;
        public int GlobalClimateID;
        
        // Temperature
        public float AverageTemperature; // Celsius
        public float TemperatureAnomaly; // Deviation from baseline
        public float MinTemperature;
        public float MaxTemperature;
        public float SeasonalVariation;
        
        // Carbon cycle
        public float AtmosphericCO2; // ppm
        public float OceanicCO2; // Gigatons absorbed
        public float TerrestrialCO2; // Gigatons in biomass
        public float CO2Flux_AtmosphereOcean; // Exchange rate
        public float CO2Flux_AtmosphereLand; // Exchange rate
        public float CO2Flux_Anthropogenic; // Human emissions
        
        // Greenhouse gases
        public float MethaneConcentration; // ppb
        public float NitrousOxideConcentration; // ppb
        public float WaterVaporConcentration; // %
        public float TotalGHGForcing; // Radiative forcing W/m²
        
        // Energy balance
        public float SolarInsolation; // W/m² incoming
        public float Albedo; // Reflectivity 0-1
        public float OutgoingLongwave; // W/m² outgoing
        public float RadiativeForcing; // Net forcing W/m²
        public float EnergyImbalance; // W/m² (positive = warming)
        
        // Feedback loops
        public float IceAlbedoFeedback; // Strength 0-1
        public float WaterVaporFeedback; // Strength 0-1
        public float CloudFeedback; // Strength (can be negative)
        public float PermafrostCarbonFeedback; // Strength 0-1
        public float ForestDiebackFeedback; // Strength 0-1
        
        // Climate sensitivity
        public float EquilibriumSensitivity; // °C per CO2 doubling
        public float TransientSensitivity; // °C per CO2 doubling (fast)
        
        // Temporal
        public int SimulationYear;
        public float DecadalTrend; // °C per decade
    }
    
    [Serializable]
    public struct CarbonCycle : IComponentData
    {
        public int RegionID;
        
        // Reservoirs (Gigatons C)
        public float Atmosphere_Reservoir;
        public float Ocean_Surface_Reservoir;
        public float Ocean_Deep_Reservoir;
        public float Vegetation_Reservoir;
        public float Soil_Reservoir;
        public float Fossil_Reservoir;
        public float Sediment_Reservoir;
        
        // Fluxes (GtC/year)
        public float Photosynthesis_Flux; // Atmosphere → Vegetation
        public float Respiration_Flux; // Vegetation → Atmosphere
        public float Decomposition_Flux; // Soil → Atmosphere
        public float OceanUptake_Flux; // Atmosphere → Ocean
        public float OceanOutgassing_Flux; // Ocean → Atmosphere
        public float FossilEmission_Flux; // Fossil → Atmosphere
        public float LandUseChange_Flux; // Vegetation → Atmosphere
        
        // Rates
        public float AtmosphericGrowthRate; // ppm/year
        public float OceanAcidificationRate; // pH change/year
        public float CarbonSinkEfficiency; // 0-1
        
        // Isotopes (for tracking sources)
        public float C13Ratio; // δ13C signature
        public float C14Ratio; // Radiocarbon (fossil fuel detection)
    }
    
    [Serializable]
    public struct WeatherPattern : IComponentData
    {
        public int PatternID;
        public int RegionID;
        public WeatherPatternType Type;
        
        // Characteristics
        public float Intensity; // 0-1
        public float Duration; // Days
        public float SpatialExtent; // km²
        public float2 MovementVector; // Direction and speed
        
        // Meteorological variables
        public float Pressure; // hPa
        public float PressureTendency; // Rising/falling
        public float Humidity; // %
        public float PrecipitationRate; // mm/hour
        public float WindSpeed; // km/h
        public float WindDirection; // Degrees
        public float Temperature; // Celsius
        public float CloudCover; // 0-1
        
        // Specific pattern data
        public float CycloneCentralPressure; // For storms
        public float FrontTemperatureGradient; // For fronts
        public float JetStreamPosition; // Latitude
        public float OscillationIndex; // For ENSO, NAO, etc.
        
        // Impacts
        public float HazardLevel; // 0-1
        public bool IsExtreme;
        public int AffectedPopulation;
        public float EconomicDamage;
    }
    
    public enum WeatherPatternType
    {
        HighPressure,
        LowPressure,
        ColdFront,
        WarmFront,
        OccludedFront,
        StationaryFront,
        TropicalCyclone,
        ExtratropicalCyclone,
        ThunderstormComplex,
        HeatWave,
        ColdWave,
        Drought,
        AtmosphericRiver,
        Monsoon,
        ENSO_Warm, // El Niño
        ENSO_Cool, // La Niña
        NAO_Positive,
        NAO_Negative
    }
    
    [Serializable]
    public struct BiomeState : IComponentData
    {
        public int BiomeID;
        public int RegionID;
        public BiomeType Type;
        
        // Vegetation
        public float VegetationCover; // 0-1
        public float BiomassDensity; // kg/m²
        public float PrimaryProductivity; // gC/m²/year
        public float LeafAreaIndex; // LAI
        public NativeArray<int> DominantSpeciesIDs;
        
        // Climate envelope
        public float TemperatureOptimum;
        public float PrecipitationOptimum;
        public float ClimateSuitability; // 0-1, current vs optimum
        
        // State transitions
        public float DegradationLevel; // 0-1
        public float RestorationPotential; // 0-1
        public float TippingPointProximity; // 0-1, closeness to regime shift
        public BiomeState CurrentState;
        public BiomeState PotentialAlternateState;
        
        // Services
        public float CarbonSequestration; // tons C/year
        public float WaterRegulation; // 0-1
        public float SoilRetention; // 0-1
        public float BiodiversitySupport; // 0-1
        
        // Disturbances
        public float FireFrequency; // Fires per decade
        public float FireSeverity; // 0-1
        public float PestPressure; // 0-1
        public float InvasiveSpeciesCover; // 0-1
    }
    
    public enum BiomeType
    {
        TropicalRainforest,
        TemperateForest,
        BorealForest,
        Savanna,
        Grassland,
        Desert,
        Tundra,
        Mangrove,
        Wetland,
        CoralReef,
        KelpForest,
        OpenOcean,
        Alpine,
        Mediterranean
    }
    
    public enum BiomeState
    {
        Pristine,
        Healthy,
        Degraded,
        Critical,
        Collapsed,
        Transitioning,
        Restored
    }
    
    [Serializable]
    public struct OceanState : IComponentData
    {
        public int OceanRegionID;
        
        // Physical properties
        public float SurfaceTemperature; // Celsius
        public float DeepTemperature; // Celsius
        public float Salinity; // PSU
        public float Density; // kg/m³
        public float MixedLayerDepth; // meters
        
        // Chemistry
        public float pH; // Acidity
        public float AragoniteSaturation; // For calcifying organisms
        public float DissolvedOxygen; // mg/L
        public float NutrientConcentration; // Nitrate, phosphate
        
        // Circulation
        public float2 SurfaceCurrent; // Velocity vector
        public float2 DeepCurrent; // Velocity vector
        public float AMOCStrength; // Atlantic Meridional Overturning
        public float UpwellingIntensity; // 0-1
        
        // Biology
        public float ChlorophyllConcentration; // Phytoplankton
        public float PrimaryProductivity; // gC/m²/year
        public float FishBiomass; // tons/km²
        public float CoralCover; // 0-1 (for reef regions)
        
        // Health indicators
        public float OceanHealthIndex; // 0-1 composite
        public float AcidificationLevel; // 0-1
        public float DeoxygenationLevel; // 0-1
        public float PollutionLevel; // 0-1
        public float OverfishingPressure; // 0-1
        
        // Climate role
        public float HeatUptake; // W/m²
        public float CarbonUptake; // GtC/year
        public float SeaIceExtent; // km² (for polar regions)
    }
    
    [Serializable]
    public struct CryosphereState : IComponentData
    {
        public int RegionID;
        public CryosphereComponent Component;
        
        // Extent and volume
        public float Area; // km²
        public float Volume; // km³
        public float Thickness; // meters
        public float AreaAnomaly; // % from baseline
        public float VolumeAnomaly; // % from baseline
        
        // Mass balance
        public float AccumulationRate; // Snow/ice gain
        public float AblationRate; // Melting/sublimation loss
        public float MassBalance; // Net gain/loss
        public float CalvingRate; // Iceberg calving (for glaciers)
        
        // Dynamics
        public float FlowVelocity; // m/year (for glaciers)
        public float GroundingLinePosition; // For ice shelves
        public float EquilibriumLineAltitude; // ELA for glaciers
        
        // Temperature
        public float SurfaceTemperature; // Celsius
        public float BasalTemperature; // At ice-bedrock interface
        public float MeltSeasonLength; // Days
        
        // Sea ice specific
        public float Concentration; // 0-1
        public float Age; // Years (multi-year ice)
        public float SnowCover; // Depth on ice
        
        // Contribution to sea level
        public float SeaLevelEquivalent; // Meters if fully melted
        public float CurrentContributionRate; // mm/year
    }
    
    public enum CryosphereComponent
    {
        IceSheet_Greenland,
        IceSheet_Antarctic,
        Glacier_Mountain,
        IceCap,
        SeaIce_Arctic,
        SeaIce_Antarctic,
        Permafrost,
        SnowCover,
        LakeIce,
        RiverIce
    }
    
    public class ClimateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Calculate energy balance
            // Update carbon cycle fluxes
            // Model temperature changes
            // Process feedback loops
            // Simulate weather patterns
            // Track biome state changes
            // Update ocean conditions
            // Calculate cryosphere dynamics
            // Project sea level rise
        }
    }
}
