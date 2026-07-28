using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Biome data structure with 6 main biomes, sub-biomes, and micro-biomes.
    /// </summary>
    [Serializable]
    public struct BiomeData
    {
        public BiomeType mainBiome;
        public SubBiomeType subBiome;
        public MicroBiomeType microBiome;
        
        // Environmental properties
        public float baseTemperature;      // Celsius
        public float temperatureVariance;  // Daily variance
        public float humidity;             // 0-1
        public float rainfall;             // mm per day average
        public float windSpeed;            // m/s average
        public float terrainRoughness;     // 0-1
        public float vegetationDensity;    // 0-1
        public float resourceRichness;     // 0-1
        
        // Visual properties
        public Color groundColor;
        public Color vegetationColor;
        public Color skyColor;
        public Color fogColor;
        public float fogDensity;
        
        // Spawn tables
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public FloraSpawnEntry[] floraSpawns;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public FaunaSpawnEntry[] faunaSpawns;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public ResourceSpawnEntry[] resourceSpawns;
        
        // Hazard properties
        public HazardType prevalentHazard;
        public float hazardIntensity;
        
        // Anomaly properties
        public bool hasAnomalyActivity;
        public float anomalyStrength;
        public AnomalyType anomalyType;
        
        public void Initialize(BiomeType main, SubBiomeType sub, MicroBiomeType micro)
        {
            mainBiome = main;
            subBiome = sub;
            microBiome = micro;
            
            SetBaseProperties();
            SetupSpawnTables();
        }
        
        private void SetBaseProperties()
        {
            switch (mainBiome)
            {
                case BiomeType.TemperateForest:
                    baseTemperature = 18f; temperatureVariance = 12f;
                    humidity = 0.65f; rainfall = 4.5f; windSpeed = 3.5f;
                    terrainRoughness = 0.4f; vegetationDensity = 0.8f;
                    resourceRichness = 0.6f;
                    groundColor = new Color(0.3f, 0.25f, 0.15f);
                    vegetationColor = new Color(0.2f, 0.5f, 0.15f);
                    skyColor = new Color(0.5f, 0.7f, 0.9f);
                    fogColor = new Color(0.7f, 0.75f, 0.7f);
                    fogDensity = 0.02f;
                    prevalentHazard = HazardType.Wildlife;
                    break;
                    
                case BiomeType.AridDesert:
                    baseTemperature = 38f; temperatureVariance = 25f;
                    humidity = 0.15f; rainfall = 0.2f; windSpeed = 8f;
                    terrainRoughness = 0.3f; vegetationDensity = 0.1f;
                    resourceRichness = 0.4f;
                    groundColor = new Color(0.8f, 0.7f, 0.5f);
                    vegetationColor = new Color(0.5f, 0.5f, 0.3f);
                    skyColor = new Color(0.9f, 0.8f, 0.6f);
                    fogColor = new Color(0.9f, 0.85f, 0.7f);
                    fogDensity = 0.01f;
                    prevalentHazard = HazardType.HeatStroke;
                    break;
                    
                case BiomeType.ArcticTundra:
                    baseTemperature = -25f; temperatureVariance = 15f;
                    humidity = 0.4f; rainfall = 0.5f; windSpeed = 12f;
                    terrainRoughness = 0.5f; vegetationDensity = 0.15f;
                    resourceRichness = 0.3f;
                    groundColor = new Color(0.9f, 0.9f, 0.95f);
                    vegetationColor = new Color(0.4f, 0.4f, 0.35f);
                    skyColor = new Color(0.7f, 0.8f, 0.9f);
                    fogColor = new Color(0.85f, 0.9f, 0.95f);
                    fogDensity = 0.03f;
                    prevalentHazard = HazardType.Hypothermia;
                    break;
                    
                case BiomeType.SwampMarsh:
                    baseTemperature = 28f; temperatureVariance = 8f;
                    humidity = 0.95f; rainfall = 8f; windSpeed = 2f;
                    terrainRoughness = 0.2f; vegetationDensity = 0.7f;
                    resourceRichness = 0.7f;
                    groundColor = new Color(0.2f, 0.15f, 0.1f);
                    vegetationColor = new Color(0.3f, 0.4f, 0.2f);
                    skyColor = new Color(0.6f, 0.65f, 0.6f);
                    fogColor = new Color(0.5f, 0.55f, 0.45f);
                    fogDensity = 0.08f;
                    prevalentHazard = HazardType.Disease;
                    break;
                    
                case BiomeType.VolcanicWasteland:
                    baseTemperature = 45f; temperatureVariance = 10f;
                    humidity = 0.1f; rainfall = 0.1f; windSpeed = 5f;
                    terrainRoughness = 0.8f; vegetationDensity = 0.02f;
                    resourceRichness = 0.8f;
                    groundColor = new Color(0.2f, 0.15f, 0.15f);
                    vegetationColor = new Color(0.1f, 0.1f, 0.1f);
                    skyColor = new Color(0.4f, 0.3f, 0.25f);
                    fogColor = new Color(0.3f, 0.25f, 0.2f);
                    fogDensity = 0.05f;
                    prevalentHazard = HazardType.Radiation;
                    hasAnomalyActivity = true;
                    anomalyStrength = 0.6f;
                    anomalyType = AnomalyType.ThermalVent;
                    break;
                    
                case BiomeType.AnomalyZone:
                    baseTemperature = 20f; temperatureVariance = 30f;
                    humidity = 0.5f; rainfall = 2f; windSpeed = 15f;
                    terrainRoughness = 0.9f; vegetationDensity = 0.3f;
                    resourceRichness = 0.9f;
                    groundColor = new Color(0.4f, 0.2f, 0.5f);
                    vegetationColor = new Color(0.6f, 0.3f, 0.7f);
                    skyColor = new Color(0.3f, 0.2f, 0.4f);
                    fogColor = new Color(0.5f, 0.3f, 0.6f);
                    fogDensity = 0.1f;
                    prevalentHazard = HazardType.RealityDistortion;
                    hasAnomalyActivity = true;
                    anomalyStrength = 1f;
                    anomalyType = AnomalyType.RealityFlux;
                    break;
            }
            
            // Apply sub-biome modifiers
            ApplySubBiomeModifiers();
        }
        
        private void ApplySubBiomeModifiers()
        {
            switch (subBiome)
            {
                case SubBiomeType.Dense:
                    vegetationDensity *= 1.3f;
                    resourceRichness *= 1.1f;
                    windSpeed *= 0.5f;
                    break;
                case SubBiomeType.Sparse:
                    vegetationDensity *= 0.5f;
                    resourceRichness *= 0.7f;
                    windSpeed *= 1.3f;
                    break;
                case SubBiomeType.Mountainous:
                    terrainRoughness *= 1.5f;
                    temperatureVariance *= 1.3f;
                    windSpeed *= 1.5f;
                    break;
                case SubBiomeType.Coastal:
                    humidity = Mathf.Max(humidity, 0.7f);
                    rainfall *= 1.2f;
                    temperatureVariance *= 0.7f;
                    break;
                case SubBiomeType.Urban:
                    baseTemperature += 3f; // Urban heat island
                    resourceRichness *= 1.5f;
                    vegetationDensity *= 0.3f;
                    prevalentHazard = HazardType.Hostiles;
                    break;
            }
        }
        
        private void SetupSpawnTables()
        {
            floraSpawns = new FloraSpawnEntry[10];
            faunaSpawns = new FaunaSpawnEntry[8];
            resourceSpawns = new ResourceSpawnEntry[5];
            
            // Populate based on biome type (simplified example)
            switch (mainBiome)
            {
                case BiomeType.TemperateForest:
                    floraSpawns[0] = new FloraSpawnEntry { floraType = FloraType.OakTree, density = 0.3f };
                    floraSpawns[1] = new FloraSpawnEntry { floraType = FloraType.PineTree, density = 0.2f };
                    floraSpawns[2] = new FloraSpawnEntry { floraType = FloraType.BerryBush, density = 0.15f };
                    faunaSpawns[0] = new FaunaSpawnEntry { faunaType = FaunaType.Deer, density = 0.1f };
                    faunaSpawns[1] = new FaunaSpawnEntry { faunaType = FaunaType.Boar, density = 0.08f };
                    resourceSpawns[0] = new ResourceSpawnEntry { resourceType = ResourceType.Wood, density = 0.4f };
                    resourceSpawns[1] = new ResourceSpawnEntry { resourceType = ResourceType.Stone, density = 0.2f };
                    break;
                    
                case BiomeType.AridDesert:
                    floraSpawns[0] = new FloraSpawnEntry { floraType = FloraType.Cactus, density = 0.1f };
                    floraSpawns[1] = new FloraSpawnEntry { floraType = FloraType.DryShrub, density = 0.05f };
                    faunaSpawns[0] = new FaunaSpawnEntry { faunaType = FaunaType.DesertFox, density = 0.05f };
                    faunaSpawns[1] = new FaunaSpawnEntry { faunaType = FaunaType.Scorpion, density = 0.08f };
                    resourceSpawns[0] = new ResourceSpawnEntry { resourceType = ResourceType.Sand, density = 0.8f };
                    resourceSpawns[1] = new ResourceSpawnEntry { resourceType = ResourceType.IronOre, density = 0.1f };
                    break;
                    
                // Additional biome spawn tables would be populated here
            }
        }
        
        public float GetTemperature(float timeOfDay, float seasonModifier)
        {
            float dailyCycle = Mathf.Sin((timeOfDay - 6f) / 24f * Mathf.PI) * temperatureVariance * 0.5f;
            return baseTemperature + dailyCycle + seasonModifier;
        }
    }
    
    public enum BiomeType
    {
        TemperateForest,
        AridDesert,
        ArcticTundra,
        SwampMarsh,
        VolcanicWasteland,
        AnomalyZone
    }
    
    public enum SubBiomeType
    {
        Dense,
        Sparse,
        Mountainous,
        Coastal,
        Urban,
        RiverValley,
        Plateau
    }
    
    public enum MicroBiomeType
    {
        Clearing,
        Grove,
        Thicket,
        Cliffside,
        Cave,
        WaterEdge,
        RuinSite,
        AnomalyHotspot
    }
    
    public enum HazardType
    {
        None,
        Wildlife,
        HeatStroke,
        Hypothermia,
        Disease,
        Radiation,
        RealityDistortion,
        Hostiles,
        ToxicGas,
        Earthquakes
    }
    
    public enum AnomalyType
    {
        None,
        ThermalVent,
        RealityFlux,
        GravityWell,
        TimeDilation,
        PhaseShift,
        EnergyStorm,
        MatterScrambler
    }
    
    [Serializable]
    public struct FloraSpawnEntry
    {
        public FloraType floraType;
        public float density;          // 0-1 spawn chance
        public float minClusterSize;
        public float maxClusterSize;
        public float growthRate;       // days to mature
        public bool isSeasonal;
        public int seasonalMonth;      // 0-11
    }
    
    [Serializable]
    public struct FaunaSpawnEntry
    {
        public FaunaType faunaType;
        public float density;
        public int minGroupSize;
        public int maxGroupSize;
        public bool isNocturnal;
        public bool isAggressive;
        public float wanderRadius;
    }
    
    [Serializable]
    public struct ResourceSpawnEntry
    {
        public ResourceType resourceType;
        public float density;
        public float minVeinSize;
        public float maxVeinSize;
        public float depletionRate;    // How fast it depletes
        public float respawnDays;      // Days to respawn
    }
    
    public enum FloraType
    {
        OakTree, PineTree, BirchTree, WillowTree,
        BerryBush, MedicinalHerb, Cactus, DryShrub,
        TallGrass, Mushrooms, Algae, Kelp,
        AnomalyFlora, CrystalPlant, GlowMoss
    }
    
    public enum FaunaType
    {
        Deer, Boar, Rabbit, Wolf, Bear,
        DesertFox, Scorpion, Sandworm,
        ArcticHare, SnowOwl, Mammoth,
        SwampCrocodile, GiantFrog, MosquitoSwarm,
        MutatedRat, RadiationRoach, PhaseCat,
        VoidStalker, QuantumBird
    }
    
    public enum ResourceType
    {
        Wood, Stone, IronOre, CopperOre, Coal,
        Sand, Clay, OilShale, UraniumOre,
        FreshWater, SaltWater, Ice,
        MedicinalPlants, FoodCrops, FiberPlants,
        AnomalyShards, DataCores, PreCollapseTech
    }
}
