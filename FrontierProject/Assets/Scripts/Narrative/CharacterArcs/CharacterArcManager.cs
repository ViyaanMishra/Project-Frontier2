using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.CharacterArcs
{
    /// <summary>
    /// Manages individual character narrative arcs, relationships, and personal growth.
    /// Tracks character development, loyalty, betrayal, and evolving storylines.
    /// </summary>
    public class CharacterArcManager : IService
    {
        private NativeHashMap<FixedString64Bytes, CharacterDefinition> _characters;
        private NativeHashMap<FixedString64Bytes, CharacterState> _characterStates;
        private NativeHashMap<FixedString64Bytes, Relationship> _relationships;
        private NativeList<FixedString64Bytes> _activeArcs;
        
        public int Priority => 9;

        public void Initialize()
        {
            _characters = new NativeHashMap<FixedString64Bytes, CharacterDefinition>(128, Allocator.Persistent);
            _characterStates = new NativeHashMap<FixedString64Bytes, CharacterState>(128, Allocator.Persistent);
            _relationships = new NativeHashMap<FixedString64Bytes, Relationship>(256, Allocator.Persistent);
            _activeArcs = new NativeList<FixedString64Bytes>(Allocator.Persistent);
        }

        public void Tick(double deltaTime)
        {
            // Update relationship decay/growth
            UpdateRelationships(deltaTime);
            
            // Check for arc progression triggers
            CheckArcProgression();
        }

        public void Shutdown()
        {
            if (_characters.IsCreated) _characters.Dispose();
            if (_characterStates.IsCreated) _characterStates.Dispose();
            if (_relationships.IsCreated) _relationships.Dispose();
            if (_activeArcs.IsCreated) _activeArcs.Dispose();
        }

        /// <summary>
        /// Registers a character definition.
        /// </summary>
        public void RegisterCharacter(CharacterDefinition character)
        {
            _characters[character.Id] = character;
            
            var state = new CharacterState
            {
                CharacterId = character.Id,
                CurrentArcStage = 0,
                LoyaltyLevel = character.BaseLoyalty,
                MoralityScore = character.BaseMorality,
                IsAlive = true,
                IsRecruited = false
            };
            _characterStates[character.Id] = state;
        }

        /// <summary>
        /// Gets the current state of a character.
        /// </summary>
        public CharacterState GetCharacterState(FixedString64Bytes characterId)
        {
            return _characterStates.TryGetValue(characterId, out var state) ? state : default;
        }

        /// <summary>
        /// Updates the relationship between two characters.
        /// </summary>
        public void UpdateRelationship(FixedString64Bytes charA, FixedString64Bytes charB, float delta)
        {
            var key = MakeRelationshipKey(charA, charB);
            
            if (!_relationships.TryGetValue(key, out var rel))
            {
                rel = new Relationship
                {
                    CharacterA = charA,
                    CharacterB = charB,
                    Strength = 0.5f,
                    TrustLevel = 0.5f,
                    RomanceLevel = 0f,
                    RivalryLevel = 0f
                };
            }

            rel.Strength = Math.Clamp(rel.Strength + delta, -1f, 1f);
            _relationships[key] = rel;

            EventBus.Publish(new RelationshipChangedEvent 
            { 
                CharacterA = charA, 
                CharacterB = charB, 
                NewStrength = rel.Strength 
            });
        }

        /// <summary>
        /// Gets the relationship strength between two characters.
        /// </summary>
        public float GetRelationshipStrength(FixedString64Bytes charA, FixedString64Bytes charB)
        {
            var key = MakeRelationshipKey(charA, charB);
            return _relationships.TryGetValue(key, out var rel) ? rel.Strength : 0.5f;
        }

        /// <summary>
        /// Advances a character's personal arc to the next stage.
        /// </summary>
        public bool AdvanceCharacterArc(FixedString64Bytes characterId)
        {
            if (!_characterStates.TryGetValue(characterId, out var state))
                return false;

            if (!_characters.TryGetValue(characterId, out var character))
                return false;

            if (state.CurrentArcStage >= character.ArcStages.Length - 1)
                return false; // Already at final stage

            state.CurrentArcStage++;
            _characterStates[characterId] = state;

            var newStage = character.ArcStages[state.CurrentArcStage];
            
            EventBus.Publish(new CharacterArcAdvancedEvent
            {
                CharacterId = characterId,
                NewStageIndex = state.CurrentArcStage,
                StageName = newStage.Name
            });

            return true;
        }

        /// <summary>
        /// Recruits a character to the player's party.
        /// </summary>
        public bool RecruitCharacter(FixedString64Bytes characterId)
        {
            if (!_characterStates.TryGetValue(characterId, out var state))
                return false;

            if (state.IsRecruited)
                return false;

            state.IsRecruited = true;
            _characterStates[characterId] = state;
            _activeArcs.Add(characterId);

            EventBus.Publish(new CharacterRecruitedEvent { CharacterId = characterId });
            return true;
        }

        /// <summary>
        /// Marks a character as deceased.
        /// </summary>
        public void KillCharacter(FixedString64Bytes characterId, FixedString128Bytes causeOfDeath)
        {
            if (!_characterStates.TryGetValue(characterId, out var state))
                return;

            state.IsAlive = false;
            state.DeathTimeTicks = MasterClock.Instance.TotalTicks;
            state.CauseOfDeath = causeOfDeath;
            _characterStates[characterId] = state;

            // Remove from active arcs
            for (int i = 0; i < _activeArcs.Length; i++)
            {
                if (_activeArcs[i] == characterId)
                {
                    _activeArcs.RemoveAt(i);
                    break;
                }
            }

            EventBus.Publish(new CharacterDiedEvent 
            { 
                CharacterId = characterId, 
                CauseOfDeath = causeOfDeath 
            });
        }

        /// <summary>
        /// Triggers a loyalty test for a character.
        /// </summary>
        public LoyaltyResult TestLoyalty(FixedString64Bytes characterId, float difficulty)
        {
            if (!_characterStates.TryGetValue(characterId, out var state))
                return LoyaltyResult.Undefined;

            var roll = new Random(Xorshift32.SeedFromTime()).NextFloat();
            var threshold = state.LoyaltyLevel * difficulty;

            if (roll > threshold)
            {
                // Failed loyalty test - potential betrayal
                state.LoyaltyLevel = Math.Max(0, state.LoyaltyLevel - 0.3f);
                _characterStates[characterId] = state;
                return LoyaltyResult.Betrayal;
            }
            else
            {
                // Passed - loyalty increases
                state.LoyaltyLevel = Math.Min(1f, state.LoyaltyLevel + 0.1f);
                _characterStates[characterId] = state;
                return LoyaltyResult.Loyal;
            }
        }

        /// <summary>
        /// Updates all relationships over time (decay or growth).
        /// </summary>
        private void UpdateRelationships(double deltaTime)
        {
            var enumerator = _relationships.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var rel = enumerator.Current.Value;
                
                // Natural decay over time
                rel.Strength = Math.Lerp(rel.Strength, 0.5f, 0.001f * (float)deltaTime);
                
                _relationships[enumerator.Current.Key] = rel;
            }
        }

        /// <summary>
        /// Checks if any character arcs should progress based on game state.
        /// </summary>
        private void CheckArcProgression()
        {
            for (int i = 0; i < _activeArcs.Length; i++)
            {
                var charId = _activeArcs[i];
                if (_characterStates.TryGetValue(charId, out var state))
                {
                    if (_characters.TryGetValue(charId, out var character))
                    {
                        var currentStage = character.ArcStages[state.CurrentArcStage];
                        
                        // Check if progression conditions are met
                        bool conditionsMet = true;
                        for (int j = 0; j < currentStage.RequiredConditions.Length; j++)
                        {
                            var store = ServiceRegistry.Get<StoryVariableStore>();
                            if (!store.EvaluateCondition(currentStage.RequiredConditions[j]))
                            {
                                conditionsMet = false;
                                break;
                            }
                        }

                        if (conditionsMet && !currentStage.IsCompleted)
                        {
                            AdvanceCharacterArc(charId);
                        }
                    }
                }
            }
        }

        private FixedString128Bytes MakeRelationshipKey(FixedString64Bytes a, FixedString64Bytes b)
        {
            // Create consistent key regardless of order
            return new FixedString128Bytes($"{a}:{b}");
        }
    }

    [Serializable]
    public struct CharacterDefinition
    {
        public FixedString64Bytes Id;
        public FixedString64Bytes Name;
        public FixedString512Bytes Description;
        public CharacterArchetype Archetype;
        public float BaseLoyalty;
        public float BaseMorality;
        public NativeArray<CharacterArcStage> ArcStages;
        public NativeArray<FixedString64Bytes> RelatedCharacters;
    }

    [Serializable]
    public struct CharacterArcStage
    {
        public FixedString64Bytes Name;
        public FixedString512Bytes Description;
        public NativeArray<FixedString64Bytes> RequiredConditions;
        public NativeArray<FixedString64Bytes> TriggeredNodes;
        public bool IsCompleted;
    }

    [Serializable]
    public struct CharacterState
    {
        public FixedString64Bytes CharacterId;
        public int CurrentArcStage;
        public float LoyaltyLevel;
        public float MoralityScore;
        public bool IsAlive;
        public bool IsRecruited;
        public double DeathTimeTicks;
        public FixedString128Bytes CauseOfDeath;
    }

    [Serializable]
    public struct Relationship
    {
        public FixedString64Bytes CharacterA;
        public FixedString64Bytes CharacterB;
        public float Strength;      // Overall relationship strength (-1 to 1)
        public float TrustLevel;    // How much they trust each other (0 to 1)
        public float RomanceLevel;  // Romantic involvement (0 to 1)
        public float RivalryLevel;  // Competitive tension (0 to 1)
    }

    public enum CharacterArchetype
    {
        Hero,
        Mentor,
        Ally,
        Rival,
        Antagonist,
        Neutral,
        Merchant,
        QuestGiver
    }

    public enum LoyaltyResult
    {
        Undefined,
        Loyal,
        Betrayal
    }

    #region Events
    public struct CharacterArcAdvancedEvent : IEvent
    {
        public FixedString64Bytes CharacterId;
        public int NewStageIndex;
        public FixedString64Bytes StageName;
    }

    public struct CharacterRecruitedEvent : IEvent
    {
        public FixedString64Bytes CharacterId;
    }

    public struct CharacterDiedEvent : IEvent
    {
        public FixedString64Bytes CharacterId;
        public FixedString128Bytes CauseOfDeath;
    }

    public struct RelationshipChangedEvent : IEvent
    {
        public FixedString64Bytes CharacterA;
        public FixedString64Bytes CharacterB;
        public float NewStrength;
    }
    #endregion
}
