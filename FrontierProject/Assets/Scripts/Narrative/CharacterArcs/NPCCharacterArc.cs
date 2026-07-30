using System;
using Unity.Collections;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.CharacterArcs
{
    /// <summary>
    /// Manages the psychological evolution of individual NPCs.
    /// Arcs progress independently based on world events, traits, and relationships.
    /// Supports 6 major arc types with multi-phase progression.
    /// </summary>
    public class NPCCharacterArc : IService
    {
        private NativeHashMap<ulong, CharacterArcData> _arcs;
        private NativeHashMap<ulong, NativeList<ArcPhaseTransition>> _transitionHistory;
        
        public int Priority => 50;

        public void Initialize()
        {
            _arcs = new NativeHashMap<ulong, CharacterArcData>(1024, Allocator.Persistent);
            _transitionHistory = new NativeHashMap<ulong, NativeList<ArcPhaseTransition>>(1024, Allocator.Persistent);
            
            EventBus.Subscribe<NarrativeEventOccurred>(OnNarrativeEvent);
            EventBus.Subscribe<EntityDestroyed>(OnEntityDeath);
        }

        public void Tick(float dt)
        {
            // Process arc progression for all active NPCs
            var enumerator = _arcs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var arc = enumerator.Current.Value;
                arc.Progress += dt * arc.Velocity;
                
                // Check for phase transitions
                if (arc.Progress >= arc.CurrentPhase.Threshold)
                {
                    TriggerPhaseTransition(enumerator.Current.Key, arc);
                }
                
                _arcs[enumerator.Current.Key] = arc;
            }
        }

        public void Shutdown()
        {
            if (_arcs.IsCreated) _arcs.Dispose();
            if (_transitionHistory.IsCreated) _transitionHistory.Dispose();
            EventBus.Unsubscribe<NarrativeEventOccurred>(OnNarrativeEvent);
            EventBus.Unsubscribe<EntityDestroyed>(OnEntityDeath);
        }

        public void RegisterNPC(ulong npcId, ArcType type, CharacterTraits traits)
        {
            var arc = new CharacterArcData
            {
                NPCId = npcId,
                ArcType = type,
                CurrentPhase = GetInitialPhase(type),
                Progress = 0f,
                Velocity = CalculateVelocity(type, traits),
                CompletedPhases = 0
            };
            
            _arcs.Add(npcId, arc);
            _transitionHistory.Add(npcId, new NativeList<ArcPhaseTransition>(Allocator.TempJob));
        }

        private void TriggerPhaseTransition(ulong npcId, CharacterArcData arc)
        {
            arc.CompletedPhases++;
            arc.CurrentPhase = GetNextPhase(arc.ArcType, arc.CompletedPhases);
            arc.Progress = 0f;
            
            var transition = new ArcPhaseTransition
            {
                NPCId = npcId,
                FromPhase = arc.CompletedPhases - 1,
                ToPhase = arc.CompletedPhases,
                Timestamp = MasterClock.ElapsedSeconds
            };
            
            _transitionHistory[npcId].Add(transition);
            
            // Publish event for UI/Quest updates
            EventBus.Publish(new CharacterArcPhaseChanged 
            { 
                NPCId = npcId, 
                NewPhase = arc.CurrentPhase 
            });
        }

        private void OnNarrativeEvent(NarrativeEventOccurred evt)
        {
            // Accelerate relevant arcs based on event type
            // Example: "Betrayal" event accelerates Corruption/Redemption arcs
        }

        private void OnEntityDeath(EntityDestroyed evt)
        {
            // Finalize arc, create legacy entry
            if (_arcs.ContainsKey(evt.EntityId))
            {
                var arc = _arcs[evt.EntityId];
                EventBus.Publish(new CharacterArcCompleted 
                { 
                    NPCId = evt.EntityId,
                    FinalPhase = arc.CurrentPhase,
                    ArcType = arc.ArcType
                });
            }
        }

        private ArcPhase GetInitialPhase(ArcType type) => type switch
        {
            ArcType.Growth => new ArcPhase { Name = "Stasis", Threshold = 0.3f },
            ArcType.Trauma => new ArcPhase { Name = "Shock", Threshold = 0.2f },
            ArcType.Redemption => new ArcPhase { Name = "Denial", Threshold = 0.4f },
            ArcType.Corruption => new ArcPhase { Name = "Temptation", Threshold = 0.3f },
            ArcType.Betrayal => new ArcPhase { Name = "Doubt", Threshold = 0.5f },
            ArcType.Sacrifice => new ArcPhase { Name = "Calling", Threshold = 0.6f },
            _ => new ArcPhase { Name = "Beginning", Threshold = 1.0f }
        };

        private ArcPhase GetNextPhase(ArcType type, int phaseIndex) => type switch
        {
            ArcType.Growth => phaseIndex switch
            {
                1 => new ArcPhase { Name = "Awakening", Threshold = 0.6f },
                2 => new ArcPhase { Name = "Struggle", Threshold = 0.8f },
                _ => new ArcPhase { Name = "Mastery", Threshold = 1.0f }
            },
            ArcType.Trauma => phaseIndex switch
            {
                1 => new ArcPhase { Name = "Numbing", Threshold = 0.5f },
                2 => new ArcPhase { Name = "Acceptance", Threshold = 0.8f },
                _ => new ArcPhase { Name = "Integration", Threshold = 1.0f }
            },
            // Additional phases for other arc types...
            _ => new ArcPhase { Name = "Conclusion", Threshold = 1.0f }
        };

        private float CalculateVelocity(ArcType type, CharacterTraits traits)
        {
            // Velocity based on personality traits
            // Neuroticism accelerates Trauma, Conscientiousness slows Corruption, etc.
            return 0.1f; 
        }
    }

    public struct CharacterArcData
    {
        public ulong NPCId;
        public ArcType ArcType;
        public ArcPhase CurrentPhase;
        public float Progress;
        public float Velocity;
        public int CompletedPhases;
    }

    public struct ArcPhase
    {
        public FixedString64Bytes Name;
        public float Threshold;
    }

    public struct ArcPhaseTransition
    {
        public ulong NPCId;
        public int FromPhase;
        public int ToPhase;
        public double Timestamp;
    }

    public enum ArcType
    {
        Growth, Trauma, Redemption, Corruption, Betrayal, Sacrifice
    }

    public struct CharacterTraits
    {
        public float Neuroticism;
        public float Conscientiousness;
        public float Openness;
        public float Agreeableness;
        public float Extraversion;
    }

    public struct CharacterArcPhaseChanged
    {
        public ulong NPCId;
        public ArcPhase NewPhase;
    }

    public struct CharacterArcCompleted
    {
        public ulong NPCId;
        public ArcPhase FinalPhase;
        public ArcType ArcType;
    }
}
