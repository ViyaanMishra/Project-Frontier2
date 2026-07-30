using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.StoryGraph
{
    /// <summary>
    /// Advanced effect types for modifying game state during narrative execution.
    /// Supports variable manipulation, event triggering, and system integration.
    /// </summary>

    /// <summary>
    /// Sets a variable to a specific value.
    /// </summary>
    [Serializable]
    public class SetVariableEffect : StoryEffect
    {
        public FixedString64Bytes VariableName;
        public object Value;
        public int DurationTicks;

        public override void Execute(StoryVariableStore store)
        {
            store.SetVariable(VariableName, Value, DurationTicks);
        }
    }

    /// <summary>
    /// Increments a numeric variable by a specified amount.
    /// </summary>
    [Serializable]
    public class IncrementVariableEffect : StoryEffect
    {
        public FixedString64Bytes VariableName;
        public float Amount;

        public override void Execute(StoryVariableStore store)
        {
            var current = store.GetVariable<float>(VariableName);
            store.SetVariable(VariableName, current + Amount);
        }
    }

    /// <summary>
    /// Decrements a numeric variable by a specified amount.
    /// </summary>
    [Serializable]
    public class DecrementVariableEffect : StoryEffect
    {
        public FixedString64Bytes VariableName;
        public float Amount;

        public override void Execute(StoryVariableStore store)
        {
            var current = store.GetVariable<float>(VariableName);
            store.SetVariable(VariableName, current - Amount);
        }
    }

    /// <summary>
    /// Toggles a boolean variable.
    /// </summary>
    [Serializable]
    public class ToggleVariableEffect : StoryEffect
    {
        public FixedString64Bytes VariableName;

        public override void Execute(StoryVariableStore store)
        {
            var current = store.GetVariable<bool>(VariableName);
            store.SetVariable(VariableName, !current);
        }
    }

    /// <summary>
    /// Triggers a global event that other systems can respond to.
    /// </summary>
    [Serializable]
    public class TriggerEventEffect : StoryEffect
    {
        public FixedString128Bytes EventName;
        public object Payload;

        public override void Execute(StoryVariableStore store)
        {
            EventBus.Publish(new NarrativeTriggerEvent 
            { 
                EventName = EventName, 
                Payload = Payload 
            });
        }
    }

    /// <summary>
    /// Starts another story node, creating a chain or sub-quest.
    /// </summary>
    [Serializable]
    public class StartNodeEffect : StoryEffect
    {
        public FixedString64Bytes TargetNodeId;

        public override void Execute(StoryVariableStore store)
        {
            var engine = ServiceRegistry.Get<StoryGraphEngine>();
            engine.StartNode(TargetNodeId);
        }
    }

    /// <summary>
    /// Unlocks a dialogue option or interaction for the player.
    /// </summary>
    [Serializable]
    public class UnlockDialogueEffect : StoryEffect
    {
        public FixedString64Bytes DialogueId;
        public FixedString64Bytes CharacterId;

        public override void Execute(StoryVariableStore store)
        {
            store.SetVariable(new FixedString64Bytes($"unlocked_{DialogueId}"), true);
        }
    }

    /// <summary>
    /// Modifies faction reputation.
    /// </summary>
    [Serializable]
    public class ModifyReputationEffect : StoryEffect
    {
        public FixedString64Bytes FactionId;
        public float Amount;

        public override void Execute(StoryVariableStore store)
        {
            var repVar = new FixedString64Bytes($"rep_{FactionId}");
            var current = store.GetVariable<float>(repVar);
            store.SetVariable(repVar, current + Amount);
        }
    }

    /// <summary>
    /// Adds an item to the player's inventory.
    /// Integrates with the Items system.
    /// </summary>
    [Serializable]
    public class AddItemEffect : StoryEffect
    {
        public FixedString64Bytes ItemId;
        public int Quantity;

        public override void Execute(StoryVariableStore store)
        {
            // Would integrate with InventorySystem
            UnityEngine.Debug.Log($"[Narrative] Adding {Quantity}x {ItemId} to inventory");
        }
    }

    /// <summary>
    /// Removes an item from the player's inventory.
    /// </summary>
    [Serializable]
    public class RemoveItemEffect : StoryEffect
    {
        public FixedString64Bytes ItemId;
        public int Quantity;

        public override void Execute(StoryVariableStore store)
        {
            // Would integrate with InventorySystem
            UnityEngine.Debug.Log($"[Narrative] Removing {Quantity}x {ItemId} from inventory");
        }
    }

    /// <summary>
    /// Spawns an entity in the world.
    /// Integrates with the ECS simulation.
    /// </summary>
    [Serializable]
    public class SpawnEntityEffect : StoryEffect
    {
        public FixedString64Bytes EntityPrefab;
        public UnityEngine.Vector3 Position;
        public UnityEngine.Quaternion Rotation;

        public override void Execute(StoryVariableStore store)
        {
            // Would integrate with EntityManager
            UnityEngine.Debug.Log($"[Narrative] Spawning {EntityPrefab} at {Position}");
        }
    }

    /// <summary>
    /// Teleports the player to a specific location.
    /// </summary>
    [Serializable]
    public class TeleportPlayerEffect : StoryEffect
    {
        public UnityEngine.Vector3 TargetPosition;
        public FixedString64Bytes TargetScene;

        public override void Execute(StoryVariableStore store)
        {
            // Would integrate with PlayerController
            UnityEngine.Debug.Log($"[Narrative] Teleporting player to {TargetPosition} in {TargetScene}");
        }
    }

    /// <summary>
    /// Plays a cutscene or cinematic sequence.
    /// </summary>
    [Serializable]
    public class PlayCutsceneEffect : StoryEffect
    {
        public FixedString64Bytes CutsceneId;
        public bool Skipable;

        public override void Execute(StoryVariableStore store)
        {
            EventBus.Publish(new CutsceneRequestEvent 
            { 
                CutsceneId = CutsceneId, 
                Skipable = Skipable 
            });
        }
    }

    /// <summary>
    /// Modifies the weather or time of day.
    /// Integrates with Environment system.
    /// </summary>
    [Serializable]
    public class SetEnvironmentEffect : StoryEffect
    {
        public FixedString64Bytes WeatherType;
        public float TimeOfDay;
        public int DurationTicks;

        public override void Execute(StoryVariableStore store)
        {
            EventBus.Publish(new EnvironmentChangeRequestEvent
            {
                WeatherType = WeatherType,
                TimeOfDay = TimeOfDay,
                DurationTicks = DurationTicks
            });
        }
    }

    /// <summary>
    /// Pushes a new variable scope for temporary state.
    /// </summary>
    [Serializable]
    public class PushScopeEffect : StoryEffect
    {
        public FixedString64Bytes ScopeName;

        public override void Execute(StoryVariableStore store)
        {
            store.PushScope(ScopeName);
        }
    }

    /// <summary>
    /// Pops the current variable scope.
    /// </summary>
    [Serializable]
    public class PopScopeEffect : StoryEffect
    {
        public override void Execute(StoryVariableStore store)
        {
            store.PopScope();
        }
    }

    /// <summary>
    /// Composite effect that executes multiple effects in sequence.
    /// </summary>
    [Serializable]
    public class CompoundEffect : StoryEffect
    {
        public NativeArray<FixedString64Bytes> EffectIds;

        public override void Execute(StoryVariableStore store)
        {
            for (int i = 0; i < EffectIds.Length; i++)
            {
                store.ExecuteEffect(EffectIds[i]);
            }
        }
    }

    #region Events
    public struct NarrativeTriggerEvent : IEvent
    {
        public FixedString128Bytes EventName;
        public object Payload;
    }

    public struct CutsceneRequestEvent : IEvent
    {
        public FixedString64Bytes CutsceneId;
        public bool Skipable;
    }

    public struct EnvironmentChangeRequestEvent : IEvent
    {
        public FixedString64Bytes WeatherType;
        public float TimeOfDay;
        public int DurationTicks;
    }
    #endregion
}
