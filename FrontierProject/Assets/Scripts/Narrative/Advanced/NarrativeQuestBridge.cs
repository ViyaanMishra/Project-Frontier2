using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.Advanced
{
    /// <summary>
    /// Bridges the narrative system with the quest system.
    /// Automatically generates quests from story nodes and tracks narrative-driven objectives.
    /// </summary>
    public class NarrativeQuestBridge : IService
    {
        private NativeHashMap<FixedString64Bytes, QuestMapping> _questMappings;
        private NativeList<FixedString64Bytes> _activeNarrativeQuests;
        
        public int Priority => 10;

        public void Initialize()
        {
            _questMappings = new NativeHashMap<FixedString64Bytes, QuestMapping>(64, Allocator.Persistent);
            _activeNarrativeQuests = new NativeList<FixedString64Bytes>(Allocator.Persistent);
            
            EventBus.Subscribe<StoryNodeExecutedEvent>(OnStoryNodeExecuted);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
        }

        public void Tick(double deltaTime)
        {
            // Sync narrative state with quest progress
            SyncQuestProgress();
        }

        public void Shutdown()
        {
            if (_questMappings.IsCreated) _questMappings.Dispose();
            if (_activeNarrativeQuests.IsCreated) _activeNarrativeQuests.Dispose();
        }

        /// <summary>
        /// Registers a mapping between a story node and a quest.
        /// </summary>
        public void RegisterMapping(FixedString64Bytes nodeId, FixedString64Bytes questId, MappingType type)
        {
            var mapping = new QuestMapping
            {
                NodeId = nodeId,
                QuestId = questId,
                Type = type,
                IsTriggered = false,
                IsCompleted = false
            };
            _questMappings[nodeId] = mapping;
        }

        /// <summary>
        /// Called when a story node is executed.
        /// </summary>
        private void OnStoryNodeExecuted(StoryNodeExecutedEvent evt)
        {
            if (_questMappings.TryGetValue(evt.NodeId, out var mapping))
            {
                switch (mapping.Type)
                {
                    case MappingType.TriggerQuest:
                        TriggerQuest(mapping.QuestId);
                        break;
                    case MappingType.CompleteQuest:
                        CompleteQuest(mapping.QuestId);
                        break;
                    case MappingType.UpdateObjective:
                        UpdateQuestObjective(mapping.QuestId, evt.NodeId);
                        break;
                }
                
                mapping.IsTriggered = true;
                _questMappings[evt.NodeId] = mapping;
            }
        }

        /// <summary>
        /// Called when a quest is completed.
        /// </summary>
        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            // Find any mappings for this quest and update narrative
            var enumerator = _questMappings.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.QuestId == evt.QuestId)
                {
                    var mapping = enumerator.Current.Value;
                    mapping.IsCompleted = true;
                    _questMappings[enumerator.Current.Key] = mapping;
                    
                    // Trigger follow-up narrative
                    var engine = ServiceRegistry.Get<StoryGraphEngine>();
                    // Could trigger next node in sequence
                }
            }
        }

        /// <summary>
        /// Triggers a quest from narrative.
        /// </summary>
        private void TriggerQuest(FixedString64Bytes questId)
        {
            EventBus.Publish(new NarrativeTriggerQuestEvent { QuestId = questId });
            _activeNarrativeQuests.Add(questId);
        }

        /// <summary>
        /// Marks a quest as complete from narrative.
        /// </summary>
        private void CompleteQuest(FixedString64Bytes questId)
        {
            EventBus.Publish(new NarrativeCompleteQuestEvent { QuestId = questId });
            
            for (int i = 0; i < _activeNarrativeQuests.Length; i++)
            {
                if (_activeNarrativeQuests[i] == questId)
                {
                    _activeNarrativeQuests.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates a quest objective based on narrative progress.
        /// </summary>
        private void UpdateQuestObjective(FixedString64Bytes questId, FixedString64Bytes nodeId)
        {
            EventBus.Publish(new NarrativeUpdateQuestEvent 
            { 
                QuestId = questId,
                CompletedNodeId = nodeId
            });
        }

        /// <summary>
        /// Syncs quest progress with narrative state.
        /// </summary>
        private void SyncQuestProgress()
        {
            // Periodic sync to ensure consistency
        }
    }

    [Serializable]
    public struct QuestMapping
    {
        public FixedString64Bytes NodeId;
        public FixedString64Bytes QuestId;
        public MappingType Type;
        public bool IsTriggered;
        public bool IsCompleted;
    }

    public enum MappingType
    {
        TriggerQuest,      // Node execution starts quest
        CompleteQuest,     // Node execution completes quest
        UpdateObjective,   // Node execution updates quest objective
        OptionalQuest      // Node makes quest available optionally
    }

    #region Events
    public struct NarrativeTriggerQuestEvent : IEvent
    {
        public FixedString64Bytes QuestId;
    }

    public struct NarrativeCompleteQuestEvent : IEvent
    {
        public FixedString64Bytes QuestId;
    }

    public struct NarrativeUpdateQuestEvent : IEvent
    {
        public FixedString64Bytes QuestId;
        public FixedString64Bytes CompletedNodeId;
    }

    public struct QuestCompletedEvent : IEvent
    {
        public FixedString64Bytes QuestId;
    }
    #endregion
}
