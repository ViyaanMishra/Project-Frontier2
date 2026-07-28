using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Faction politics system handling internal blocs, strikes, sabotage, and lockouts.
    /// </summary>
    public enum FactionStance
    {
        Allied = 0,
        Friendly = 1,
        Neutral = 2,
        Hostile = 3,
        AtWar = 4
    }

    public struct FactionBloc
    {
        public string blocId;
        public string blocName;
        public float influence;       // 0.0 - 1.0
        public int memberCount;
        public float ideologyScore;   // -1.0 (radical) to 1.0 (conservative)
        public bool isRuling;
        public float satisfaction;    // 0.0 - 1.0
        public float aggression;      // 0.0 - 1.0
    }

    public struct PoliticalEvent
    {
        public string eventId;
        public string factionId;
        public string blocId;
        public EventType type;
        public float impact;
        public float duration;
        public float startTime;
    }

    public enum EventType
    {
        Strike,
        Sabotage,
        Lockout,
        Coup,
        Reform,
        Alliance,
        Betrayal
    }

    public class FactionPoliticsSystem : IDisposable
    {
        private NativeHashMap<string, NativeList<FactionBloc>> _factionBlocs;
        private NativeHashMap<string, FactionStance> _factionRelations;
        private NativeList<PoliticalEvent> _activeEvents;
        private readonly float _eventCheckInterval = 3600f; // Check every game hour
        private float _nextEventCheck;

        public FactionPoliticsSystem()
        {
            _factionBlocs = new NativeHashMap<string, NativeList<FactionBloc>>(100, Allocator.Persistent);
            _factionRelations = new NativeHashMap<string, FactionStance>(100, Allocator.Persistent);
            _activeEvents = new NativeList<PoliticalEvent>(Allocator.Persistent);
            _nextEventCheck = 0f;
        }

        public void InitializeFaction(string factionId, int blocCount)
        {
            var blocs = new NativeList<FactionBloc>(blocCount, Allocator.Persistent);

            for (int i = 0; i < blocCount; i++)
            {
                var bloc = new FactionBloc
                {
                    blocId = $"{factionId}_BLOC_{i}",
                    blocName = $"Bloc {i + 1}",
                    influence = 1f / blocCount,
                    memberCount = UnityEngine.Random.Range(10, 100),
                    ideologyScore = UnityEngine.Random.Range(-1f, 1f),
                    isRuling = i == 0,
                    satisfaction = UnityEngine.Random.Range(0.3f, 0.8f),
                    aggression = UnityEngine.Random.Range(0.1f, 0.5f)
                };
                blocs.Add(bloc);
            }

            if (_factionBlocs.ContainsKey(factionId))
                _factionBlocs[factionId].Dispose();

            _factionBlocs.Add(factionId, blocs);
            _factionRelations.TryAdd(factionId, FactionStance.Neutral);
        }

        public void SetFactionRelation(string factionId, FactionStance stance)
        {
            if (_factionRelations.ContainsKey(factionId))
                _factionRelations[factionId] = stance;
            else
                _factionRelations.TryAdd(factionId, stance);
        }

        public void SimulatePolitics(float gameTime)
        {
            if (gameTime < _nextEventCheck) return;
            _nextEventCheck = gameTime + _eventCheckInterval;

            var keys = _factionBlocs.GetKeyArray(Allocator.Temp);

            foreach (var factionId in keys)
            {
                if (!_factionBlocs.TryGetValue(factionId, out var blocs)) continue;

                // Update bloc satisfaction based on events
                for (int i = 0; i < blocs.Length; i++)
                {
                    var bloc = blocs[i];

                    // Satisfaction decay/growth
                    float satisfactionChange = UnityEngine.Random.Range(-0.01f, 0.01f);
                    bloc.satisfaction = math.clamp(bloc.satisfaction + satisfactionChange, 0f, 1f);

                    // Check for strike condition
                    if (bloc.satisfaction < 0.3f && UnityEngine.Random.value < 0.1f)
                    {
                        TriggerStrike(factionId, bloc.blocId);
                    }

                    // Check for sabotage by radical blocs
                    if (bloc.ideologyScore < -0.5f && bloc.aggression > 0.7f && UnityEngine.Random.value < 0.05f)
                    {
                        TriggerSabotage(factionId, bloc.blocId);
                    }

                    blocs[i] = bloc;
                }

                // Check for power shifts
                CheckPowerShift(factionId, blocs);
            }

            keys.Dispose();

            // Clean up expired events
            for (int i = _activeEvents.Length - 1; i >= 0; i--)
            {
                var evt = _activeEvents[i];
                if (gameTime - evt.startTime > evt.duration)
                {
                    _activeEvents.RemoveAt(i);
                }
            }
        }

        private void TriggerStrike(string factionId, string blocId)
        {
            var evt = new PoliticalEvent
            {
                eventId = Guid.NewGuid().ToString(),
                factionId = factionId,
                blocId = blocId,
                type = EventType.Strike,
                impact = -0.2f, // Negative resource production
                duration = 7200f, // 2 hours
                startTime = UnityEngine.Time.time
            };
            _activeEvents.Add(evt);

            // Reduce satisfaction further
            if (_factionBlocs.TryGetValue(factionId, out var blocs))
            {
                for (int i = 0; i < blocs.Length; i++)
                {
                    if (blocs[i].blocId == blocId)
                    {
                        var bloc = blocs[i];
                        bloc.satisfaction = math.max(0f, bloc.satisfaction - 0.1f);
                        blocs[i] = bloc;
                        break;
                    }
                }
            }
        }

        private void TriggerSabotage(string factionId, string blocId)
        {
            var evt = new PoliticalEvent
            {
                eventId = Guid.NewGuid().ToString(),
                factionId = factionId,
                blocId = blocId,
                type = EventType.Sabotage,
                impact = -0.3f, // Infrastructure damage
                duration = 3600f,
                startTime = UnityEngine.Time.time
            };
            _activeEvents.Add(evt);
        }

        private void CheckPowerShift(string factionId, NativeList<FactionBloc> blocs)
        {
            // Find ruling bloc
            int rulingIndex = -1;
            for (int i = 0; i < blocs.Length; i++)
            {
                if (blocs[i].isRuling)
                {
                    rulingIndex = i;
                    break;
                }
            }

            if (rulingIndex < 0) return;

            var rulingBloc = blocs[rulingIndex];

            // Check if another bloc has more influence+satisfaction
            for (int i = 0; i < blocs.Length; i++)
            {
                if (i == rulingIndex) continue;

                var challenger = blocs[i];
                float rulingPower = rulingBloc.influence * rulingBloc.satisfaction;
                float challengerPower = challenger.influence * challenger.satisfaction;

                if (challengerPower > rulingPower * 1.5f && UnityEngine.Random.value < 0.1f)
                {
                    // Coup attempt
                    if (UnityEngine.Random.value < challenger.aggression)
                    {
                        TriggerCoup(factionId, challenger.blocId, rulingBloc.blocId);
                    }
                }
            }
        }

        private void TriggerCoup(string factionId, string challengerId, string rulingId)
        {
            var evt = new PoliticalEvent
            {
                eventId = Guid.NewGuid().ToString(),
                factionId = factionId,
                blocId = challengerId,
                type = EventType.Coup,
                impact = 0.5f, // Major shift
                duration = 1800f,
                startTime = UnityEngine.Time.time
            };
            _activeEvents.Add(evt);

            // Swap ruling status
            if (_factionBlocs.TryGetValue(factionId, out var blocs))
            {
                for (int i = 0; i < blocs.Length; i++)
                {
                    var bloc = blocs[i];
                    if (bloc.blocId == challengerId)
                    {
                        bloc.isRuling = true;
                        blocs[i] = bloc;
                    }
                    else if (bloc.blocId == rulingId)
                    {
                        bloc.isRuling = false;
                        bloc.satisfaction *= 0.5f;
                        blocs[i] = bloc;
                    }
                }
            }
        }

        public float GetFactionProductionModifier(string factionId)
        {
            float modifier = 1f;

            // Apply event impacts
            for (int i = 0; i < _activeEvents.Length; i++)
            {
                if (_activeEvents[i].factionId == factionId)
                {
                    if (_activeEvents[i].type == EventType.Strike ||
                        _activeEvents[i].type == EventType.Sabotage)
                    {
                        modifier += _activeEvents[i].impact;
                    }
                }
            }

            return math.max(0f, modifier);
        }

        public FactionStance GetRelation(string factionId)
        {
            if (_factionRelations.TryGetValue(factionId, out var stance))
                return stance;
            return FactionStance.Neutral;
        }

        public NativeList<PoliticalEvent> GetActiveEvents() => _activeEvents;

        public void Dispose()
        {
            var keys = _factionBlocs.GetKeyArray(Allocator.Temp);
            foreach (var key in keys)
            {
                if (_factionBlocs.TryGetValue(key, out var list))
                    list.Dispose();
            }
            _factionBlocs.Dispose();
            _factionRelations.Dispose();
            _activeEvents.Dispose();
            keys.Dispose();
        }
    }
}
