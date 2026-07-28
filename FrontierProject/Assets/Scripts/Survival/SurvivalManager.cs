using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frontier.Survival
{
    /// <summary>
    /// Survival need types with thresholds and effects.
    /// </summary>
    public enum NeedType
    {
        Hunger,         // Food consumption
        Thirst,         // Water consumption
        Sleep,          // Fatigue accumulation
        Hygiene,        // Disease risk
        Social,         // Isolation penalty
        Recreation,     // Boredom management
        Comfort,        // Temperature/crowding
        Safety          // Threat proximity
    }

    /// <summary>
    /// Status effect definition.
    /// </summary>
    [Serializable]
    public struct StatusEffect
    {
        public string Id;
        public string Name;
        public EffectType Type;
        public float Duration;          // Seconds (-1 for permanent until cured)
        public float TickInterval;      // Seconds between ticks
        public List<StatModifier> Modifiers;
        public bool IsDebuff;
        public string CureItemId;       // Item that cures this effect
    }

    public enum EffectType
    {
        Bleeding,
        Poisoned,
        Irradiated,
        Burning,
        Frozen,
        Shocked,
        BrokenLimb,
        Infection,
        AnomalyTouched,
        WellFed,
        Rested,
        Inspired,
        Dehydrated,
        Starving,
        Exhausted,
        Depressed
    }

    [Serializable]
    public struct StatModifier
    {
        public StatType Stat;
        public float Value;             // Absolute or percentage
        public bool IsPercentage;
    }

    public enum StatType
    {
        Health,
        Stamina,
        MovementSpeed,
        DamageDealt,
        DamageTaken,
        CraftingSpeed,
        HarvestYield,
        ExperienceGain
    }

    /// <summary>
    /// Manages survival needs for an entity.
    /// </summary>
    public class NeedsTracker
    {
        private Dictionary<NeedType, float> _needs = new Dictionary<NeedType, float>();
        private Dictionary<NeedType, float> _decayRates = new Dictionary<NeedType, float>();
        private Dictionary<NeedType, float> _thresholds = new Dictionary<NeedType, float>();
        
        // Default values (0-100 scale)
        private const float DEFAULT_DECAY_HUNGER = 2.0f;     // Per hour
        private const float DEFAULT_DECAY_THIRST = 3.0f;     // Per hour
        private const float DEFAULT_DECAY_SLEEP = 1.5f;      // Per hour
        private const float DEFAULT_DECAY_HYGIENE = 0.5f;    // Per hour
        private const float DEFAULT_DECAY_SOCIAL = 1.0f;     // Per hour
        private const float DEFAULT_DECAY_RECREATION = 0.8f; // Per hour
        private const float DEFAULT_DECAY_COMFORT = 0.3f;    // Per hour
        private const float DEFAULT_DECAY_SAFETY = 0.0f;     // Event-based

        public event Action<NeedType, float> OnNeedChanged;
        public event Action<NeedType> OnNeedCritical;

        public NeedsTracker()
        {
            InitializeNeeds();
        }

        private void InitializeNeeds()
        {
            // Initialize all needs to 100%
            foreach (NeedType need in Enum.GetValues(typeof(NeedType)))
            {
                _needs[need] = 100f;
                _thresholds[need] = 25f; // Critical threshold
            }

            // Set decay rates (per minute for gameplay balance)
            _decayRates[NeedType.Hunger] = DEFAULT_DECAY_HUNGER / 60f;
            _decayRates[NeedType.Thirst] = DEFAULT_DECAY_THIRST / 60f;
            _decayRates[NeedType.Sleep] = DEFAULT_DECAY_SLEEP / 60f;
            _decayRates[NeedType.Hygiene] = DEFAULT_DECAY_HYGIENE / 60f;
            _decayRates[NeedType.Social] = DEFAULT_DECAY_SOCIAL / 60f;
            _decayRates[NeedType.Recreation] = DEFAULT_DECAY_RECREATION / 60f;
            _decayRates[NeedType.Comfort] = DEFAULT_DECAY_COMFORT / 60f;
            _decayRates[NeedType.Safety] = 0f;
        }

        /// <summary>
        /// Update needs based on time delta.
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (var kvp in _decayRates)
            {
                if (kvp.Value <= 0) continue;

                _needs[kvp.Key] -= kvp.Value * deltaTime;
                _needs[kvp.Key] = Mathf.Clamp(_needs[kvp.Key], 0, 100);

                OnNeedChanged?.Invoke(kvp.Key, _needs[kvp.Key]);

                // Check critical threshold
                if (_needs[kvp.Key] <= _thresholds[kvp.Key])
                {
                    OnNeedCritical?.Invoke(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Satisfy a need by a certain amount.
        /// </summary>
        public void Satisfy(NeedType need, float amount)
        {
            if (!_needs.ContainsKey(need))
                return;

            _needs[need] = Mathf.Min(_needs[need] + amount, 100f);
            OnNeedChanged?.Invoke(need, _needs[need]);
        }

        /// <summary>
        /// Get current need value.
        /// </summary>
        public float GetNeed(NeedType need)
        {
            return _needs.TryGetValue(need, out float value) ? value : 0f;
        }

        /// <summary>
        /// Set decay rate modifier (from perks, items, etc.).
        /// </summary>
        public void SetDecayModifier(NeedType need, float modifier)
        {
            // modifier: 0.5 = half decay, 2.0 = double decay
            if (_decayRates.ContainsKey(need))
                _decayRates[need] *= modifier;
        }

        /// <summary>
        /// Apply environmental effects on needs.
        /// </summary>
        public void ApplyEnvironmentEffect(string environmentType)
        {
            switch (environmentType)
            {
                case "hot":
                    _decayRates[NeedType.Thirst] *= 1.5f;
                    _decayRates[NeedType.Comfort] *= 1.3f;
                    break;
                case "cold":
                    _decayRates[NeedType.Comfort] *= 1.5f;
                    break;
                case "dangerous":
                    _decayRates[NeedType.Safety] = 5f; // Rapid safety decay
                    break;
                case "social":
                    _needs[NeedType.Social] = Mathf.Min(_needs[NeedType.Social] + 0.5f, 100f);
                    break;
            }
        }
    }

    /// <summary>
    /// Manages status effects on an entity.
    /// </summary>
    public class StatusEffectManager
    {
        private Dictionary<string, StatusEffect> _activeEffects = new Dictionary<string, StatusEffect>();
        private Dictionary<string, float> _effectTimers = new Dictionary<string, float>();
        
        // Predefined effects database
        private static Dictionary<string, StatusEffect> _effectDatabase;

        public event Action<string> OnEffectApplied;
        public event Action<string> OnEffectRemoved;
        public event Action<string> OnEffectTicked;

        static StatusEffectManager()
        {
            InitializeEffectDatabase();
        }

        private static void InitializeEffectDatabase()
        {
            _effectDatabase = new Dictionary<string, StatusEffect>();

            // Debuffs
            _effectDatabase["bleeding"] = new StatusEffect
            {
                Id = "bleeding",
                Name = "Bleeding",
                Type = EffectType.Bleeding,
                Duration = -1,
                TickInterval = 2f,
                IsDebuff = true,
                CureItemId = "bandage",
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.Health, Value = -5f, IsPercentage = false }
                }
            };

            _effectDatabase["poisoned"] = new StatusEffect
            {
                Id = "poisoned",
                Name = "Poisoned",
                Type = EffectType.Poisoned,
                Duration = 60f,
                TickInterval = 5f,
                IsDebuff = true,
                CureItemId = "antidote",
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.Health, Value = -10f, IsPercentage = false },
                    new StatModifier { Stat = StatType.MovementSpeed, Value = -20f, IsPercentage = true }
                }
            };

            _effectDatabase["irradiated"] = new StatusEffect
            {
                Id = "irradiated",
                Name = "Irradiated",
                Type = EffectType.Irradiated,
                Duration = 300f,
                TickInterval = 10f,
                IsDebuff = true,
                CureItemId = "radAway",
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.Health, Value = -15f, IsPercentage = false },
                    new StatModifier { Stat = StatType.ExperienceGain, Value = -50f, IsPercentage = true }
                }
            };

            _effectDatabase["burning"] = new StatusEffect
            {
                Id = "burning",
                Name = "Burning",
                Type = EffectType.Burning,
                Duration = 10f,
                TickInterval = 1f,
                IsDebuff = true,
                CureItemId = "water",
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.Health, Value = -20f, IsPercentage = false }
                }
            };

            _effectDatabase["broken_leg"] = new StatusEffect
            {
                Id = "broken_leg",
                Name = "Broken Leg",
                Type = EffectType.BrokenLimb,
                Duration = -1,
                TickInterval = 0f,
                IsDebuff = true,
                CureItemId = "splint",
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.MovementSpeed, Value = -60f, IsPercentage = true }
                }
            };

            _effectDatabase["anomaly_touched"] = new StatusEffect
            {
                Id = "anomaly_touched",
                Name = "Anomaly Touched",
                Type = EffectType.AnomalyTouched,
                Duration = -1,
                TickInterval = 30f,
                IsDebuff = true,
                CureItemId = "realityAnchor",
                Modifiers = new List<StatModifier>() // Random teleports handled separately
            };

            // Buffs
            _effectDatabase["well_fed"] = new StatusEffect
            {
                Id = "well_fed",
                Name = "Well Fed",
                Type = EffectType.WellFed,
                Duration = 1800f, // 30 minutes
                TickInterval = 0f,
                IsDebuff = false,
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.Health, Value = 10f, IsPercentage = true },
                    new StatModifier { Stat = StatType.Stamina, Value = 15f, IsPercentage = true }
                }
            };

            _effectDatabase["rested"] = new StatusEffect
            {
                Id = "rested",
                Name = "Rested",
                Type = EffectType.Rested,
                Duration = 900f, // 15 minutes
                TickInterval = 0f,
                IsDebuff = false,
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.ExperienceGain, Value = 25f, IsPercentage = true },
                    new StatModifier { Stat = StatType.CraftingSpeed, Value = 20f, IsPercentage = true }
                }
            };

            _effectDatabase["inspired"] = new StatusEffect
            {
                Id = "inspired",
                Name = "Inspired",
                Type = EffectType.Inspired,
                Duration = 600f, // 10 minutes
                TickInterval = 0f,
                IsDebuff = false,
                Modifiers = new List<StatModifier>
                {
                    new StatModifier { Stat = StatType.CraftingSpeed, Value = 50f, IsPercentage = true },
                    new StatModifier { Stat = StatType.HarvestYield, Value = 30f, IsPercentage = true }
                }
            };
        }

        /// <summary>
        /// Apply a status effect.
        /// </summary>
        public bool ApplyEffect(string effectId)
        {
            if (!_effectDatabase.ContainsKey(effectId))
            {
                Debug.LogError($"[StatusEffectManager] Unknown effect: {effectId}");
                return false;
            }

            var effect = _effectDatabase[effectId];

            // Check if already active (for non-stacking effects)
            if (_activeEffects.ContainsKey(effectId))
            {
                // Refresh duration if applicable
                if (effect.Duration > 0)
                    _effectTimers[effectId] = effect.Duration;
                return true;
            }

            _activeEffects[effectId] = effect;
            
            if (effect.Duration > 0)
                _effectTimers[effectId] = effect.Duration;

            OnEffectApplied?.Invoke(effectId);
            Debug.Log($"[StatusEffectManager] Applied effect: {effect.Name}");
            return true;
        }

        /// <summary>
        /// Update effect timers and ticks.
        /// </summary>
        public void Update(float deltaTime)
        {
            var toRemove = new List<string>();

            foreach (var kvp in _activeEffects)
            {
                var effect = kvp.Value;
                string id = kvp.Key;

                // Handle timed effects
                if (effect.Duration > 0)
                {
                    _effectTimers[id] -= deltaTime;
                    if (_effectTimers[id] <= 0)
                    {
                        toRemove.Add(id);
                        continue;
                    }
                }

                // Handle ticking effects
                if (effect.TickInterval > 0)
                {
                    if (!_effectTimers.ContainsKey(id + "_tick"))
                        _effectTimers[id + "_tick"] = effect.TickInterval;

                    _effectTimers[id + "_tick"] -= deltaTime;
                    if (_effectTimers[id + "_tick"] <= 0)
                    {
                        _effectTimers[id + "_tick"] = effect.TickInterval;
                        OnEffectTicked?.Invoke(id);
                        
                        // Apply tick damage/healing
                        foreach (var mod in effect.Modifiers)
                        {
                            if (mod.Stat == StatType.Health && mod.Value < 0)
                            {
                                // Health damage per tick
                                Debug.Log($"[StatusEffectManager] {effect.Name} ticked for {Mathf.Abs(mod.Value)} health");
                            }
                        }
                    }
                }
            }

            // Remove expired effects
            foreach (var id in toRemove)
            {
                RemoveEffect(id);
            }
        }

        /// <summary>
        /// Remove a status effect.
        /// </summary>
        public void RemoveEffect(string effectId)
        {
            if (_activeEffects.ContainsKey(effectId))
            {
                _activeEffects.Remove(effectId);
                
                if (_effectTimers.ContainsKey(effectId))
                    _effectTimers.Remove(effectId);
                if (_effectTimers.ContainsKey(effectId + "_tick"))
                    _effectTimers.Remove(effectId + "_tick");

                OnEffectRemoved?.Invoke(effectId);
            }
        }

        /// <summary>
        /// Cure an effect with an item.
        /// </summary>
        public bool CureWithItem(string itemId)
        {
            foreach (var kvp in _activeEffects)
            {
                if (kvp.Value.CureItemId == itemId)
                {
                    RemoveEffect(kvp.Key);
                    Debug.Log($"[StatusEffectManager] Cured {kvp.Value.Name} with {itemId}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get all active effects.
        /// </summary>
        public List<StatusEffect> GetActiveEffects()
        {
            return new List<StatusEffect>(_activeEffects.Values);
        }

        /// <summary>
        /// Check if a specific effect is active.
        /// </summary>
        public bool HasEffect(string effectId)
        {
            return _activeEffects.ContainsKey(effectId);
        }

        /// <summary>
        /// Get combined stat modifiers from all active effects.
        /// </summary>
        public Dictionary<StatType, float> GetStatModifiers()
        {
            var modifiers = new Dictionary<StatType, float>();

            foreach (var effect in _activeEffects.Values)
            {
                foreach (var mod in effect.Modifiers)
                {
                    if (!modifiers.ContainsKey(mod.Stat))
                        modifiers[mod.Stat] = 0;

                    modifiers[mod.Stat] += mod.IsPercentage ? mod.Value : mod.Value;
                }
            }

            return modifiers;
        }
    }
}
