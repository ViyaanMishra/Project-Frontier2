using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.Core
{
    /// <summary>
    /// The central cognitive architecture for the narrative engine.
    /// Simulates a "world mind" that tracks global emotional resonance, thematic tension, and narrative entropy.
    /// </summary>
    public class NarrativeConsciousness : IService
    {
        // Global Narrative State
        public float GlobalTension { get; private set; }
        public float EmotionalResonance { get; private set; }
        public float NarrativeEntropy { get; private set; }
        public int CurrentEra { get; private set; }

        // Neuro-Semantic Memory Graph
        private NativeHashMap<int, NarrativeNode> _memoryGraph;
        private NativeList<int> _activeThreads;
        
        // Resonance Fields
        private FixedString512Bytes[] _dominantThemes;
        private float[] _themeIntensity;

        public int Priority => 10;

        public void Initialize()
        {
            _memoryGraph = new NativeHashMap<int, NarrativeNode>(1024, Allocator.Persistent);
            _activeThreads = new NativeList<int>(Allocator.Persistent);
            _dominantThemes = new FixedString512Bytes[6];
            _themeIntensity = new float[6];
            
            GlobalTension = 0.0f;
            EmotionalResonance = 0.0f;
            NarrativeEntropy = 0.0f;
            CurrentEra = 1;
            
            EventBus.Subscribe<NarrativeEventTriggered>(OnNarrativeEvent);
            EventBus.Subscribe<EntityDestroyed>(OnEntityDeath);
        }

        public void Tick(float dt)
        {
            // Decay entropy over time if no events occur
            NarrativeEntropy = Math.Max(0, NarrativeEntropy - (dt * 0.05f));
            
            // Update resonance based on active threads
            CalculateGlobalResonance();
            
            // Check for Era transitions based on tension thresholds
            if (GlobalTension > 100.0f && CurrentEra < 5)
            {
                TransitionEra();
            }
        }

        public void Shutdown()
        {
            if (_memoryGraph.IsCreated) _memoryGraph.Dispose();
            if (_activeThreads.IsCreated) _activeThreads.Dispose();
            EventBus.Unsubscribe<NarrativeEventTriggered>(OnNarrativeEvent);
            EventBus.Unsubscribe<EntityDestroyed>(OnEntityDeath);
        }

        private void CalculateGlobalResonance()
        {
            float totalIntensity = 0;
            for (int i = 0; i < _themeIntensity.Length; i++)
            {
                totalIntensity += _themeIntensity[i];
            }
            EmotionalResonance = totalIntensity / _themeIntensity.Length;
        }

        private void TransitionEra()
        {
            CurrentEra++;
            GlobalTension = 0;
            NarrativeEntropy = 1.0f; // Spike entropy to force new story generation
            
            var eraEvent = new NarrativeEventTriggered
            {
                EventType = NarrativeEventType.EraTransition,
                Intensity = 1.0f,
                Description = $"The world has entered Era {CurrentEra}"
            };
            EventBus.Publish(eraEvent);
        }

        private void OnNarrativeEvent(NarrativeEventTriggered evt)
        {
            GlobalTension += evt.Intensity * 10;
            NarrativeEntropy += 0.1f;
            
            // Inject into memory graph
            int nodeId = evt.GetHashCode();
            if (!_memoryGraph.ContainsKey(nodeId))
            {
                var node = new NarrativeNode
                {
                    Id = nodeId,
                    Type = NodeType.Event,
                    Timestamp = MasterClock.ElapsedTime,
                    Content = evt.Description
                };
                _memoryGraph.Add(nodeId, node);
                _activeThreads.Add(nodeId);
            }
        }

        private void OnEntityDeath(EntityDestroyed evt)
        {
            // Death increases tension and adds a "Loss" theme
            GlobalTension += 5.0f;
            AddTheme("Loss", 0.5f);
        }

        public void AddTheme(string theme, float intensity)
        {
            for (int i = 0; i < _dominantThemes.Length; i++)
            {
                if (_dominantThemes[i].IsEmpty || _dominantThemes[i].ToString() == theme)
                {
                    _dominantThemes[i] = new FixedString512Bytes(theme);
                    _themeIntensity[i] = Math.Min(1.0f, _themeIntensity[i] + intensity);
                    return;
                }
            }
        }

        public bool HasTheme(string theme)
        {
            for (int i = 0; i < _dominantThemes.Length; i++)
            {
                if (!_dominantThemes[i].IsEmpty && _dominantThemes[i].ToString() == theme)
                    return true;
            }
            return false;
        }

        public NativeHashMap<int, NarrativeNode>.Enumerator GetMemoryEnumerator()
        {
            return _memoryGraph.GetEnumerator();
        }
    }

    public struct NarrativeNode
    {
        public int Id;
        public NodeType Type;
        public double Timestamp;
        public FixedString512Bytes Content;
        public int LinkCount;
    }

    public enum NodeType
    {
        Event,
        CharacterArc,
        SagaBeat,
        LoreDiscovery,
        WorldState
    }

    public struct NarrativeEventTriggered
    {
        public NarrativeEventType EventType;
        public float Intensity;
        public FixedString512Bytes Description;
    }

    public enum NarrativeEventType
    {
        None,
        Conflict,
        Resolution,
        Discovery,
        Loss,
        Betrayal,
        Alliance,
        EraTransition
    }
}
