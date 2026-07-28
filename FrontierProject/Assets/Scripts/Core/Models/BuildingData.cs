using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Building data structure with 5 tiers, structural integrity, and power draw.
    /// </summary>
    [Serializable]
    public struct BuildingData
    {
        public EntityGUID guid;
        public BuildingType buildingType;
        public BuildingTier tier;
        public int factionId;
        public int ownerId;
        
        // Position and orientation
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        
        // Construction state
        public ConstructionStage constructionStage;
        public float constructionProgress; // 0-1
        public int[] requiredMaterials;    // Indexed by ResourceType
        public int[] consumedMaterials;
        
        // Structural properties
        public float maxStructuralIntegrity;
        public float currentStructuralIntegrity;
        public float loadBearingCapacity;
        public bool isLoadBearing;
        public int[] connectedSupports; // EntityGUIDs of connected structures
        
        // Power system
        public float powerDraw;
        public float maxPowerInput;
        public bool isPowered;
        public int powerConnectionId;
        
        // Health and damage
        public float maxHealth;
        public float currentHealth;
        public float armorRating;
        public DamageFlags damageFlags;
        
        // Functional properties
        public int storageSlots;
        public int currentStorage;
        public int workerCapacity;
        public int currentWorkers;
        public float productionEfficiency;
        
        // Environmental effects
        public float interiorTemperature;
        public float airQuality;
        public float lightLevel;
        public bool isEnclosed;
        public bool hasClimateControl;
        
        public void Initialize(BuildingType type, BuildingTier t, Vector3 pos, Quaternion rot)
        {
            buildingType = type;
            tier = t;
            position = pos;
            rotation = rot;
            scale = GetBaseScaleForType(type);
            
            SetPropertiesForTier();
            
            constructionStage = ConstructionStage.Foundation;
            constructionProgress = 0f;
            currentStructuralIntegrity = maxStructuralIntegrity;
            currentHealth = maxHealth;
            isPowered = false;
            productionEfficiency = 1f;
        }
        
        private Vector3 GetBaseScaleForType(BuildingType type)
        {
            return type switch
            {
                BuildingType.WoodenShack => new Vector3(4, 3, 4),
                BuildingType.ConcreteHouse => new Vector3(6, 4, 6),
                BuildingType.SteelWarehouse => new Vector3(12, 6, 8),
                BuildingType.ReinforcedBunker => new Vector3(10, 5, 10),
                BuildingType.AdvancedLab => new Vector3(15, 8, 12),
                BuildingType.Wall => new Vector3(4, 3, 0.3f),
                BuildingType.Gate => new Vector3(4, 3.5f, 0.5f),
                BuildingType.Tower => new Vector3(4, 12, 4),
                BuildingType.SolarArray => new Vector3(8, 2, 4),
                BuildingType.WindTurbine => new Vector3(3, 15, 3),
                BuildingType.WaterTower => new Vector3(5, 10, 5),
                BuildingType.FarmPlot => new Vector3(6, 0.5f, 6),
                BuildingType.Greenhouse => new Vector3(10, 5, 6),
                BuildingType.Workshop => new Vector3(8, 5, 6),
                BuildingType.Hospital => new Vector3(15, 6, 10),
                _ => new Vector3(4, 3, 4)
            };
        }
        
        private void SetPropertiesForTier()
        {
            float tierMultiplier = (float)tier;
            
            switch (buildingType)
            {
                case BuildingType.WoodenShack:
                    maxStructuralIntegrity = 100 * tierMultiplier;
                    maxHealth = 150 * tierMultiplier;
                    armorRating = 0.1f;
                    powerDraw = 0.5f;
                    storageSlots = 10;
                    workerCapacity = 2;
                    break;
                    
                case BuildingType.ConcreteHouse:
                    maxStructuralIntegrity = 300 * tierMultiplier;
                    maxHealth = 400 * tierMultiplier;
                    armorRating = 0.3f;
                    powerDraw = 2f;
                    storageSlots = 30;
                    workerCapacity = 4;
                    break;
                    
                case BuildingType.SteelWarehouse:
                    maxStructuralIntegrity = 500 * tierMultiplier;
                    maxHealth = 600 * tierMultiplier;
                    armorRating = 0.5f;
                    powerDraw = 5f;
                    storageSlots = 100;
                    workerCapacity = 8;
                    break;
                    
                case BuildingType.ReinforcedBunker:
                    maxStructuralIntegrity = 1000 * tierMultiplier;
                    maxHealth = 1200 * tierMultiplier;
                    armorRating = 0.8f;
                    powerDraw = 10f;
                    storageSlots = 50;
                    workerCapacity = 10;
                    break;
                    
                case BuildingType.AdvancedLab:
                    maxStructuralIntegrity = 800 * tierMultiplier;
                    maxHealth = 700 * tierMultiplier;
                    armorRating = 0.6f;
                    powerDraw = 50f;
                    storageSlots = 40;
                    workerCapacity = 15;
                    productionEfficiency = 1.5f;
                    break;
                    
                case BuildingType.Wall:
                    maxStructuralIntegrity = 200 * tierMultiplier;
                    maxHealth = 250 * tierMultiplier;
                    armorRating = 0.4f;
                    powerDraw = 0f;
                    isLoadBearing = true;
                    break;
                    
                case BuildingType.Tower:
                    maxStructuralIntegrity = 400 * tierMultiplier;
                    maxHealth = 350 * tierMultiplier;
                    armorRating = 0.3f;
                    powerDraw = 3f;
                    isLoadBearing = true;
                    break;
            }
            
            // Tier bonuses
            if (tier >= BuildingTier.Tier3)
            {
                hasClimateControl = true;
                productionEfficiency *= 1.2f;
            }
            if (tier >= BuildingTier.Tier4)
            {
                armorRating = Mathf.Min(armorRating + 0.2f, 0.95f);
                productionEfficiency *= 1.3f;
            }
        }
        
        public void ApplyDamage(float damage, DamageType damageType)
        {
            float effectiveDamage = damage * (1 - armorRating);
            
            // Structural damage affects integrity
            if (damageType == DamageType.Explosive || damageType == DamageType.Impact)
            {
                currentStructuralIntegrity -= effectiveDamage * 1.5f;
                damageFlags |= DamageFlags.StructuralCompromised;
            }
            
            // Fire damage over time
            if (damageType == DamageType.Fire)
            {
                damageFlags |= DamageFlags.OnFire;
                effectiveDamage *= 1.3f;
            }
            
            currentHealth -= effectiveDamage;
            
            if (currentHealth <= 0)
            {
                damageFlags |= DamageFlags.Destroyed;
                if (isLoadBearing)
                {
                    damageFlags |= DamageFlags.CollapseImminent;
                }
            }
            
            if (currentStructuralIntegrity <= 0)
            {
                damageFlags |= DamageFlags.Collapsed;
            }
        }
        
        public void UpdateConstruction(float deltaTime, int[] materialsProvided)
        {
            if (constructionStage == ConstructionStage.Complete) return;
            
            // Check if all materials consumed
            bool allConsumed = true;
            for (int i = 0; i < requiredMaterials.Length; i++)
            {
                if (consumedMaterials[i] < requiredMaterials[i])
                {
                    allConsumed = false;
                    // Consume provided materials
                    consumedMaterials[i] = Mathf.Min(consumedMaterials[i] + materialsProvided[i], requiredMaterials[i]);
                }
            }
            
            if (allConsumed)
            {
                constructionProgress += deltaTime * GetConstructionSpeed();
                if (constructionProgress >= 1f)
                {
                    AdvanceConstructionStage();
                }
            }
        }
        
        private float GetConstructionSpeed()
        {
            float baseSpeed = 0.1f; // Complete in 10 seconds if materials available
            
            if (tier >= BuildingTier.Tier2) baseSpeed *= 1.2f;
            if (tier >= BuildingTier.Tier3) baseSpeed *= 1.3f;
            if (tier >= BuildingTier.Tier4) baseSpeed *= 1.5f;
            
            return baseSpeed;
        }
        
        private void AdvanceConstructionStage()
        {
            constructionProgress = 0f;
            constructionStage++;
            
            if (constructionStage > ConstructionStage.Finishes)
            {
                constructionStage = ConstructionStage.Complete;
            }
        }
    }
    
    public enum BuildingType
    {
        WoodenShack, ConcreteHouse, SteelWarehouse, ReinforcedBunker, AdvancedLab,
        Wall, Gate, Tower, Watchtower,
        SolarArray, WindTurbine, GeneratorBuilding, BatteryStorage,
        WaterTower, PumpStation, WaterPurifier,
        FarmPlot, Greenhouse, Silo, Barn,
        Workshop, Fabricator, Smelter, Refinery,
        Hospital, Clinic, QuarantineZone,
        Barracks, CommandCenter, CommunicationsArray,
        RoadSegment, Bridge, LandingPad,
        ConveyorHub, DronePort, TrainStation
    }
    
    public enum BuildingTier
    {
        Tier1 = 1,  // Primitive (wood, scrap)
        Tier2 = 2,  // Basic (concrete, steel)
        Tier3 = 3,  // Advanced (composites, electronics)
        Tier4 = 4,  // High-tech (nanomaterials, AI)
        Tier5 = 5   // Anomaly tech (reality-stabilized)
    }
    
    public enum ConstructionStage
    {
        NotStarted,
        Foundation,
        Framework,
        Walls,
        Roof,
        Interior,
        Finishes,
        Complete
    }
    
    [Flags]
    public enum DamageFlags : ulong
    {
        None = 0,
        OnFire = 1UL << 0,
        Flooded = 1UL << 1,
        StructuralCompromised = 1UL << 2,
        PowerDisconnected = 1UL << 3,
        Contaminated = 1UL << 4,
        Breached = 1UL << 5,
        CollapseImminent = 1UL << 6,
        Collapsed = 1UL << 7,
        Destroyed = 1UL << 8
    }
    
    public enum DamageType
    {
        Impact,
        Piercing,
        Explosive,
        Fire,
        Energy,
        Corrosive,
        Radiation,
        Anomaly
    }
}
