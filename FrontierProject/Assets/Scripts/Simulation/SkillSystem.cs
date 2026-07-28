using System;
using Unity.Collections;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// Skill and progression system with 16 skill trees.
    /// Each skill has 10 levels, XP curves, and perk unlocks.
    /// </summary>
    public class SkillSystem : IDisposable
    {
        public enum SkillType
        {
            // Combat Skills
            Melee,
            Ranged,
            HeavyWeapons,
            Explosives,
            
            // Technical Skills
            Construction,
            Engineering,
            Electronics,
            
            // Science Skills
            Medicine,
            Chemistry,
            Botany,
            
            // Vehicle Skills
            Driving,
            Piloting,
            Navigation,
            
            // Social Skills
            Leadership,
            Trading,
            Espionage,
            
            Count
        }
        
        [Serializable]
        public struct SkillLevel
        {
            public int Level;              // 1-10
            public float CurrentXP;
            public float RequiredXP;
            public bool IsMaxed;
        }
        
        [Serializable]
        public struct SkillData
        {
            public SkillType Type;
            public SkillLevel Level;
            public float TotalXPEarned;
            public int PerksUnlocked;
            public ulong PerkBitmask;      // Bitmask of unlocked perks (10 bits)
        }
        
        [Serializable]
        public struct EntitySkills
        {
            public ulong EntityID;
            public SkillData Melee;
            public SkillData Ranged;
            public SkillData HeavyWeapons;
            public SkillData Explosives;
            public SkillData Construction;
            public SkillData Engineering;
            public SkillData Electronics;
            public SkillData Medicine;
            public SkillData Chemistry;
            public SkillData Botany;
            public SkillData Driving;
            public SkillData Piloting;
            public SkillData Navigation;
            public SkillData Leadership;
            public SkillData Trading;
            public SkillData Espionage;
        }
        
        private NativeHashMap<ulong, EntitySkills> _entitySkills;
        private int _capacity;
        
        // XP curve: Base * (Multiplier ^ Level)
        private const float XPBase = 100f;
        private const float XPMultiplier = 1.5f;
        
        public int TrackedEntityCount => _entitySkills.Count();
        
        public SkillSystem(int capacity = 10000)
        {
            _capacity = capacity;
            _entitySkills = new NativeHashMap<ulong, EntitySkills>(capacity, Allocator.Persistent);
        }
        
        public void RegisterEntity(ulong entityId)
        {
            if (_entitySkills.ContainsKey(entityId))
                return;
            
            if (_entitySkills.Count() >= _capacity)
            {
                Debug.LogWarning($"SkillSystem: Capacity ({_capacity}) exceeded!");
                return;
            }
            
            var skills = CreateDefaultSkills(entityId);
            _entitySkills[entityId] = skills;
        }
        
        public void UnregisterEntity(ulong entityId)
        {
            _entitySkills.Remove(entityId);
        }
        
        private EntitySkills CreateDefaultSkills(ulong entityId)
        {
            var skills = new EntitySkills { EntityID = entityId };
            
            // Initialize all skills to level 1
            skills.Melee = CreateSkillData(SkillType.Melee);
            skills.Ranged = CreateSkillData(SkillType.Ranged);
            skills.HeavyWeapons = CreateSkillData(SkillType.HeavyWeapons);
            skills.Explosives = CreateSkillData(SkillType.Explosives);
            skills.Construction = CreateSkillData(SkillType.Construction);
            skills.Engineering = CreateSkillData(SkillType.Engineering);
            skills.Electronics = CreateSkillData(SkillType.Electronics);
            skills.Medicine = CreateSkillData(SkillType.Medicine);
            skills.Chemistry = CreateSkillData(SkillType.Chemistry);
            skills.Botany = CreateSkillData(SkillType.Botany);
            skills.Driving = CreateSkillData(SkillType.Driving);
            skills.Piloting = CreateSkillData(SkillType.Piloting);
            skills.Navigation = CreateSkillData(SkillType.Navigation);
            skills.Leadership = CreateSkillData(SkillType.Leadership);
            skills.Trading = CreateSkillData(SkillType.Trading);
            skills.Espionage = CreateSkillData(SkillType.Espionage);
            
            return skills;
        }
        
        private SkillData CreateSkillData(SkillType type)
        {
            return new SkillData
            {
                Type = type,
                Level = new SkillLevel
                {
                    Level = 1,
                    CurrentXP = 0f,
                    RequiredXP = XPBase,
                    IsMaxed = false
                },
                TotalXPEarned = 0f,
                PerksUnlocked = 0,
                PerkBitmask = 0
            };
        }
        
        public void AddXP(ulong entityId, SkillType skillType, float xpAmount)
        {
            if (!_entitySkills.TryGetValue(entityId, out var skills))
                return;
            
            ref SkillData skillData = GetSkillRef(ref skills, skillType);
            
            if (skillData.Level.IsMaxed)
                return;
            
            skillData.Level.CurrentXP += xpAmount;
            skillData.TotalXPEarned += xpAmount;
            
            // Check for level up
            while (skillData.Level.CurrentXP >= skillData.Level.RequiredXP && !skillData.Level.IsMaxed)
            {
                skillData.Level.CurrentXP -= skillData.Level.RequiredXP;
                skillData.Level.Level++;
                
                if (skillData.Level.Level >= 10)
                {
                    skillData.Level.IsMaxed = true;
                    skillData.Level.RequiredXP = 0;
                }
                else
                {
                    skillData.Level.RequiredXP = CalculateRequiredXP(skillData.Level.Level);
                }
                
                // Unlock perk at this level
                skillData.PerksUnlocked++;
                skillData.PerkBitmask |= (1UL << (skillData.Level.Level - 1));
                
                EventBus.Publish(new OnSkillLevelUp
                {
                    EntityID = entityId,
                    SkillType = skillType,
                    NewLevel = skillData.Level.Level
                });
            }
            
            _entitySkills[entityId] = skills;
        }
        
        private float CalculateRequiredXP(int level)
        {
            return XPBase * Mathf.Pow(XPMultiplier, level - 1);
        }
        
        public int GetSkillLevel(ulong entityId, SkillType skillType)
        {
            if (!_entitySkills.TryGetValue(entityId, out var skills))
                return 0;
            
            ref SkillData skillData = GetSkillRef(ref skills, skillType);
            return skillData.Level.Level;
        }
        
        public float GetSkillXP(ulong entityId, SkillType skillType)
        {
            if (!_entitySkills.TryGetValue(entityId, out var skills))
                return 0f;
            
            ref SkillData skillData = GetSkillRef(ref skills, skillType);
            return skillData.Level.CurrentXP;
        }
        
        public bool HasPerk(ulong entityId, SkillType skillType, int perkIndex)
        {
            if (!_entitySkills.TryGetValue(entityId, out var skills))
                return false;
            
            ref SkillData skillData = GetSkillRef(ref skills, skillType);
            return (skillData.PerkBitmask & (1UL << perkIndex)) != 0;
        }
        
        public float GetSkillModifier(ulong entityId, SkillType skillType)
        {
            int level = GetSkillLevel(entityId, skillType);
            // Each level provides 5% bonus, max 50% at level 10
            return 1f + (level - 1) * 0.05f;
        }
        
        private ref SkillData GetSkillRef(ref EntitySkills skills, SkillType type)
        {
            switch (type)
            {
                case SkillType.Melee: return ref skills.Melee;
                case SkillType.Ranged: return ref skills.Ranged;
                case SkillType.HeavyWeapons: return ref skills.HeavyWeapons;
                case SkillType.Explosives: return ref skills.Explosives;
                case SkillType.Construction: return ref skills.Construction;
                case SkillType.Engineering: return ref skills.Engineering;
                case SkillType.Electronics: return ref skills.Electronics;
                case SkillType.Medicine: return ref skills.Medicine;
                case SkillType.Chemistry: return ref skills.Chemistry;
                case SkillType.Botany: return ref skills.Botany;
                case SkillType.Driving: return ref skills.Driving;
                case SkillType.Piloting: return ref skills.Piloting;
                case SkillType.Navigation: return ref skills.Navigation;
                case SkillType.Leadership: return ref skills.Leadership;
                case SkillType.Trading: return ref skills.Trading;
                case SkillType.Espionage: return ref skills.Espionage;
                default: return ref skills.Melee;
            }
        }
        
        public EntitySkills GetSkills(ulong entityId)
        {
            if (_entitySkills.TryGetValue(entityId, out var skills))
                return skills;
            return default;
        }
        
        public void Dispose()
        {
            _entitySkills.Dispose();
        }
    }
    
    /// <summary>
    /// Event fired when a skill levels up.
    /// </summary>
    public struct OnSkillLevelUp
    {
        public ulong EntityID;
        public SkillSystem.SkillType SkillType;
        public int NewLevel;
    }
    
    /// <summary>
    /// Static class defining perks for each skill tree.
    /// </summary>
    public static class SkillPerks
    {
        public const int MaxPerksPerSkill = 10;
        
        // Combat Perks
        public const string Melee_Perk1 = "Quick Strike";      // +10% attack speed
        public const string Melee_Perk2 = "Power Blow";        // +15% damage
        public const string Melee_Perk3 = "Bleeding Edge";     // Causes bleed on hit
        public const string Melee_Perk4 = "Parry";             // Chance to block melee attacks
        public const string Melee_Perk5 = "Dual Wield";        // Can use two melee weapons
        public const string Melee_Perk6 = "Armor Piercing";    // Ignores 25% armor
        public const string Melee_Perk7 = "Life Steal";        // Heal on kill
        public const string Melee_Perk8 = "Critical Master";   // +20% crit chance
        public const string Melee_Perk9 = "Whirlwind";         // Hit multiple targets
        public const string Melee_Perk10 = "Legendary Blade";  // Unique weapon effects
        
        public const string Ranged_Perk1 = "Steady Hands";     // +10% accuracy
        public const string Ranged_Perk2 = "Quick Reload";     // +15% reload speed
        public const string Ranged_Perk3 = "Headhunter";       // +25% headshot damage
        public const string Ranged_Perk4 = "Suppression";      // Enemies pinned by fire
        public const string Ranged_Perk5 = "Ricochet";         // Bullets can bounce
        public const string Ranged_Perk6 = "Long Range";       // Reduced damage dropoff
        public const string Ranged_Perk7 = "Multishot";        // Chance for extra projectile
        public const string Ranged_Perk8 = "Silent Killer";    // Stealth kills don't break stealth
        public const string Ranged_Perk9 = "Bullet Time";      // Slow motion when aiming
        public const string Ranged_Perk10 = "Dead Eye";        // Guaranteed crit on weak points
        
        // Technical Perks
        public const string Construction_Perk1 = "Efficient Builder";  // -10% material cost
        public const string Construction_Perk2 = "Quick Build";        // +20% build speed
        public const string Construction_Perk3 = "Reinforced";         // +25% building HP
        public const string Construction_Perk4 = "Blueprint Reader";   // Can craft advanced items
        public const string Construction_Perk5 = "Salvage Expert";     // More materials from deconstruction
        public const string Construction_Perk6 = "Structural Analysis";// See building weaknesses
        public const string Construction_Perk7 = "Prefab Master";      // Instant small structures
        public const string Construction_Perk8 = "Fortress Designer";  // Defensive bonuses
        public const string Construction_Perk9 = "Automation Expert";  // Conveyor efficiency
        public const string Construction_Perk10 = "Architect Vision";  // See optimal placements
        
        public const string Engineering_Perk1 = "Tool Efficiency";     // Tools last longer
        public const string Engineering_Perk2 = "Machine Whisperer";   // Faster repairs
        public const string Engineering_Perk3 = "Overclock";           // Machines run faster
        public const string Engineering_Perk4 = "Jury Rig";            // Emergency fixes
        public const string Engineering_Perk5 = "Efficiency Expert";   // Reduced power consumption
        public const string Engineering_Perk6 = "Master Craftsman";    // Craft rare items
        public const string Engineering_Perk7 = "Predictive Maintenance";// Prevent breakdowns
        public const string Engineering_Perk8 = "Reverse Engineer";    // Learn from salvage
        public const string Engineering_Perk9 = "Mass Production";     // Batch crafting
        public const string Engineering_Perk10 = "Technological Singularity";// Unique inventions
        
        // Medical Perks
        public const string Medicine_Perk1 = "First Aid";        // Faster bandaging
        public const string Medicine_Perk2 = "Diagnosis";        // See exact health status
        public const string Medicine_Perk3 = "Surgical Precision";// Better surgery outcomes
        public const string Medicine_Perk4 = "Pharmacist";       // Stronger meds
        public const string Medicine_Perk5 = "Disease Resistance";// Immunity boost
        public const string Medicine_Perk6 = "Trauma Surgeon";   // Revive chance
        public const string Medicine_Perk7 = "Biotech Expert";   // Cybernetics compatibility
        public const string Medicine_Perk8 = "Pandemic Control"; // Quarantine efficiency
        public const string Medicine_Perk9 = "Regenerative Therapy";// Accelerated healing
        public const string Medicine_Perk10 = "Immortality Research";// Cheat death once
        
        // Social Perks
        public const string Leadership_Perk1 = "Inspiring";      // Morale bonus to nearby allies
        public const string Leadership_Perk2 = "Tactician";      // Combat bonuses to squad
        public const string Leadership_Perk3 = "Delegator";      // NPCs work faster
        public const string Leadership_Perk4 = "Charismatic";    // Better trade prices
        public const string Leadership_Perk5 = "Intimidating";   // Enemies flee easier
        public const string Leadership_Perk6 = "Diplomat";       // Faction relations improve faster
        public const string Leadership_Perk7 = "Commander";      // Larger squad size
        public const string Leadership_Perk8 = "Legend";         // Reputation spreads faster
        public const string Leadership_Perk9 = "Cult Leader";    // Extreme loyalty
        public const string Leadership_Perk10 = "World Leader";  // Global influence
        
        public const string Trading_Perk1 = "Haggler";           // Better buy prices
        public const string Trading_Perk2 = "Salesman";          // Better sell prices
        public const string Trading_Perk3 = "Appraiser";         // See item values
        public const string Trading_Perk4 = "Black Market";      // Access illegal goods
        public const string Trading_Perk5 = "Caravan Master";    // Safer trade routes
        public const string Trading_Perk6 = "Monopoly";          // Price manipulation
        public const string Trading_Perk7 = "Counterfeit";       // Create fake currency
        public const string Trading_Perk8 = "Market Prediction"; // Know price fluctuations
        public const string Trading_Perk9 = "Trade Empire";      // Automated trading
        public const string Trading_Perk10 = "Economic Dominance";// Control market prices
        
        // Additional perks would be defined for remaining skills...
    }
}
