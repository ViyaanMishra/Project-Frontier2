using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.StoryGraph
{
    /// <summary>
    /// Advanced condition types for complex narrative branching logic.
    /// Supports compound conditions, probability checks, and world state queries.
    /// </summary>
    
    /// <summary>
    /// Checks if a specific variable equals a target value.
    /// </summary>
    [Serializable]
    public class VariableEqualsCondition : StoryCondition
    {
        public FixedString64Bytes VariableName;
        public object TargetValue;

        public override bool Evaluate(StoryVariableStore store)
        {
            if (!store.HasVariable(VariableName))
                return false;
            
            var currentValue = store.GetVariable<object>(VariableName);
            return Equals(currentValue, TargetValue);
        }
    }

    /// <summary>
    /// Checks if a numeric variable is greater than a threshold.
    /// </summary>
    [Serializable]
    public class VariableGreaterThanCondition : StoryCondition
    {
        public FixedString64Bytes VariableName;
        public float Threshold;

        public override bool Evaluate(StoryVariableStore store)
        {
            if (!store.HasVariable(VariableName))
                return false;
            
            var value = store.GetVariable<float>(VariableName);
            return value > Threshold;
        }
    }

    /// <summary>
    /// Logical AND of multiple conditions.
    /// </summary>
    [Serializable]
    public class AndCondition : StoryCondition
    {
        public NativeArray<FixedString64Bytes> ConditionIds;

        public override bool Evaluate(StoryVariableStore store)
        {
            for (int i = 0; i < ConditionIds.Length; i++)
            {
                if (!store.EvaluateCondition(ConditionIds[i]))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Logical OR of multiple conditions.
    /// </summary>
    [Serializable]
    public class OrCondition : StoryCondition
    {
        public NativeArray<FixedString64Bytes> ConditionIds;

        public override bool Evaluate(StoryVariableStore store)
        {
            for (int i = 0; i < ConditionIds.Length; i++)
            {
                if (store.EvaluateCondition(ConditionIds[i]))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Logical NOT of a single condition.
    /// </summary>
    [Serializable]
    public class NotCondition : StoryCondition
    {
        public FixedString64Bytes ConditionId;

        public override bool Evaluate(StoryVariableStore store)
        {
            return !store.EvaluateCondition(ConditionId);
        }
    }

    /// <summary>
    /// Condition that passes with a certain probability.
    /// Useful for random narrative branches.
    /// </summary>
    [Serializable]
    public class ProbabilityCondition : StoryCondition
    {
        public float Probability; // 0.0 to 1.0
        public int Seed;

        public override bool Evaluate(StoryVariableStore store)
        {
            var rng = new Random(Xorshift32.SeedFromTime() ^ Seed);
            return rng.NextFloat() < Probability;
        }
    }

    /// <summary>
    /// Checks if a specific story node has been completed.
    /// </summary>
    [Serializable]
    public class NodeCompletedCondition : StoryCondition
    {
        public FixedString64Bytes NodeId;

        public override bool Evaluate(StoryVariableStore store)
        {
            var engine = ServiceRegistry.Get<StoryGraphEngine>();
            var node = engine.GetNode(NodeId);
            return node.IsCompleted;
        }
    }

    /// <summary>
    /// Checks the current game time against a range.
    /// </summary>
    [Serializable]
    public class TimeRangeCondition : StoryCondition
    {
        public double StartTicks;
        public double EndTicks;

        public override bool Evaluate(StoryVariableStore store)
        {
            var currentTime = MasterClock.Instance.TotalTicks;
            return currentTime >= StartTicks && currentTime <= EndTicks;
        }
    }

    /// <summary>
    /// Checks if the player is in a specific biome or location.
    /// </summary>
    [Serializable]
    public class LocationCondition : StoryCondition
    {
        public FixedString64Bytes RequiredBiome;
        public float Radius;

        public override bool Evaluate(StoryVariableStore store)
        {
            // Query world simulation for player location
            // This would integrate with the WorldGen system
            var playerPos = GetPlayerPosition();
            var biome = BiomeSystem.GetBiomeAt(playerPos);
            return biome.Id == RequiredBiome;
        }

        private UnityEngine.Vector3 GetPlayerPosition()
        {
            // Placeholder - would query ECS player entity
            return UnityEngine.Vector3.zero;
        }
    }

    /// <summary>
    /// Checks relationship level between characters.
    /// </summary>
    [Serializable]
    public class RelationshipCondition : StoryCondition
    {
        public FixedString64Bytes CharacterA;
        public FixedString64Bytes CharacterB;
        public float MinRelationship;

        public override bool Evaluate(StoryVariableStore store)
        {
            var relationship = GetRelationship(CharacterA, CharacterB);
            return relationship >= MinRelationship;
        }

        private float GetRelationship(FixedString64Bytes a, FixedString64Bytes b)
        {
            // Query social simulation system
            return 0.5f; // Placeholder
        }
    }

    /// <summary>
    /// Composite condition builder for fluent API usage.
    /// </summary>
    public class ConditionBuilder
    {
        private NativeList<FixedString64Bytes> _andConditions;
        private NativeList<FixedString64Bytes> _orConditions;
        private FixedString64Bytes _notCondition;

        public ConditionBuilder()
        {
            _andConditions = new NativeList<FixedString64Bytes>(Allocator.Temp);
            _orConditions = new NativeList<FixedString64Bytes>(Allocator.Temp);
        }

        public ConditionBuilder And(FixedString64Bytes conditionId)
        {
            _andConditions.Add(conditionId);
            return this;
        }

        public ConditionBuilder Or(FixedString64Bytes conditionId)
        {
            _orConditions.Add(conditionId);
            return this;
        }

        public ConditionBuilder Not(FixedString64Bytes conditionId)
        {
            _notCondition = conditionId;
            return this;
        }

        public FixedString64Bytes Build(StoryVariableStore store, string name)
        {
            var id = new FixedString64Bytes(name);
            
            if (_andConditions.Length > 1)
            {
                var andCond = new AndCondition 
                { 
                    ConditionIds = _andConditions.ToArray(Allocator.Persistent) 
                };
                store.RegisterCondition(new FixedString64Bytes($"{name}_AND"), andCond);
            }

            if (_orConditions.Length > 1)
            {
                var orCond = new OrCondition 
                { 
                    ConditionIds = _orConditions.ToArray(Allocator.Persistent) 
                };
                store.RegisterCondition(new FixedString64Bytes($"{name}_OR"), orCond);
            }

            return id;
        }

        public void Dispose()
        {
            if (_andConditions.IsCreated) _andConditions.Dispose();
            if (_orConditions.IsCreated) _orConditions.Dispose();
        }
    }
}
