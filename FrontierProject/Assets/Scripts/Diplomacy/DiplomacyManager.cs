using System;
using UnityEngine;
using Frontier.Core;

namespace Frontier.Diplomacy
{
    /// <summary>
    /// Manages faction relations, trade agreements, war/peace states, and diplomatic actions.
    /// Integrated with the central EventBus for cross-system communication.
    /// </summary>
    public class DiplomacyManager : MonoBehaviour
    {
        public static DiplomacyManager Instance { get; private set; }
        
        [SerializeField] private FactionData[] allFactions;
        [SerializeField] private float relationshipDecayRate = 0.1f;
        
        private NativeHashMap<int, FactionState> factionStates;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Initialize();
        }
        
        private void Initialize()
        {
            factionStates = new NativeHashMap<int, FactionState>(allFactions.Length);
            
            foreach (var faction in allFactions)
            {
                var state = new FactionState
                {
                    factionId = faction.id,
                    reputation = faction.startingReputation,
                    relationshipState = RelationshipState.Neutral,
                    tradeAgreements = new int[0],
                    activeWars = new int[0],
                    embassies = new int[0]
                };
                factionStates.Add(faction.id, state);
            }
        }
        
        public void ModifyReputation(int factionId, float amount, ReputationReason reason)
        {
            if (!factionStates.TryGetValue(factionId, out var state)) return;
            
            // Apply modifiers based on reason
            float multiplier = GetReasonMultiplier(reason);
            float actualChange = amount * multiplier;
            
            state.reputation = Mathf.Clamp(state.reputation + actualChange, -1000, 1000);
            UpdateRelationshipState(ref state);
            
            factionStates[factionId] = state;
            
            // Fire event through unified EventBus
            EventBus<FactionReputationChanged>.Raise(new FactionReputationChanged
            {
                factionId = factionId,
                oldReputation = state.reputation - actualChange,
                newReputation = state.reputation,
                reason = reason
            });
            
            // Publish to faction politics system for simulation integration
            EventBus<FactionConflictEvent>.Raise(new FactionConflictEvent
            {
                FactionA = factionId,
                ConflictType = reason switch
                {
                    ReputationReason.Murder or ReputationReason.Theft or ReputationReason.TerritoryViolation => ConflictType.HostileAction,
                    ReputationReason.QuestCompletion or ReputationReason.CombatAssist => ConflictType.CooperativeAction,
                    _ => ConflictType.DiplomaticAction
                },
                Severity = Mathf.Abs(actualChange) / 100f
            });
        }
        
        private float GetReasonMultiplier(ReputationReason reason)
        {
            return reason switch
            {
                ReputationReason.QuestCompletion => 1.0f,
                ReputationReason.Trade => 0.5f,
                ReputationReason.CombatAssist => 1.5f,
                ReputationReason.TerritoryViolation => -2.0f,
                ReputationReason.Theft => -3.0f,
                ReputationReason.Murder => -5.0f,
                ReputationReason.Gift => 0.8f,
                ReputationReason.Treaty => 2.0f,
                _ => 1.0f
            };
        }
        
        private void UpdateRelationshipState(ref FactionState state)
        {
            RelationshipState newState;
            
            if (state.reputation >= 500)
                newState = RelationshipState.Allied;
            else if (state.reputation >= 200)
                newState = RelationshipState.Friendly;
            else if (state.reputation >= -200)
                newState = RelationshipState.Neutral;
            else if (state.reputation >= -500)
                newState = RelationshipState.Unfriendly;
            else
                newState = RelationshipState.Hostile;
            
            if (newState != state.relationshipState)
            {
                var oldState = state.relationshipState;
                state.relationshipState = newState;
                
                EventBus<FactionRelationChanged>.Raise(new FactionRelationChanged
                {
                    factionId = state.factionId,
                    oldState = oldState,
                    newState = newState
                });
            }
        }
        
        public bool DeclareWar(int aggressorId, int targetId)
        {
            if (!factionStates.TryGetValue(aggressorId, out var aggressorState) ||
                !factionStates.TryGetValue(targetId, out var targetState))
                return false;
            
            // Add to active wars
            aggressorState.activeWars = AddToArray(aggressorState.activeWars, targetId);
            targetState.activeWars = AddToArray(targetState.activeWars, aggressorId);
            
            // Set relationship to hostile
            aggressorState.reputation = Mathf.Min(aggressorState.reputation, -500);
            targetState.reputation = Mathf.Min(targetState.reputation, -500);
            
            factionStates[aggressorId] = aggressorState;
            factionStates[targetId] = targetState;
            
            EventBus<FactionWarDeclared>.Raise(new FactionWarDeclared
            {
                aggressorId = aggressorId,
                targetId = targetId
            });
            
            return true;
        }
        
