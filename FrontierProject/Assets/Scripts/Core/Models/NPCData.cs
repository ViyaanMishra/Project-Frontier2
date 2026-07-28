using System;
using System.Runtime.InteropServices;

namespace Frontier.Core.Models
{
    /// <summary>
    /// NPC data structure containing DNA, traits, skills, memories, and faction affiliation.
    /// </summary>
    [Serializable]
    public struct NPCData
    {
        public EntityGUID guid;
        public ulong dnaHash; // 64-bit DNA hash
        public string dnaString; // Full DNA sequence for genetic operations
        public int factionId;
        public float age;
        public Gender gender;
        
        // Traits (bitmask for efficiency)
        public TraitFlags traits;
        
        // Skills (indexed by SkillType enum)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] skillLevels;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] skillXP;
        
        // Memory slots (last 10 significant events)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public MemoryEntry[] memories;
        
        // Current state
        public float health;
        public float stamina;
        public int currentJobId;
        public EntityState state;
        
        // Needs (linked to NeedsSystem)
        public NeedState needs;
        
        // Psychology
        public float stressLevel;
        public float morale;
        public TraumaFlags traumas;
        
        // Relationships
        public int relationshipPartnerId;
        public int familyGroupId;
        
        public void Initialize(string dna, int faction, Gender g, float startingAge)
        {
            dnaString = dna;
            dnaHash = ComputeDNAHash(dna);
            factionId = faction;
            gender = g;
            age = startingAge;
            health = 100f;
            stamina = 100f;
            stressLevel = 0f;
            morale = 50f;
            skillLevels = new float[16];
            skillXP = new float[16];
            memories = new MemoryEntry[10];
            traits = TraitFlags.None;
            traumas = TraumaFlags.None;
        }
        
        private ulong ComputeDNAHash(string dna)
        {
            unchecked
            {
                ulong hash = 14695981039346656037UL;
                foreach (char c in dna)
                {
                    hash ^= c;
                    hash *= 1099511628211UL;
                }
                return hash;
            }
        }
        
        public void AddMemory(MemoryEventType type, int entityId, float intensity)
        {
            // Shift memories
            for (int i = memories.Length - 1; i > 0; i--)
            {
                memories[i] = memories[i - 1];
            }
            memories[0] = new MemoryEntry
            {
                eventType = type,
                relatedEntityId = entityId,
                intensity = intensity,
                timestamp = UnityEngine.Time.time
            };
        }
    }
    
    public enum Gender { Male, Female, NonBinary }
    
    [Flags]
    public enum TraitFlags : ulong
    {
        None = 0,
        Strong = 1UL << 0,
        Fast = 1UL << 1,
        Smart = 1UL << 2,
        Charismatic = 1UL << 3,
        Lucky = 1UL << 4,
        NightOwl = 1UL << 5,
        EarlyBird = 1UL << 6,
        IronStomach = 1UL << 7,
        Resilient = 1UL << 8,
        QuickLearner = 1UL << 9,
        NaturalLeader = 1UL << 10,
        Stealthy = 1UL << 11,
        MechanicallyInclined = 1UL << 12,
        GreenThumb = 1UL << 13,
        Medic = 1UL << 14,
        Aggressive = 1UL << 15,
        Pacifist = 1UL << 16,
        Claustrophobic = 1UL << 17,
        Pyromaniac = 1UL << 18,
        Kleptomaniac = 1UL << 19,
        Alcoholic = 1UL << 20,
        Addict = 1UL << 21,
        Hemophobic = 1UL << 22,
        Cannibal = 1UL << 23,
        Mutant = 1UL << 24,
        CyberneticallyEnhanced = 1UL << 25,
        Radiotolerant = 1UL << 26,
        AnomalyTouched = 1UL << 27
    }
    
    [Flags]
    public enum TraumaFlags : ulong
    {
        None = 0,
        PTSD = 1UL << 0,
        Anxiety = 1UL << 1,
        Depression = 1UL << 2,
        Paranoia = 1UL << 3,
        Agoraphobia = 1UL << 4,
        SurvivorGuilt = 1UL << 5,
        CombatStress = 1UL << 6,
        IsolationPsychosis = 1UL << 7,
        RealityDissociation = 1UL << 8
    }
    
    public enum EntityState { Idle, Working, Sleeping, Eating, Socializing, InCombat, Fleeing, Injured, Dead }
    
    [Serializable]
    public struct MemoryEntry
    {
        public MemoryEventType eventType;
        public int relatedEntityId;
        public float intensity; // 0-1, how impactful
        public float timestamp;
    }
    
    public enum MemoryEventType
    {
        CombatKill,
        FriendDeath,
        Injury,
        Achievement,
        Betrayal,
        Romance,
        Birth,
        Disaster,
        Discovery,
        Theft
    }
    
    [Serializable]
    public struct NeedState
    {
        public float hunger;      // 0-100, 0 = starving
        public float thirst;      // 0-100, 0 = dehydrated
        public float energy;      // 0-100, 0 = exhausted
        public float hygiene;     // 0-100, 0 = filthy
        public float social;      // 0-100, 0 = isolated
        public float recreation;  // 0-100, 0 = bored/depressed
        public float comfort;     // 0-100, affected by temp/noise/crowding
        public float safety;      // 0-100, threat proximity
        
        public void Decay(float deltaTime, float difficultyMultiplier = 1f)
        {
            hunger -= 2.5f * deltaTime * difficultyMultiplier;
            thirst -= 4f * deltaTime * difficultyMultiplier;
            energy -= 1.5f * deltaTime * difficultyMultiplier;
            hygiene -= 0.5f * deltaTime * difficultyMultiplier;
            social -= 1f * deltaTime * difficultyMultiplier;
            recreation -= 0.8f * deltaTime * difficultyMultiplier;
        }
        
        public void Clamp()
        {
            hunger = UnityEngine.Mathf.Clamp(hunger, 0, 100);
            thirst = UnityEngine.Mathf.Clamp(thirst, 0, 100);
            energy = UnityEngine.Mathf.Clamp(energy, 0, 100);
            hygiene = UnityEngine.Mathf.Clamp(hygiene, 0, 100);
            social = UnityEngine.Mathf.Clamp(social, 0, 100);
            recreation = UnityEngine.Mathf.Clamp(recreation, 0, 100);
            comfort = UnityEngine.Mathf.Clamp(comfort, 0, 100);
            safety = UnityEngine.Mathf.Clamp(safety, 0, 100);
        }
    }
}
