using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// Per-entity need accumulators for survival simulation.
    /// Tracks 8 core needs with decay rates and thresholds.
    /// </summary>
    public class NeedsSystem : IDisposable
    {
        [Serializable]
        public struct NeedValue
        {
            public float Current;      // 0-100
            public float Max;
            public float DecayRate;    // Per tick
            public float LastUpdateTick;
            public bool IsCritical;    // Below 20%
        }
        
        public enum NeedType
        {
            Hunger,
            Thirst,
            Sleep,
            Hygiene,
            Social,
            Recreation,
            Comfort,
            Safety,
            Count
        }
        
        [Serializable]
        public struct EntityNeeds
        {
            public ulong EntityID;
            public NeedValue Hunger;
            public NeedValue Thirst;
            public NeedValue Sleep;
            public NeedValue Hygiene;
            public NeedValue Social;
            public NeedValue Recreation;
            public NeedValue Comfort;
            public NeedValue Safety;
            public float OverallMorale; // Calculated from all needs
            public bool IsStarving;
            public bool IsDehydrated;
            public bool IsExhausted;
            public bool IsDepressed;
        }
        
        private NativeHashMap<ulong, EntityNeeds> _entityNeeds;
        private int _capacity;
        
        // Default configurations per need type
        private static readonly NeedValue DefaultHunger = new NeedValue { Current = 80f, Max = 100f, DecayRate = 0.05f };
        private static readonly NeedValue DefaultThirst = new NeedValue { Current = 80f, Max = 100f, DecayRate = 0.08f };
        private static readonly NeedValue DefaultSleep = new NeedValue { Current = 100f, Max = 100f, DecayRate = 0.03f };
        private static readonly NeedValue DefaultHygiene = new NeedValue { Current = 80f, Max = 100f, DecayRate = 0.02f };
        private static readonly NeedValue DefaultSocial = new NeedValue { Current = 60f, Max = 100f, DecayRate = 0.01f };
        private static readonly NeedValue DefaultRecreation = new NeedValue { Current = 70f, Max = 100f, DecayRate = 0.02f };
        private static readonly NeedValue DefaultComfort = new NeedValue { Current = 50f, Max = 100f, DecayRate = 0.01f };
        private static readonly NeedValue DefaultSafety = new NeedValue { Current = 100f, Max = 100f, DecayRate = 0f };
        
        public int TrackedEntityCount => _entityNeeds.Count();
        
        public NeedsSystem(int capacity = 10000)
        {
            _capacity = capacity;
            _entityNeeds = new NativeHashMap<ulong, EntityNeeds>(capacity, Allocator.Persistent);
        }
        
        public void RegisterEntity(ulong entityId)
        {
            if (_entityNeeds.ContainsKey(entityId))
                return;
            
            if (_entityNeeds.Count() >= _capacity)
            {
                Debug.LogWarning($"NeedsSystem: Capacity ({_capacity}) exceeded!");
                return;
            }
            
            var needs = new EntityNeeds
            {
                EntityID = entityId,
                Hunger = DefaultHunger,
                Thirst = DefaultThirst,
                Sleep = DefaultSleep,
                Hygiene = DefaultHygiene,
                Social = DefaultSocial,
                Recreation = DefaultRecreation,
                Comfort = DefaultComfort,
                Safety = DefaultSafety,
                OverallMorale = 100f,
                IsStarving = false,
                IsDehydrated = false,
                IsExhausted = false,
                IsDepressed = false
            };
            
            _entityNeeds[entityId] = needs;
        }
        
        public void UnregisterEntity(ulong entityId)
        {
            _entityNeeds.Remove(entityId);
        }
        
        public void UpdateNeeds(ulong entityId, long currentTick, float deltaTime)
        {
            if (!_entityNeeds.TryGetValue(entityId, out var needs))
                return;
            
            // Apply decay to each need
            ApplyDecay(ref needs.Hunger, currentTick, deltaTime);
            ApplyDecay(ref needs.Thirst, currentTick, deltaTime);
            ApplyDecay(ref needs.Sleep, currentTick, deltaTime);
            ApplyDecay(ref needs.Hygiene, currentTick, deltaTime);
            ApplyDecay(ref needs.Social, currentTick, deltaTime);
            ApplyDecay(ref needs.Recreation, currentTick, deltaTime);
            ApplyDecay(ref needs.Comfort, currentTick, deltaTime);
            // Safety does not decay naturally
            
            // Update status flags
            needs.IsStarving = needs.Hunger.Current < 20f;
            needs.IsDehydrated = needs.Thirst.Current < 20f;
            needs.IsExhausted = needs.Sleep.Current < 20f;
            needs.IsDepressed = needs.Social.Current < 20f || needs.Recreation.Current < 20f;
            
            // Calculate overall morale (weighted average)
            needs.OverallMorale = CalculateMorale(needs);
            
            _entityNeeds[entityId] = needs;
            
            // Publish event if any need became critical
            if (needs.IsStarving || needs.IsDehydrated || needs.IsExhausted)
            {
                EventBus.Publish(new OnNeedCritical
                {
                    EntityID = entityId,
                    CriticalNeeds = GetCriticalNeeds(needs)
                });
            }
        }
        
        private void ApplyDecay(ref NeedValue need, long currentTick, float deltaTime)
        {
            float ticksSinceUpdate = currentTick - need.LastUpdateTick;
            if (ticksSinceUpdate < 1f)
                return;
            
            need.Current -= need.DecayRate * ticksSinceUpdate;
            need.Current = Mathf.Max(0f, need.Current);
            need.IsCritical = need.Current < 20f;
            need.LastUpdateTick = currentTick;
        }
        
        private float CalculateMorale(EntityNeeds needs)
        {
            // Weighted morale calculation
            float morale = 0f;
            float totalWeight = 0f;
            
            // Survival needs have higher weight
            morale += Mathf.Clamp01(needs.Hunger.Current / needs.Hunger.Max) * 20f;
            morale += Mathf.Clamp01(needs.Thirst.Current / needs.Thirst.Max) * 20f;
            morale += Mathf.Clamp01(needs.Sleep.Current / needs.Sleep.Max) * 15f;
            
            // Psychological needs
            morale += Mathf.Clamp01(needs.Social.Current / needs.Social.Max) * 15f;
            morale += Mathf.Clamp01(needs.Recreation.Current / needs.Recreation.Max) * 10f;
            morale += Mathf.Clamp01(needs.Comfort.Current / needs.Comfort.Max) * 10f;
            morale += Mathf.Clamp01(needs.Safety.Current / needs.Safety.Max) * 10f;
            
            totalWeight = 100f;
            
            return morale / totalWeight * 100f;
        }
        
        private NeedType[] GetCriticalNeeds(EntityNeeds needs)
        {
            var criticalList = new System.Collections.Generic.List<NeedType>();
            
            if (needs.Hunger.Current < 20f) criticalList.Add(NeedType.Hunger);
            if (needs.Thirst.Current < 20f) criticalList.Add(NeedType.Thirst);
            if (needs.Sleep.Current < 20f) criticalList.Add(NeedType.Sleep);
            if (needs.Hygiene.Current < 20f) criticalList.Add(NeedType.Hygiene);
            if (needs.Social.Current < 20f) criticalList.Add(NeedType.Social);
            if (needs.Recreation.Current < 20f) criticalList.Add(NeedType.Recreation);
            if (needs.Comfort.Current < 20f) criticalList.Add(NeedType.Comfort);
            if (needs.Safety.Current < 20f) criticalList.Add(NeedType.Safety);
            
            return criticalList.ToArray();
        }
        
        public void ModifyNeed(ulong entityId, NeedType needType, float amount)
        {
            if (!_entityNeeds.TryGetValue(entityId, out var needs))
                return;
            
            switch (needType)
            {
                case NeedType.Hunger:
                    needs.Hunger.Current = Mathf.Clamp(needs.Hunger.Current + amount, 0f, needs.Hunger.Max);
                    break;
                case NeedType.Thirst:
                    needs.Thirst.Current = Mathf.Clamp(needs.Thirst.Current + amount, 0f, needs.Thirst.Max);
                    break;
                case NeedType.Sleep:
                    needs.Sleep.Current = Mathf.Clamp(needs.Sleep.Current + amount, 0f, needs.Sleep.Max);
                    break;
                case NeedType.Hygiene:
                    needs.Hygiene.Current = Mathf.Clamp(needs.Hygiene.Current + amount, 0f, needs.Hygiene.Max);
                    break;
                case NeedType.Social:
                    needs.Social.Current = Mathf.Clamp(needs.Social.Current + amount, 0f, needs.Social.Max);
                    break;
                case NeedType.Recreation:
                    needs.Recreation.Current = Mathf.Clamp(needs.Recreation.Current + amount, 0f, needs.Recreation.Max);
                    break;
                case NeedType.Comfort:
                    needs.Comfort.Current = Mathf.Clamp(needs.Comfort.Current + amount, 0f, needs.Comfort.Max);
                    break;
                case NeedType.Safety:
                    needs.Safety.Current = Mathf.Clamp(needs.Safety.Current + amount, 0f, needs.Safety.Max);
                    break;
            }
            
            _entityNeeds[entityId] = needs;
        }
        
        public EntityNeeds GetNeeds(ulong entityId)
        {
            if (_entityNeeds.TryGetValue(entityId, out var needs))
                return needs;
            return default;
        }
        
        public float GetMorale(ulong entityId)
        {
            if (!_entityNeeds.TryGetValue(entityId, out var needs))
                return 0f;
            return needs.OverallMorale;
        }
        
        public void Dispose()
        {
            _entityNeeds.Dispose();
        }
    }
    
    /// <summary>
    /// Event fired when an entity's need becomes critical.
    /// </summary>
    public struct OnNeedCritical
    {
        public ulong EntityID;
        public NeedsSystem.NeedType[] CriticalNeeds;
    }
    
    /// <summary>
    /// Food categories with nutrition values.
    /// </summary>
    public enum FoodCategory
    {
        Raw,           // Low nutrition, disease risk
        Cooked,        // Standard nutrition
        Preserved,     // Medium nutrition, long shelf life
        Luxury,        // High nutrition, morale boost
        Medicinal,     // Healing properties
        Toxic,         // Damages health
        Mutated,       // Unpredictable effects
        Synthetic,     // Artificial, low satisfaction
        Anomalous,     // Reality-bending effects
        Rotten         // Negative nutrition
    }
    
    /// <summary>
    /// Water purity levels.
    /// </summary>
    public enum WaterPurity
    {
        Contaminated,  // Causes disease
        Murky,         // Low quality
        Normal,        // Safe to drink
        Purified,      // Bonus hydration
        MineralRich,   // Health benefits
        AnomalyTainted // Strange effects
    }
}