        public bool SignPeaceTreaty(int faction1Id, int faction2Id, int[] terms)
        {
            if (!factionStates.TryGetValue(faction1Id, out var state1) ||
                !factionStates.TryGetValue(faction2Id, out var state2))
                return false;
            
            // Remove from active wars
            state1.activeWars = RemoveFromArray(state1.activeWars, faction2Id);
            state2.activeWars = RemoveFromArray(state2.activeWars, faction1Id);
            
            // Boost reputation
            state1.reputation = Mathf.Max(state1.reputation, -200);
            state2.reputation = Mathf.Max(state2.reputation, -200);
            
            factionStates[faction1Id] = state1;
            factionStates[faction2Id] = state2;
            
            EventBus<FactionPeaceSigned>.Raise(new FactionPeaceSigned
            {
                faction1Id = faction1Id,
                faction2Id = faction2Id,
                terms = terms
            });
            
            return true;
        }
        
        public bool EstablishTradeAgreement(int faction1Id, int faction2Id, TradeTerm[] terms)
        {
            if (!factionStates.TryGetValue(faction1Id, out var state1) ||
                !factionStates.TryGetValue(faction2Id, out var state2))
                return false;
            
            if (state1.relationshipState == RelationshipState.Hostile ||
                state2.relationshipState == RelationshipState.Hostile)
                return false;
            
            // Add trade agreement
            state1.tradeAgreements = AddToArray(state1.tradeAgreements, faction2Id);
            state2.tradeAgreements = AddToArray(state2.tradeAgreements, faction1Id);
            
            factionStates[faction1Id] = state1;
            factionStates[faction2Id] = state2;
            
            return true;
        }
        
        public float GetReputation(int factionId)
        {
            return factionStates.TryGetValue(factionId, out var state) ? state.reputation : 0;
        }
        
        public RelationshipState GetRelationshipState(int factionId)
        {
            return factionStates.TryGetValue(factionId, out var state) ? state.relationshipState : RelationshipState.Neutral;
        }
        
        public bool IsAtWar(int faction1Id, int faction2Id)
        {
            if (!factionStates.TryGetValue(faction1Id, out var state)) return false;
            return Array.IndexOf(state.activeWars, faction2Id) >= 0;
        }
        
        public bool HasTradeAgreement(int faction1Id, int faction2Id)
        {
            if (!factionStates.TryGetValue(faction1Id, out var state)) return false;
            return Array.IndexOf(state.tradeAgreements, faction2Id) >= 0;
        }
        
        public FactionData GetFactionData(int factionId)
        {
            foreach (var faction in allFactions)
            {
                if (faction.id == factionId) return faction;
            }
            return default;
        }
        
        private T[] AddToArray<T>(T[] array, T item)
        {
            var newArray = new T[array.Length + 1];
            Array.Copy(array, newArray, array.Length);
            newArray[array.Length] = item;
            return newArray;
        }
        
        private T[] RemoveFromArray<T>(T[] array, T item)
        {
            var list = new System.Collections.Generic.List<T>(array);
            list.Remove(item);
            return list.ToArray();
        }
        
        private void Update()
        {
            // Decay relationships over time
            foreach (var kvp in factionStates)
            {
                var state = kvp.Value;
                if (state.relationshipState == RelationshipState.Neutral)
                {
                    state.reputation = Mathf.MoveTowards(state.reputation, 0, relationshipDecayRate * Time.deltaTime);
                    factionStates[kvp.Key] = state;
                }
            }
        }
    }
    
    [Serializable]
    public struct FactionData
    {
        public int id;
        public string name;
        public string description;
        public Color factionColor;
        public float startingReputation;
        public FactionType factionType;
        public string leaderName;
        public string capitalLocation;
        public string ideology;
        public int[] alliedFactions;
        public int[] enemyFactions;
        public string[] specializations;
    }
    
    [Serializable]
    public struct FactionState
    {
        public int factionId;
        public float reputation;
        public RelationshipState relationshipState;
        public int[] tradeAgreements;
        public int[] activeWars;
        public int[] embassies;
        public float lastInteractionTime;
        public int completedQuests;
        public int totalTradedValue;
    }
    
    public enum RelationshipState
    {
        Allied,      // 500+ rep
        Friendly,    // 200-499 rep
        Neutral,     // -199 to 199 rep
        Unfriendly,  // -200 to -499 rep
        Hostile      // -500 or less rep
    }
    
    public enum FactionType
    {
        Government,
        Corporation,
        Rebel,
        Religious,
        Scientific,
        Criminal,
        Mercenary,
        Tribal
    }
    
    public enum ReputationReason
    {
        QuestCompletion,
        Trade,
        CombatAssist,
        TerritoryViolation,
        Theft,
        Murder,
        Gift,
        Treaty,
        SharedEnemy,
        Betrayal
    }
    
    [Serializable]
    public struct TradeTerm
    {
        public int resourceId;
        public int quantity;
        public float priceModifier;
        public bool isExclusive;
        public float duration;
    }
    
    // Event structs
    public struct FactionReputationChanged
    {
        public int factionId;
        public float oldReputation;
        public float newReputation;
        public ReputationReason reason;
    }
    
    public struct FactionRelationChanged
    {
        public int factionId;
        public RelationshipState oldState;
        public RelationshipState newState;
    }
    
    public struct FactionWarDeclared
    {
        public int aggressorId;
        public int targetId;
    }
    
    public struct FactionPeaceSigned
    {
        public int faction1Id;
        public int faction2Id;
        public int[] terms;
    }
}
