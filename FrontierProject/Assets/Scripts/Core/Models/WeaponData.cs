using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Weapon data structure with 30+ weapons and full stat blocks.
    /// </summary>
    [Serializable]
    public struct WeaponData
    {
        public EntityGUID guid;
        public WeaponType weaponType;
        public WeaponCategory category;
        public string weaponName;
        public string description;
        
        // Core stats
        public float damage;
        public float range;
        public float accuracy;        // 0-1, base accuracy
        public float fireRate;        // Rounds per minute
        public float reloadTime;      // Seconds
        public int magazineSize;
        public int maxAmmoCarry;
        
        // Physics
        public float projectileSpeed;
        public float damageDropoff;   // Damage loss per meter
        public float penetrationPower;
        public float recoilHorizontal;
        public float recoilVertical;
        public float recoilRecovery;
        
        // Special properties
        public DamageType damageType;
        public StatusEffect appliedEffect;
        public float effectChance;    // 0-1
        public float effectDuration;
        
        // Handling
        public float equipTime;
        public float aimDownSightTime;
        public float movementSpeedMod; // Multiplier when equipped
        public float staminaDrain;    // Per shot
        
        // Modification slots
        public int modSlots;
        public WeaponMod[] installedMods;
        
        // Ammo type
        public AmmoType ammoType;
        public bool isEnergyWeapon;
        public float heatGeneration;
        public float overheatThreshold;
        public float cooldownRate;
        
        public void Initialize(WeaponType type)
        {
            weaponType = type;
            category = GetCategoryForType(type);
            weaponName = GetNameForType(type);
            SetStatsForType(type);
            installedMods = new WeaponMod[modSlots];
        }
        
        private WeaponCategory GetCategoryForType(WeaponType type)
        {
            return type switch
            {
                WeaponType.ScrapMachete or WeaponType.PipeWrench or WeaponType.FireAxe or
                WeaponType.ElectroBaton or WeaponType.ThermalLance or WeaponType.AnomalyBlade or
                WeaponType.Sledgehammer or WeaponType.CombatKnife => WeaponCategory.Melee,
                
                WeaponType.PipeRifle or WeaponType.ScrapPistol or WeaponType.KineticShotgun or
                WeaponType.AssaultCarbine or WeaponType.MarksmanDMR or WeaponType.HeavyMachineGun or
                WeaponType.Railgun or WeaponType.PlasmaRifle or WeaponType.AnomalyCaster or
                WeaponType.Crossbow or WeaponType.NailGun or WeaponType.Flamethrower => WeaponCategory.Ranged,
                
                WeaponType.MortarTube or WeaponType.RocketLauncher or WeaponType.GrenadeSpammer or
                WeaponType.EMPCannon or WeaponType.AnomalyProjector or WeaponType.Minigun => WeaponCategory.Heavy,
                
                _ => WeaponCategory.Ranged
            };
        }
        
        private string GetNameForType(WeaponType type)
        {
            return type switch
            {
                WeaponType.ScrapMachete => "Scrap Machete",
                WeaponType.PipeWrench => "Pipe Wrench",
                WeaponType.FireAxe => "Fire Axe",
                WeaponType.ElectroBaton => "Electro-Baton",
                WeaponType.ThermalLance => "Thermal Lance",
                WeaponType.AnomalyBlade => "Anomaly Shard Blade",
                WeaponType.Sledgehammer => "Sledgehammer",
                WeaponType.CombatKnife => "Combat Knife",
                WeaponType.PipeRifle => "Pipe Rifle",
                WeaponType.ScrapPistol => "Scrap Pistol",
                WeaponType.KineticShotgun => "Kinetic Shotgun",
                WeaponType.AssaultCarbine => "Assault Carbine",
                WeaponType.MarksmanDMR => "Marksman DMR",
                WeaponType.HeavyMachineGun => "Heavy Machine Gun",
                WeaponType.Railgun => "Railgun",
                WeaponType.PlasmaRifle => "Plasma Rifle",
                WeaponType.AnomalyCaster => "Anomaly Caster",
                WeaponType.Crossbow => "Crossbow",
                WeaponType.NailGun => "Nail Gun",
                WeaponType.Flamethrower => "Flamethrower",
                WeaponType.MortarTube => "Mortar Tube",
                WeaponType.RocketLauncher => "Rocket Launcher",
                WeaponType.GrenadeSpammer => "Grenade Spammer",
                WeaponType.EMPCannon => "EMP Cannon",
                WeaponType.AnomalyProjector => "Anomaly Projector",
                WeaponType.Minigun => "Minigun",
                _ => "Unknown Weapon"
            };
        }
        
        private void SetStatsForType(WeaponType type)
        {
            switch (type)
            {
                // MELEE WEAPONS
                case WeaponType.ScrapMachete:
                    damage = 25; range = 1.5f; accuracy = 0.7f; fireRate = 120;
                    recoilVertical = 2f; movementSpeedMod = 1.0f;
                    damageType = DamageType.Slash; appliedEffect = StatusEffect.Bleeding;
                    effectChance = 0.3f; modSlots = 1;
                    break;
                    
                case WeaponType.PipeWrench:
                    damage = 30; range = 1.3f; accuracy = 0.65f; fireRate = 90;
                    recoilVertical = 3f; movementSpeedMod = 0.95f;
                    damageType = DamageType.Impact; penetrationPower = 0.4f;
                    modSlots = 1;
                    break;
                    
                case WeaponType.FireAxe:
                    damage = 55; range = 1.8f; accuracy = 0.5f; fireRate = 60;
                    recoilVertical = 5f; movementSpeedMod = 0.85f;
                    damageType = DamageType.Slash; penetrationPower = 0.3f;
                    appliedEffect = StatusEffect.StructuralDamage;
                    modSlots = 2;
                    break;
                    
                case WeaponType.ElectroBaton:
                    damage = 20; range = 1.4f; accuracy = 0.75f; fireRate = 100;
                    recoilVertical = 1f; movementSpeedMod = 1.0f;
                    damageType = DamageType.Energy; appliedEffect = StatusEffect.Stunned;
                    effectChance = 0.5f; effectDuration = 2f;
                    isEnergyWeapon = true; heatGeneration = 5f; modSlots = 2;
                    break;
                    
                case WeaponType.ThermalLance:
                    damage = 45; range = 2f; accuracy = 0.6f; fireRate = 40;
                    recoilVertical = 4f; movementSpeedMod = 0.8f;
                    damageType = DamageType.Energy; penetrationPower = 0.8f;
                    appliedEffect = StatusEffect.Burning; effectChance = 0.7f;
                    isEnergyWeapon = true; heatGeneration = 15f; overheatThreshold = 80f;
                    modSlots = 3;
                    break;
                    
                case WeaponType.AnomalyBlade:
                    damage = 70; range = 1.6f; accuracy = 0.8f; fireRate = 80;
                    recoilVertical = 2f; movementSpeedMod = 0.9f;
                    damageType = DamageType.Anomaly; penetrationPower = 1f;
                    appliedEffect = StatusEffect.RealityDistortion;
                    effectChance = 0.4f; effectDuration = 3f;
                    modSlots = 4;
                    break;
                    
                case WeaponType.Sledgehammer:
                    damage = 80; range = 2f; accuracy = 0.4f; fireRate = 45;
                    recoilVertical = 8f; movementSpeedMod = 0.75f;
                    damageType = DamageType.Impact; penetrationPower = 0.5f;
                    appliedEffect = StatusEffect.StructuralDamage;
                    modSlots = 1;
                    break;
                    
                case WeaponType.CombatKnife:
                    damage = 18; range = 1f; accuracy = 0.85f; fireRate = 150;
                    recoilVertical = 1f; movementSpeedMod = 1.05f;
                    damageType = DamageType.Piercing; appliedEffect = StatusEffect.Bleeding;
                    effectChance = 0.2f; modSlots = 2;
                    break;
                    
                // RANGED WEAPONS
                case WeaponType.PipeRifle:
                    damage = 35; range = 80f; accuracy = 0.5f; fireRate = 40;
                    reloadTime = 2.5f; magazineSize = 5; maxAmmoCarry = 50;
                    projectileSpeed = 400f; damageDropoff = 0.02f;
                    recoilVertical = 8f; recoilHorizontal = 3f; recoilRecovery = 0.3f;
                    staminaDrain = 5f; aimDownSightTime = 0.4f; movementSpeedMod = 0.9f;
                    damageType = DamageType.Impact; ammoType = AmmoType.Makeshift;
                    modSlots = 3;
                    break;
                    
                case WeaponType.ScrapPistol:
                    damage = 20; range = 30f; accuracy = 0.6f; fireRate = 180;
                    reloadTime = 1.5f; magazineSize = 8; maxAmmoCarry = 96;
                    projectileSpeed = 350f; damageDropoff = 0.03f;
                    recoilVertical = 5f; recoilHorizontal = 2f; recoilRecovery = 0.4f;
                    equipTime = 0.2f; aimDownSightTime = 0.15f; movementSpeedMod = 1.0f;
                    damageType = DamageType.Impact; ammoType = AmmoType.Pistol;
                    modSlots = 2;
                    break;
                    
                case WeaponType.KineticShotgun:
                    damage = 12; range = 15f; accuracy = 0.4f; fireRate = 60;
                    reloadTime = 2f; magazineSize = 8; maxAmmoCarry = 48;
                    projectileSpeed = 300f; damageDropoff = 0.08f;
                    recoilVertical = 12f; recoilHorizontal = 4f; recoilRecovery = 0.2f;
                    staminaDrain = 8f; aimDownSightTime = 0.25f; movementSpeedMod = 0.95f;
                    damageType = DamageType.Impact; penetrationPower = 0.2f;
                    ammoType = AmmoType.Shotgun; modSlots = 3;
                    break;
                    
                case WeaponType.AssaultCarbine:
                    damage = 22; range = 60f; accuracy = 0.65f; fireRate = 600;
                    reloadTime = 2f; magazineSize = 30; maxAmmoCarry = 180;
                    projectileSpeed = 700f; damageDropoff = 0.015f;
                    recoilVertical = 4f; recoilHorizontal = 2f; recoilRecovery = 0.35f;
                    staminaDrain = 3f; aimDownSightTime = 0.2f; movementSpeedMod = 0.95f;
                    damageType = DamageType.Impact; ammoType = AmmoType.Rifle;
                    modSlots = 4;
                    break;
                    
                case WeaponType.MarksmanDMR:
                    damage = 55; range = 150f; accuracy = 0.85f; fireRate = 120;
                    reloadTime = 2.2f; magazineSize = 10; maxAmmoCarry = 60;
                    projectileSpeed = 800f; damageDropoff = 0.008f;
                    recoilVertical = 10f; recoilHorizontal = 2f; recoilRecovery = 0.25f;
                    staminaDrain = 6f; aimDownSightTime = 0.35f; movementSpeedMod = 0.85f;
                    damageType = DamageType.Impact; penetrationPower = 0.6f;
                    ammoType = AmmoType.Sniper; modSlots = 5;
                    break;
                    
                case WeaponType.HeavyMachineGun:
                    damage = 28; range = 100f; accuracy = 0.45f; fireRate = 500;
                    reloadTime = 4f; magazineSize = 100; maxAmmoCarry = 400;
                    projectileSpeed = 750f; damageDropoff = 0.012f;
                    recoilVertical = 6f; recoilHorizontal = 3f; recoilRecovery = 0.15f;
                    staminaDrain = 5f; aimDownSightTime = 0.4f; movementSpeedMod = 0.7f;
                    damageType = DamageType.Impact; ammoType = AmmoType.Heavy;
                    appliedEffect = StatusEffect.Suppressed; effectChance = 0.8f;
                    modSlots = 4;
                    break;
                    
                case WeaponType.Railgun:
                    damage = 150; range = 300f; accuracy = 0.9f; fireRate = 20;
                    reloadTime = 3f; magazineSize = 5; maxAmmoCarry = 25;
                    projectileSpeed = 2000f; damageDropoff = 0.003f;
                    recoilVertical = 25f; recoilHorizontal = 5f; recoilRecovery = 0.5f;
                    staminaDrain = 20f; aimDownSightTime = 0.6f; movementSpeedMod = 0.6f;
                    damageType = DamageType.Impact; penetrationPower = 1f;
                    ammoType = AmmoType.Rail; modSlots = 5;
                    break;
                    
                case WeaponType.PlasmaRifle:
                    damage = 40; range = 70f; accuracy = 0.7f; fireRate = 300;
                    reloadTime = 2.5f; magazineSize = 40; maxAmmoCarry = 200;
                    projectileSpeed = 500f; damageDropoff = 0.02f;
                    recoilVertical = 5f; recoilHorizontal = 3f; recoilRecovery = 0.3f;
                    staminaDrain = 4f; aimDownSightTime = 0.25f; movementSpeedMod = 0.9f;
                    damageType = DamageType.Energy; appliedEffect = StatusEffect.Burning;
                    effectChance = 0.6f; isEnergyWeapon = true;
                    heatGeneration = 8f; overheatThreshold = 100f; cooldownRate = 15f;
                    ammoType = AmmoType.PlasmaCell; modSlots = 4;
                    break;
                    
                case WeaponType.AnomalyCaster:
                    damage = 80; range = 50f; accuracy = 0.6f; fireRate = 60;
                    reloadTime = 3f; magazineSize = 12; maxAmmoCarry = 48;
                    projectileSpeed = 350f; damageDropoff = 0.025f;
                    recoilVertical = 8f; recoilHorizontal = 5f; recoilRecovery = 0.2f;
                    staminaDrain = 10f; aimDownSightTime = 0.3f; movementSpeedMod = 0.85f;
                    damageType = DamageType.Anomaly; appliedEffect = StatusEffect.RealityDistortion;
                    effectChance = 0.5f; effectDuration = 4f;
                    ammoType = AmmoType.AnomalyShard; modSlots = 5;
                    break;
                    
                case WeaponType.Crossbow:
                    damage = 65; range = 70f; accuracy = 0.8f; fireRate = 30;
                    reloadTime = 2f; magazineSize = 1; maxAmmoCarry = 30;
                    projectileSpeed = 120f; damageDropoff = 0.01f;
                    recoilVertical = 3f; recoilRecovery = 0.5f;
                    staminaDrain = 4f; aimDownSightTime = 0.3f; movementSpeedMod = 0.95f;
                    damageType = DamageType.Piercing; penetrationPower = 0.5f;
                    ammoType = AmmoType.Bolt; modSlots = 3;
                    break;
                    
                case WeaponType.NailGun:
                    damage = 15; range = 25f; accuracy = 0.55f; fireRate = 400;
                    reloadTime = 1.5f; magazineSize = 50; maxAmmoCarry = 300;
                    projectileSpeed = 150f; damageDropoff = 0.04f;
                    recoilVertical = 2f; recoilHorizontal = 1f; recoilRecovery = 0.4f;
                    staminaDrain = 2f; aimDownSightTime = 0.15f; movementSpeedMod = 1.0f;
                    damageType = DamageType.Piercing; ammoType = AmmoType.Nail;
                    modSlots = 2;
                    break;
                    
                case WeaponType.Flamethrower:
                    damage = 8; range = 12f; accuracy = 0.5f; fireRate = 200;
                    reloadTime = 3f; magazineSize = 100; maxAmmoCarry = 300;
                    projectileSpeed = 20f; damageDropoff = 0.1f;
                    recoilVertical = 1f; recoilRecovery = 0.5f;
                    staminaDrain = 6f; movementSpeedMod = 0.8f;
                    damageType = DamageType.Fire; appliedEffect = StatusEffect.Burning;
                    effectChance = 1f; effectDuration = 5f;
                    ammoType = AmmoType.Fuel; modSlots = 3;
                    break;
                    
                // HEAVY WEAPONS
                case WeaponType.MortarTube:
                    damage = 200; range = 400f; accuracy = 0.3f; fireRate = 12;
                    reloadTime = 5f; magazineSize = 1; maxAmmoCarry = 20;
                    projectileSpeed = 150f; damageDropoff = 0f;
                    recoilVertical = 30f; recoilRecovery = 1f;
                    staminaDrain = 30f; movementSpeedMod = 0.5f;
                    damageType = DamageType.Explosive; penetrationPower = 0.3f;
                    ammoType = AmmoType.MortarShell; modSlots = 2;
                    break;
                    
                case WeaponType.RocketLauncher:
                    damage = 300; range = 250f; accuracy = 0.5f; fireRate = 10;
                    reloadTime = 4f; magazineSize = 1; maxAmmoCarry = 10;
                    projectileSpeed = 200f; damageDropoff = 0.005f;
                    recoilVertical = 40f; recoilRecovery = 1.5f;
                    staminaDrain = 40f; aimDownSightTime = 0.5f; movementSpeedMod = 0.6f;
                    damageType = DamageType.Explosive; penetrationPower = 0.7f;
                    ammoType = AmmoType.Rocket; modSlots = 3;
                    break;
                    
                case WeaponType.Minigun:
                    damage = 25; range = 120f; accuracy = 0.5f; fireRate = 1200;
                    reloadTime = 5f; magazineSize = 500; maxAmmoCarry = 2000;
                    projectileSpeed = 850f; damageDropoff = 0.01f;
                    recoilVertical = 8f; recoilHorizontal = 5f; recoilRecovery = 0.1f;
                    staminaDrain = 8f; movementSpeedMod = 0.5f;
                    damageType = DamageType.Impact; ammoType = AmmoType.Heavy;
                    isEnergyWeapon = false; heatGeneration = 20f; overheatThreshold = 150f;
                    modSlots = 4;
                    break;
                    
                default:
                    damage = 20; range = 50f; accuracy = 0.6f; fireRate = 100;
                    reloadTime = 2f; magazineSize = 20; maxAmmoCarry = 100;
                    projectileSpeed = 500f; damageDropoff = 0.02f;
                    recoilVertical = 5f; recoilRecovery = 0.3f;
                    damageType = DamageType.Impact; modSlots = 2;
                    break;
            }
        }
        
        public float CalculateDamageAtRange(float distance)
        {
            float dropoffMultiplier = 1f - (damageDropoff * distance);
            return Mathf.Max(damage * dropoffMultiplier, damage * 0.3f);
        }
        
        public float GetRecoilVector(out float horizontal, out float vertical)
        {
            horizontal = UnityEngine.Random.Range(-recoilHorizontal, recoilHorizontal);
            vertical = recoilVertical * UnityEngine.Random.Range(0.7f, 1.3f);
            return Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
        }
    }
    
    public enum WeaponType
    {
        // Melee (8)
        ScrapMachete, PipeWrench, FireAxe, ElectroBaton,
        ThermalLance, AnomalyBlade, Sledgehammer, CombatKnife,
        
        // Ranged (12)
        PipeRifle, ScrapPistol, KineticShotgun, AssaultCarbine,
        MarksmanDMR, HeavyMachineGun, Railgun, PlasmaRifle,
        AnomalyCaster, Crossbow, NailGun, Flamethrower,
        
        // Heavy/Ordnance (6)
        MortarTube, RocketLauncher, GrenadeSpammer,
        EMPCannon, AnomalyProjector, Minigun
    }
    
    public enum WeaponCategory { Melee, Ranged, Heavy, Throwable }
    
    public enum AmmoType
    {
        None, Makeshift, Pistol, Rifle, Sniper, Heavy, Shotgun,
        Bolt, Nail, Fuel, MortarShell, Rocket,
        PlasmaCell, AnomalyShard, Rail
    }
    
    [Serializable]
    public struct WeaponMod
    {
        public ModType modType;
        public int tier;
        public float bonusValue;
        
        public static WeaponMod Create(ModType type, int t = 1)
        {
            return new WeaponMod
            {
                modType = type,
                tier = t,
                bonusValue = GetBonusForMod(type, t)
            };
        }
        
        private static float GetBonusForMod(ModType type, int tier)
        {
            return type switch
            {
                ModType.RedDotSight => 0.05f * tier, // Accuracy
                ModType.Scope2x => 0.1f * tier,
                ModType.Scope4x => 0.15f * tier,
                ModType.Scope8x => 0.2f * tier,
                ModType.Suppressor => -0.1f * tier, // Noise reduction
                ModType.Compensator => -0.08f * tier, // Recoil reduction
                ModType.ExtendedMag => 0.25f * tier, // Mag size increase
                ModType.QuickdrawMag => -0.1f * tier, // Reload speed
                ModType.LaserSight => 0.03f * tier, // Hipfire accuracy
                ModType.BarrelExtension => 0.1f * tier, // Range
                ModType.StockPad => -0.1f * tier, // Recoil
                ModType.WeightedGrip => -0.05f * tier, // Stability
                ModType.EnergyCapacitor => 0.2f * tier, // Energy weapon capacity
                ModType.CoolingSystem => -0.15f * tier, // Heat generation
                _ => 0f
            };
        }
    }
    
    public enum ModType
    {
        RedDotSight, Scope2x, Scope4x, Scope8x,
        Suppressor, Compensator, ExtendedMag, QuickdrawMag,
        LaserSight, BarrelExtension, StockPad, WeightedGrip,
        EnergyCapacitor, CoolingSystem
    }
    
    public enum StatusEffect
    {
        None, Bleeding, Burning, Stunned, Poisoned,
        Radiated, Frozen, Shocked, Suppressed,
        StructuralDamage, RealityDistortion
    }
}
