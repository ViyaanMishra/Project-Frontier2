using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Disasters
{
    /// <summary>
    /// Atmospheric disaster simulation with hurricane tracking,
    /// tornado formation, severe weather prediction, and storm dynamics.
    /// </summary>
    [Serializable]
    public struct TropicalCyclone : IComponentData
    {
        public int StormID;
        public string StormName;
        public StormClassification Classification;
        public int BasinID;
        
        // Position and movement
        public float2 CenterPosition;
        public float CurrentHeading; // Degrees
        public float ForwardSpeed; // km/h
        public float2 MovementVector;
        
        // Intensity
        public float MaximumSustainedWinds; // km/h
        public float MinimumCentralPressure; // hPa
        public float WindRadii_34kt; // NE, SE, SW, NW quadrants
        public float WindRadii_50kt;
        public float WindRadii_64kt;
        public float EyeDiameter; // km (0 if no eye)
        
        // Structure
        public float RadiusOfMaximumWinds; // km
        public float OuterCoreRadius; // km
        public float SymmetryIndex; // 0-1, circular to asymmetric
        public float EyewallReplacementCycle; // 0-1 if occurring
        
        // Environment
        public float SeaSurfaceTemperature; // °C
        public float OceanHeatContent; // kJ/cm²
        public float VerticalWindShear; // kt
        public float MidLevelHumidity; // %
        public float OutflowQuality; // 0-1
        
        // Forecast
        public NativeArray<float2> TrackForecast; // 5-day positions
        public NativeArray<float> IntensityForecast; // 5-day winds
        public float LandfallProbability_48h; // 0-1
        public float RapidIntensificationRisk; // 0-1
        
        // Impacts
        public float StormSurge_Maximum; // Meters
        public float Rainfall_Total; // mm
        public float TornadoOutbreakRisk; // 0-1
        public int AffectedPopulation;
        public float EconomicDamage;
        
        // Temporal
        public int FormationTick;
        public int DissipationTick;
        public bool IsActive;
    }
    
    public enum StormClassification
    {
        TropicalDepression,   // < 63 km/h
        TropicalStorm,        // 63-118 km/h
        Hurricane_Cat1,       // 119-153 km/h
        Hurricane_Cat2,       // 154-177 km/h
        Hurricane_Cat3,       // 178-208 km/h (Major)
        Hurricane_Cat4,       // 209-251 km/h (Major)
        Hurricane_Cat5,       // > 252 km/h (Major)
        Typhoon,
        SuperTyphoon,
        Cyclone
    }
    
    [Serializable]
    public struct TornadoEvent : IComponentData
    {
        public int EventID;
        public int RegionID;
        public float2 PathStart;
        public float2 PathEnd;
        
        // Characteristics
        public float EF_Scale; // 0-5 Enhanced Fujita
        public float MaximumWinds; // km/h
        public float PathLength; // km
        public float PathWidth; // Meters
        public float Duration; // Minutes
        
        // Parent storm
        public int ParentSupercellID;
        public float MesocycloneStrength; // 0-1
        public float RotationVelocity; // m/s
        
        // Damage
        public int Fatalities;
        public int Injuries;
        public int StructuresDestroyed;
        public int StructuresDamaged;
        public float EconomicDamage;
        public float DamagePathSeverity; // 0-1 average
        
        // Meteorological conditions
        public float CAPE; // Convective Available Potential Energy
        public float Helicity; // Storm-relative helicity
        public float Shear_0_6km; // Bulk shear
        public float LCL_Height; // Lifted Condensation Level
        public float CIN; // Convective Inhibition
        
        // Temporal
        public int TouchdownTick;
        public int LiftoffTick;
        public bool IsConfirmed;
        public bool IsDamaging;
    }
    
    [Serializable]
    public struct SevereThunderstorm : IComponentData
    {
        public int StormID;
        public int RegionID;
        public float2 Position;
        public StormMode Mode;
        
        // Dynamics
        public float UpdraftVelocity; // m/s
        public float DowndraftVelocity; // m/s
        public float RotationalVelocity; // m/s (if supercell)
        public float CloudTopHeight; // km
        public float EchoTopHeight; // km (radar)
        
        // Hazards
        public float HailSize_Max; // cm diameter
        public float HailFallRate; // Stones/m²/min
        public float WindGust_Max; // km/h
        public float DownburstIntensity; // 0-1
        public float LightningRate; // Flashes/min
        public float RainfallRate; // mm/hour
        
        // Supercell characteristics
        public bool IsSupercell;
        public float MesocycloneDepth; // km
        public float RotationDepth; // km
        public float Vorticity; // s^-1
        public float BWER_Size; // Bounded Weak Echo Region
        
        // Evolution
        public StormPhase Phase;
        public float LifecycleProgress; // 0-1
        public float MergingPotential; // With other storms
        public bool IsSplitting; // Splitting supercell
        public bool IsOccluding;
        
        // Warning status
        public bool HasWarningIssued;
        public float WarningLeadTime; // Minutes
        public float FalseAlarmRisk; // 0-1
    }
    
    public enum StormMode
    {
        SingleCell,
        MultiCellCluster,
        MultiCellLine,      // Squall line
        Supercell_Classic,
        Supercell_HP,       // High precipitation
        Supercell_LP,       // Low precipitation
        MCS,                // Mesoscale Convective System
        Derecho,
        QuasiLinear
    }
    
    public enum StormPhase
    {
        Developing,
        Mature,
        Decaying,
        Regenerating,
        Merging,
        Dissipating
    }
    
    [Serializable]
    public struct BlizzardEvent : IComponentData
    {
        public int EventID;
        public int RegionID;
        public float2 CenterPosition;
        public float SpatialExtent; // km²
        
        // Conditions
        public float WindSpeed_Sustained; // km/h
        public float WindGusts; // km/h
        public float Visibility; // Meters
        public float Temperature; // Celsius
        public float WindChill; // Celsius
        
        // Snow
        public float SnowfallRate; // cm/hour
        public float TotalSnowfall; // cm
        public float SnowWaterEquivalent; // mm
        public float SnowDensity; // kg/m³
        public float DriftingSeverity; // 0-1
        
        // Duration
        public float Duration_Hours;
        public int StartTick;
        public int EndTick;
        public bool IsActive;
        
        // Impacts
        public int TravelDisruptions; // Road closures, flight cancellations
        public int PowerOutages;
        public int ShelterOpenings;
        public int Fatalities;
        public float EconomicDamage;
        public float AgriculturalDamage;
        
        // Classification
        public bool MeetsBlizzardCriteria; // NWS definition
        public float NESIS_Score; // Northeast Snow Impact Scale
        public float RSIS_Score; // Regional Snowfall Index
    }
    
    [Serializable]
    public struct DroughtEvent : IComponentData
    {
        public int EventID;
        public int RegionID;
        public DroughtType Type;
        
        // Extent
        public float AreaAffected; // km²
        public float SeverityIndex; // 0-1
        public float Duration_Months;
        public int StartTick;
        
        // Meteorological indicators
        public float PrecipitationDeficit; // % below normal
        public float SPI_Value; // Standardized Precipitation Index
        public float PDSI_Value; // Palmer Drought Severity Index
        public float SoilMoisture_Anomaly; // % below normal
        public float GroundwaterLevel_Anomaly; // Meters below normal
        
        // Impacts
        public float CropYieldReduction; // % reduction
        public float LivestockImpact; // 0-1
        public float WaterSupplyStress; // 0-1
        public float WildfireRiskIncrease; // 0-1
        public float EcosystemStress; // 0-1
        
        // Socioeconomic
        public int AffectedPopulation;
        public float AgriculturalLosses; // USD
        public float MunicipalRestrictions; // 0-1, restriction level
        public bool IsEmergencyDeclared;
        public float ReliefCost;
        
        // Recovery
        public float RecoveryProgress; // 0-1
        public float ResilienceIndex; // 0-1
        public float TimeToRecovery; // Months projected
    }
    
    public enum DroughtType
    {
        Meteorological,     // Precipitation deficit
        Agricultural,       // Soil moisture deficit
        Hydrological,       // Water supply deficit
        Socioeconomic       // Supply/demand imbalance
    }
    
    [Serializable]
    public struct HeatWaveEvent : IComponentData
    {
        public int EventID;
        public int RegionID;
        public float2 CoverageArea; // Centroid and radius
        
        // Temperature
        public float MaxTemperature; // Celsius
        public float TemperatureAnomaly; // Above normal
        public float Duration_Days;
        public float ApparentTemperature; // Heat index
        public float WetBulbTemperature; // Critical threshold
        
        // Conditions
        public float Humidity; // %
        public float DewPoint; // Celsius
        public float NighttimeMinimum; // Overnight low
        public float UrbanHeatIslandEffect; // °C enhancement
        
        // Impacts
        public int HeatRelatedIllnesses;
        public int HeatRelatedDeaths;
        public int Hospitalizations;
        public float MortalityExcess; // % above baseline
        public float LaborProductivityLoss; // %
        
        // Infrastructure
        public float PowerDemandPeak; // MW
        public bool GridStressed;
        public int RollingBlackouts;
        public float TransportationImpacts; // Rail buckling, road melting
        
        // Response
        public bool IsWarningIssued;
        public int CoolingCentersOpened;
        public float PublicHealthResponse; // 0-1
    }
    
    public class AtmosphericDisasterSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Track tropical cyclone development
            // Calculate storm intensity changes
            // Model tornado formation conditions
            // Simulate severe thunderstorm evolution
            // Process blizzard dynamics
            // Monitor drought progression
            // Track heat wave development
            // Generate warnings and forecasts
            // Calculate impacts and damages
        }
    }
    
    /// <summary>
    /// Helper methods for atmospheric disaster calculations.
    /// </summary>
    public static class AtmosphericCalculator
    {
        public static float CalculateHurricanePI(float SST, float outflowTemp, float surfacePressure)
        {
            // Potential Intensity theory (Emanuel)
            float CAPE_s = 300f; // Simplified surface CAPE
            float CAPE_b = 100f; // Boundary layer CAPE
            float Tk = outflowTemp + 273.15f; // Kelvin
            float Ts = SST + 273.15f; // Kelvin
            
            float efficiency = (Ts - Tk) / Ts;
            float maxWind = math.sqrt(efficiency * (CAPE_s - CAPE_b));
            
            return maxWind * 3.6f; // Convert to km/h
        }
        
        public static StormClassification GetSaffirSimpsonCategory(float windSpeedKmh)
        {
            if (windSpeedKmh < 63f) return StormClassification.TropicalDepression;
            if (windSpeedKmh < 119f) return StormClassification.TropicalStorm;
            if (windSpeedKmh < 154f) return StormClassification.Hurricane_Cat1;
            if (windSpeedKmh < 178f) return StormClassification.Hurricane_Cat2;
            if (windSpeedKmh < 209f) return StormClassification.Hurricane_Cat3;
            if (windSpeedKmh < 252f) return StormClassification.Hurricane_Cat4;
            return StormClassification.Hurricane_Cat5;
        }
        
        public static float CalculateHeatIndex(float tempC, float humidityPercent)
        {
            // Simplified heat index calculation
            float tempF = tempC * 1.8f + 32f;
            float hi = -42.379f + 2.04901523f * tempF + 10.14333127f * humidityPercent
                       - 0.22475541f * tempF * humidityPercent - 0.00683783f * tempF * tempF
                       - 0.05481717f * humidityPercent * humidityPercent
                       + 0.00122874f * tempF * tempF * humidityPercent
                       + 0.00085282f * tempF * humidityPercent * humidityPercent
                       - 0.00000199f * tempF * tempF * humidityPercent * humidityPercent;
            
            return (hi - 32f) / 1.8f; // Convert back to Celsius
        }
        
        public static float CalculateTornadoCAPEShear(CAPE, float shear)
        {
            // Significant Tornado Parameter (STP)
            float effectiveSRH = shear * 0.5f; // Simplified storm-relative helicity
            float effectiveBulkShear = shear;
            
            if (CAPE < 500f || effectiveBulkShear < 10f) return 0f;
            
            float stp = (CAPE / 1500f) * ((20000f - effectiveSRH) / 10000f) 
                       * (effectiveBulkShear / 20f);
            
            return math.clamp(stp, 0f, 10f);
        }
    }
}
