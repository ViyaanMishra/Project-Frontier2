using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Vehicle data structure containing chassis type, modules, fuel, health, and cargo.
    /// </summary>
    [Serializable]
    public struct VehicleData
    {
        public EntityGUID guid;
        public VehicleType chassisType;
        public string vehicleName;
        public int factionId;
        public int ownerId; // EntityGUID of owner
        
        // Core stats
        public float maxHealth;
        public float currentHealth;
        public float maxFuel;
        public float currentFuel;
        public float maxCargoCapacity;
        public float currentCargoWeight;
        
        // Performance
        public float maxSpeed;
        public float acceleration;
        public float handling;
        public float armorRating;
        
        // Modules (up to 8 slot configurations)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public VehicleModule[] installedModules;
        
        // Damage states per component
        public ComponentDamage engineDamage;
        public ComponentDamage transmissionDamage;
        public ComponentDamage hullDamage;
        public ComponentDamage[] wheelDamage; // 4 wheels
        public ComponentDamage turretDamage;
        
        // Status
        public VehicleState state;
        public bool isEngineRunning;
        public bool isHeadlightsOn;
        public int currentDriverId;
        public int[] passengerIds; // Up to 8 passengers
        
        // Fuel consumption (liters per km at max speed)
        public float fuelConsumptionRate;
        
        public void Initialize(VehicleType type, string name, int faction)
        {
            chassisType = type;
            vehicleName = name;
            factionId = faction;
            
            // Set base stats based on type
            SetBaseStatsForType(type);
            
            currentHealth = maxHealth;
            currentFuel = maxFuel * 0.5f; // Start half full
            isEngineRunning = false;
            state = VehicleState.Parked;
            installedModules = new VehicleModule[8];
            wheelDamage = new ComponentDamage[4];
            passengerIds = new int[8];
        }
        
        private void SetBaseStatsForType(VehicleType type)
        {
            switch (type)
            {
                case VehicleType.ScavengerQuad:
                    maxHealth = 150; maxFuel = 40; maxCargoCapacity = 200;
                    maxSpeed = 85; acceleration = 12; handling = 0.9f; armorRating = 0.3f;
                    fuelConsumptionRate = 0.15f;
                    break;
                case VehicleType.ArmoredRover:
                    maxHealth = 400; maxFuel = 80; maxCargoCapacity = 400;
                    maxSpeed = 55; acceleration = 6; handling = 0.6f; armorRating = 0.7f;
                    fuelConsumptionRate = 0.35f;
                    break;
                case VehicleType.LogisticsHauler:
                    maxHealth = 350; maxFuel = 150; maxCargoCapacity = 2000;
                    maxSpeed = 40; acceleration = 3; handling = 0.4f; armorRating = 0.5f;
                    fuelConsumptionRate = 0.6f;
                    break;
                case VehicleType.HoverSkiff:
                    maxHealth = 120; maxFuel = 50; maxCargoCapacity = 300;
                    maxSpeed = 95; acceleration = 15; handling = 0.85f; armorRating = 0.2f;
                    fuelConsumptionRate = 0.25f;
                    break;
                case VehicleType.WalkerMech:
                    maxHealth = 800; maxFuel = 200; maxCargoCapacity = 600;
                    maxSpeed = 30; acceleration = 4; handling = 0.5f; armorRating = 0.9f;
                    fuelConsumptionRate = 0.8f;
                    break;
                case VehicleType.ScoutBuggy:
                    maxHealth = 100; maxFuel = 35; maxCargoCapacity = 100;
                    maxSpeed = 110; acceleration = 18; handling = 0.95f; armorRating = 0.1f;
                    fuelConsumptionRate = 0.12f;
                    break;
                case VehicleType.MobileWorkshop:
                    maxHealth = 300; maxFuel = 100; maxCargoCapacity = 1200;
                    maxSpeed = 35; acceleration = 4; handling = 0.5f; armorRating = 0.6f;
                    fuelConsumptionRate = 0.45f;
                    break;
                case VehicleType.FuelTanker:
                    maxHealth = 250; maxFuel = 500; maxCargoCapacity = 5000;
                    maxSpeed = 45; acceleration = 3; handling = 0.35f; armorRating = 0.4f;
                    fuelConsumptionRate = 0.55f;
                    break;
                case VehicleType.AttackHelicopter:
                    maxHealth = 350; maxFuel = 300; maxCargoCapacity = 300;
                    maxSpeed = 180; acceleration = 20; handling = 0.75f; armorRating = 0.5f;
                    fuelConsumptionRate = 1.2f;
                    break;
                case VehicleType.CargoHelicopter:
                    maxHealth = 400; maxFuel = 400; maxCargoCapacity = 1500;
                    maxSpeed = 120; acceleration = 10; handling = 0.6f; armorRating = 0.4f;
                    fuelConsumptionRate = 1.5f;
                    break;
                case VehicleType.AmphibiousAPC:
                    maxHealth = 500; maxFuel = 120; maxCargoCapacity = 800;
                    maxSpeed = 65; acceleration = 7; handling = 0.65f; armorRating = 0.75f;
                    fuelConsumptionRate = 0.5f;
                    break;
                case VehicleType.AnomalySkimmer:
                    maxHealth = 280; maxFuel = 180; maxCargoCapacity = 400;
                    maxSpeed = 100; acceleration = 16; handling = 0.8f; armorRating = 0.6f;
                    fuelConsumptionRate = 0.7f;
                    break;
                default:
                    maxHealth = 200; maxFuel = 60; maxCargoCapacity = 300;
                    maxSpeed = 60; acceleration = 8; handling = 0.7f; armorRating = 0.5f;
                    fuelConsumptionRate = 0.3f;
                    break;
            }
        }
        
        public void ApplyDamage(float damage, DamageZone zone)
        {
            switch (zone)
            {
                case DamageZone.Engine:
                    engineDamage.currentHealth -= damage;
                    if (engineDamage.currentHealth <= 0) isEngineRunning = false;
                    break;
                case DamageZone.Hull:
                    currentHealth -= damage * (1 - armorRating);
                    break;
                case DamageZone.Turret:
                    turretDamage.currentHealth -= damage;
                    break;
                case DamageZone.Wheel:
                case DamageZone.Track:
                    for (int i = 0; i < wheelDamage.Length; i++)
                    {
                        if (wheelDamage[i].currentHealth > 0)
                        {
                            wheelDamage[i].currentHealth -= damage;
                            break;
                        }
                    }
                    break;
            }
            
            if (currentHealth <= 0)
            {
                state = VehicleState.Destroyed;
                // Chance to explode if fuel tank hit
                if (UnityEngine.Random.value < 0.3f && currentFuel > maxFuel * 0.3f)
                {
                    // Trigger explosion event
                }
            }
        }
        
        public float GetFuelEfficiency()
        {
            float efficiency = 1f;
            
            // Damaged engine reduces efficiency
            if (engineDamage.currentHealth < engineDamage.maxHealth * 0.5f)
                efficiency *= 0.7f;
            
            // Damaged wheels/tracks reduce efficiency
            int damagedWheels = 0;
            for (int i = 0; i < wheelDamage.Length; i++)
            {
                if (wheelDamage[i].currentHealth < wheelDamage[i].maxHealth * 0.5f)
                    damagedWheels++;
            }
            efficiency *= Mathf.Clamp(1f - (damagedWheels * 0.1f), 0.3f, 1f);
            
            // Cargo weight affects efficiency
            float loadRatio = currentCargoWeight / maxCargoCapacity;
            efficiency *= Mathf.Clamp(1f - (loadRatio * 0.3f), 0.5f, 1f);
            
            return efficiency;
        }
    }
    
    public enum VehicleType
    {
        ScavengerQuad,
        ArmoredRover,
        LogisticsHauler,
        HoverSkiff,
        WalkerMech,
        ScoutBuggy,
        MobileWorkshop,
        FuelTanker,
        ArmoredTrain,
        AttackHelicopter,
        CargoHelicopter,
        AmphibiousAPC,
        AnomalySkimmer
    }
    
    public enum VehicleState { Parked, Driving, Idling, Broken, Destroyed, InCombat }
    
    public enum DamageZone { Engine, Hull, Turret, Wheel, Track, Transmission, FuelTank }
    
    [Serializable]
    public struct VehicleModule
    {
        public ModuleType moduleType;
        public int level;
        public bool isActive;
        public float powerDraw;
        public float bonusValue;
        
        public static VehicleModule Create(ModuleType type, int lvl = 1)
        {
            return new VehicleModule
            {
                moduleType = type,
                level = lvl,
                isActive = true,
                powerDraw = GetPowerDraw(type, lvl),
                bonusValue = GetBonusValue(type, lvl)
            };
        }
        
        private static float GetPowerDraw(ModuleType type, int level)
        {
            return type switch
            {
                ModuleType.ArmorPlating => 0,
                ModuleType.CargoRack => 0,
                ModuleType.TurretMount => 5,
                ModuleType.ShieldGenerator => 25,
                ModuleType.ECMJammer => 15,
                ModuleType.NightVision => 3,
                ModuleType.ReinforcedSuspension => 0,
                ModuleType.TurboEngine => 10,
                _ => 0
            } * (1 + level * 0.2f);
        }
        
        private static float GetBonusValue(ModuleType type, int level)
        {
            return type switch
            {
                ModuleType.ArmorPlating => level * 0.1f, // +10% armor per level
                ModuleType.CargoRack => level * 50, // +50 capacity per level
                ModuleType.TurretMount => level * 0.15f, // +15% damage
                ModuleType.ShieldGenerator => level * 25, // shield HP
                ModuleType.ECMJammer => level * 0.1f, // jam radius
                ModuleType.NightVision => 0,
                ModuleType.ReinforcedSuspension => level * 0.05f, // handling
                ModuleType.TurboEngine => level * 0.08f, // speed
                _ => 0
            };
        }
    }
    
    public enum ModuleType
    {
        ArmorPlating,
        CargoRack,
        TurretMount,
        ShieldGenerator,
        ECMJammer,
        NightVision,
        ReinforcedSuspension,
        TurboEngine
    }
    
    [Serializable]
    public struct ComponentDamage
    {
        public float maxHealth;
        public float currentHealth;
        public bool isCritical;
        
        public ComponentDamage(float max)
        {
            maxHealth = max;
            currentHealth = max;
            isCritical = false;
        }
    }
}
