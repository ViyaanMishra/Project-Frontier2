using System;
using Unity.Collections;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.Emergent
{
    /// <summary>
    /// Generates emergent narrative events from gameplay systems.
    /// Creates dynamic stories based on player actions, world simulation, and character interactions.
    /// </summary>
    public class EmergentNarrativeEngine : IService
    {
        private NativeHashMap<FixedString64Bytes, NarrativePattern> _patterns;
        private NativeQueue<DetectableEvent> _eventBuffer;
        private NativeList<EventChain> _activeChains;
        private float _patternMatchThreshold;
        
        public int Priority => 4;

        public void Initialize()
        {
            _patterns = new NativeHashMap<FixedString64Bytes, NarrativePattern>(64, Allocator.Persistent);
            _eventBuffer = new NativeQueue<DetectableEvent>(Allocator.Persistent);
            _activeChains = new NativeList<EventChain>(Allocator.Persistent);
            _patternMatchThreshold = 0.7f;
            
            // Subscribe to game-wide events
            EventBus.Subscribe<PlayerKillEvent>(OnPlayerKill);
            EventBus.Subscribe<FactionConflictEvent>(OnFactionConflict);
            EventBus.Subscribe<DiscoveryEvent>(OnDiscovery);
            EventBus.Subscribe<EconomyFluctuationEvent>(OnEconomyChange);
        }

        public void Tick(double deltaTime)
        {
            // Process buffered events
            while (_eventBuffer.Count > 0)
            {
                var evt = _eventBuffer.Dequeue();
                AnalyzeEvent(evt);
            }
            
            // Update active event chains
            UpdateChains(deltaTime);
        }

        public void Shutdown()
        {
            if (_patterns.IsCreated) _patterns.Dispose();
            if (_eventBuffer.IsCreated) _eventBuffer.Dispose();
            if (_activeChains.IsCreated) _activeChains.Dispose();
        }

        /// <summary>
        /// Registers a narrative pattern for detection.
        /// </summary>
        public void RegisterPattern(NarrativePattern pattern)
        {
            _patterns[pattern.Id] = pattern;
        }

        /// <summary>
        /// Buffers an event for analysis.
        /// </summary>
        public void ReportEvent(DetectableEvent evt)
        {
            _eventBuffer.Enqueue(evt);
        }

        /// <summary>
        /// Analyzes an event for pattern matching and chain creation.
        /// </summary>
        private void AnalyzeEvent(DetectableEvent evt)
        {
            // Check against all registered patterns
            var enumerator = _patterns.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var pattern = enumerator.Current.Value;
                float matchScore = CalculatePatternMatch(pattern, evt);
                
                if (matchScore >= _patternMatchThreshold)
                {
                    // Found a matching pattern - create or extend event chain
                    CreateOrUpdateChain(enumerator.Current.Key, evt, matchScore);
                }
            }
        }

        /// <summary>
        /// Calculates how well an event matches a pattern.
        /// </summary>
        private float CalculatePatternMatch(NarrativePattern pattern, DetectableEvent evt)
        {
            float score = 0f;
            
            // Type match
            if (pattern.RequiredEventType == evt.EventType)
                score += 0.4f;
            
            // Location relevance
            if (pattern.RelevantLocations.Length > 0)
            {
                foreach (var loc in pattern.RelevantLocations)
                {
                    if (evt.Location == loc)
                    {
                        score += 0.2f;
                        break;
                    }
                }
            }
            
            // Faction relevance
            if (pattern.RelevantFactions.Length > 0)
            {
                foreach (var fac in pattern.RelevantFactions)
                {
                    if (evt.InvolvedFactions.Contains(fac))
                    {
                        score += 0.2f;
                        break;
                    }
                }
            }
            
            // Temporal relevance (recent events matter more)
            double timeSinceEvent = MasterClock.Instance.TotalTicks - evt.Timestamp;
            if (timeSinceEvent < pattern.TimeWindow)
                score += 0.2f * (1.0f - (float)(timeSinceEvent / pattern.TimeWindow));
            
            return Math.Min(1.0f, score);
        }

        /// <summary>
        /// Creates a new event chain or updates an existing one.
        /// </summary>
        private void CreateOrUpdateChain(FixedString64Bytes patternId, DetectableEvent evt, float matchScore)
        {
            int existingIndex = -1;
            for (int i = 0; i < _activeChains.Length; i++)
            {
                if (_activeChains[i].PatternId == patternId)
                {
                    existingIndex = i;
                    break;
                }
            }
            
            if (existingIndex >= 0)
            {
                // Extend existing chain
                var chain = _activeChains[existingIndex];
                chain.Events.Add(evt);
                chain.LastEventTime = evt.Timestamp;
                chain.Intensity = Math.Min(1.0f, chain.Intensity + 0.1f);
                _activeChains[existingIndex] = chain;
            }
            else
            {
                // Create new chain
                var newChain = new EventChain
                {
                    PatternId = patternId,
                    Events = new NativeList<DetectableEvent>(Allocator.Persistent),
                    StartTime = evt.Timestamp,
                    LastEventTime = evt.Timestamp,
                    Intensity = matchScore,
                    IsResolved = false
                };
                newChain.Events.Add(evt);
                _activeChains.Add(newChain);
            }
        }

        /// <summary>
        /// Updates active chains and checks for resolution.
        /// </summary>
        private void UpdateChains(double deltaTime)
        {
            for (int i = _activeChains.Length - 1; i >= 0; i--)
            {
                var chain = _activeChains[i];
                
                // Check if chain has timed out
                if (MasterClock.Instance.TotalTicks - chain.LastEventTime > 1000.0) // 1000 ticks timeout
                {
                    ResolveChain(i);
                    continue;
                }
                
                // Check intensity threshold for narrative generation
                if (chain.Intensity >= 0.8f && !chain.HasGeneratedNarrative)
                {
                    GenerateNarrativeFromChain(chain);
                    chain.HasGeneratedNarrative = true;
                    _activeChains[i] = chain;
                }
            }
        }

        /// <summary>
        /// Resolves a chain and publishes the emergent narrative.
        /// </summary>
        private void ResolveChain(int chainIndex)
        {
            var chain = _activeChains[chainIndex];
            
            if (!chain.HasGeneratedNarrative)
            {
                GenerateNarrativeFromChain(chain);
            }
            
            EventBus.Publish(new EmergentNarrativeResolvedEvent
            {
                PatternId = chain.PatternId,
                EventCount = chain.Events.Length,
                FinalIntensity = chain.Intensity
            });
            
            chain.Dispose();
            _activeChains.RemoveAt(chainIndex);
        }

        /// <summary>
        /// Generates a narrative summary from an event chain.
        /// </summary>
        private void GenerateNarrativeFromChain(EventChain chain)
        {
            if (!_patterns.TryGetValue(chain.PatternId, out var pattern))
                return;
            
            var narrative = new EmergentNarrative
            {
                Id = new FixedString64Bytes($"emergent_{MasterClock.Instance.TotalTicks:X}"),
                Title = pattern.DisplayName,
                Description = BuildNarrativeDescription(pattern, chain),
                StartTicks = chain.StartTime,
                EndTicks = MasterClock.Instance.TotalTicks,
                Intensity = chain.Intensity,
                InvolvedEntities = ExtractInvolvedEntities(chain)
            };
            
            EventBus.Publish(new EmergentNarrativeCreatedEvent
            {
                Narrative = narrative
            });
        }

        /// <summary>
        /// Builds a descriptive summary of the emergent narrative.
        /// </summary>
        private FixedString512Bytes BuildNarrativeDescription(NarrativePattern pattern, EventChain chain)
        {
            // Would use template system to build rich description
            return new FixedString512Bytes($"An emerging situation matching pattern: {pattern.DisplayName}");
        }

        /// <summary>
        /// Extracts all unique entities involved in the chain.
        /// </summary>
        private NativeArray<FixedString64Bytes> ExtractInvolvedEntities(EventChain chain)
        {
            var entities = new NativeHashSet<FixedString64Bytes>(16, Allocator.Temp);
            
            for (int i = 0; i < chain.Events.Length; i++)
            {
                foreach (var entity in chain.Events[i].InvolvedEntities)
                {
                    entities.Add(entity);
                }
            }
            
            return entities.ToArray(Allocator.Persistent);
        }

        // Event handlers
        private void OnPlayerKill(PlayerKillEvent evt)
        {
            ReportEvent(new DetectableEvent
            {
                EventType = EventType.Combat,
                Timestamp = MasterClock.Instance.TotalTicks,
                Location = evt.Location,
                InvolvedEntities = new NativeArray<FixedString64Bytes>(new[] { evt.VictimId, evt.KillerId }, Allocator.Temp),
                InvolvedFactions = new NativeArray<FixedString64Bytes>()
            });
        }

        private void OnFactionConflict(FactionConflictEvent evt)
        {
            ReportEvent(new DetectableEvent
            {
                EventType = EventType.Conflict,
                Timestamp = MasterClock.Instance.TotalTicks,
                Location = evt.Location,
                InvolvedEntities = new NativeArray<FixedString64Bytes>(),
                InvolvedFactions = new NativeArray<FixedString64Bytes>(new[] { evt.FactionA, evt.FactionB }, Allocator.Temp)
            });
        }

        private void OnDiscovery(DiscoveryEvent evt)
        {
            ReportEvent(new DetectableEvent
            {
                EventType = EventType.Discovery,
                Timestamp = MasterClock.Instance.TotalTicks,
                Location = evt.Location,
                InvolvedEntities = new NativeArray<FixedString64Bytes>(new[] { evt.DiscovererId }, Allocator.Temp),
                InvolvedFactions = new NativeArray<FixedString64Bytes>()
            });
        }

        private void OnEconomyChange(EconomyFluctuationEvent evt)
        {
            ReportEvent(new DetectableEvent
            {
                EventType = EventType.Economic,
                Timestamp = MasterClock.Instance.TotalTicks,
                Location = evt.MarketLocation,
                InvolvedEntities = new NativeArray<FixedString64Bytes>(),
                InvolvedFactions = new NativeArray<FixedString64Bytes>()
            });
        }
    }

    [Serializable]
    public struct NarrativePattern
    {
        public FixedString64Bytes Id;
        public FixedString128Bytes DisplayName;
        public EventType RequiredEventType;
        public NativeArray<FixedString64Bytes> RelevantLocations;
        public NativeArray<FixedString64Bytes> RelevantFactions;
        public double TimeWindow;
        public int MinEventsForTrigger;
    }

    [Serializable]
    public struct DetectableEvent
    {
        public EventType EventType;
        public double Timestamp;
        public FixedString64Bytes Location;
        public NativeArray<FixedString64Bytes> InvolvedEntities;
        public NativeArray<FixedString64Bytes> InvolvedFactions;
    }

    public struct EventChain
    {
        public FixedString64Bytes PatternId;
        public NativeList<DetectableEvent> Events;
        public double StartTime;
        public double LastEventTime;
        public float Intensity;
        public bool IsResolved;
        public bool HasGeneratedNarrative;

        public void Dispose()
        {
            if (Events.IsCreated) Events.Dispose();
        }
    }

    [Serializable]
    public struct EmergentNarrative
    {
        public FixedString64Bytes Id;
        public FixedString128Bytes Title;
        public FixedString512Bytes Description;
        public double StartTicks;
        public double EndTicks;
        public float Intensity;
        public NativeArray<FixedString64Bytes> InvolvedEntities;
    }

    public enum EventType
    {
        Combat,
        Conflict,
        Discovery,
        Economic,
        Social,
        Environmental,
        Political
    }

    #region Events
    public struct EmergentNarrativeCreatedEvent : IEvent
    {
        public EmergentNarrative Narrative;
    }

    public struct EmergentNarrativeResolvedEvent : IEvent
    {
        public FixedString64Bytes PatternId;
        public int EventCount;
        public float FinalIntensity;
    }

    // External event types (would be defined in respective systems)
    public struct PlayerKillEvent : IEvent
    {
        public FixedString64Bytes VictimId;
        public FixedString64Bytes KillerId;
        public FixedString64Bytes Location;
    }

    public struct FactionConflictEvent : IEvent
    {
        public FixedString64Bytes FactionA;
        public FixedString64Bytes FactionB;
        public FixedString64Bytes Location;
    }

    public struct DiscoveryEvent : IEvent
    {
        public FixedString64Bytes DiscovererId;
        public FixedString64Bytes Location;
        public FixedString64Bytes WhatWasDiscovered;
    }

    public struct EconomyFluctuationEvent : IEvent
    {
        public FixedString64Bytes MarketLocation;
        public FixedString64Bytes Commodity;
        public float PriceChange;
    }
    #endregion
}
