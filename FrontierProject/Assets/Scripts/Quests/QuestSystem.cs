using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frontier.Quests
{
    /// <summary>
    /// Quest objective types.
    /// </summary>
    public enum ObjectiveType
    {
        Kill,
        Collect,
        Deliver,
        Explore,
        Talk,
        Build,
        Escort,
        Defend,
        Survive,
        Craft
    }

    /// <summary>
    /// Single quest objective.
    /// </summary>
    [Serializable]
    public struct QuestObjective
    {
        public string Id;
        public ObjectiveType Type;
        public string TargetId;       // Entity/item ID to interact with
        public int TargetAmount;      // Required amount
        public int CurrentAmount;     // Progress
        public bool Completed;
        public string Description;
        public Vector3? Location;     // Optional marker location
    }

    /// <summary>
    /// Quest reward definition.
    /// </summary>
    [Serializable]
    public struct QuestReward
    {
        public string ItemId;
        public int Amount;
        public float Experience;
        public float FactionReputation;
        public string UnlockId;       // Recipe, area, etc.
    }

    /// <summary>
    /// Quest state tracking.
    /// </summary>
    public enum QuestState
    {
        Available,
        Active,
        Completed,
        Failed,
        TurnedIn
    }

    /// <summary>
    /// Main quest system managing storyline, side quests, and dynamic events.
    /// </summary>
    public class QuestSystem
    {
        private Dictionary<string, QuestData> _questTemplates = new Dictionary<string, QuestData>();
        private Dictionary<string, QuestInstance> _activeQuests = new Dictionary<string, QuestInstance>();
        private List<string> _completedQuestIds = new List<string>();
        
        // Event callbacks
        public event Action<QuestInstance> OnQuestStarted;
        public event Action<QuestInstance, QuestObjective> OnObjectiveUpdated;
        public event Action<QuestInstance> OnQuestCompleted;
        public event Action<QuestInstance> OnQuestFailed;

        /// <summary>
        /// Register a quest template.
        /// </summary>
        public void RegisterQuest(QuestData questData)
        {
            if (!_questTemplates.ContainsKey(questData.Id))
                _questTemplates[questData.Id] = questData;
        }

        /// <summary>
        /// Start a quest for the player.
        /// </summary>
        public QuestInstance StartQuest(string questId, string instanceId = null)
        {
            if (!_questTemplates.ContainsKey(questId))
            {
                Debug.LogError($"[QuestSystem] Quest template not found: {questId}");
                return null;
            }

            if (IsQuestActive(questId))
            {
                Debug.LogWarning($"[QuestSystem] Quest already active: {questId}");
                return null;
            }

            var template = _questTemplates[questId];
            
            // Check prerequisites
            foreach (var prereq in template.Prerequisites)
            {
                if (!_completedQuestIds.Contains(prereq))
                {
                    Debug.LogWarning($"[QuestSystem] Prerequisite not met: {prereq}");
                    return null;
                }
            }

            string id = instanceId ?? $"{questId}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            
            var instance = new QuestInstance
            {
                Id = id,
                QuestTemplateId = questId,
                Title = template.Title,
                Description = template.Description,
                State = QuestState.Active,
                Objectives = new List<QuestObjective>(),
                Rewards = template.Rewards,
                StartTime = DateTime.Now
            };

            // Initialize objectives
            foreach (var obj in template.Objectives)
            {
                instance.Objectives.Add(new QuestObjective
                {
                    Id = obj.Id,
                    Type = obj.Type,
                    TargetId = obj.TargetId,
                    TargetAmount = obj.TargetAmount,
                    CurrentAmount = 0,
                    Completed = false,
                    Description = obj.Description,
                    Location = obj.Location
                });
            }

            _activeQuests[id] = instance;
            OnQuestStarted?.Invoke(instance);

            Debug.Log($"[QuestSystem] Started quest: {template.Title}");
            return instance;
        }

        /// <summary>
        /// Update progress on an objective.
        /// </summary>
        public void UpdateObjective(string questId, string objectiveId, int amount)
        {
            var instance = GetActiveQuest(questId);
            if (instance == null) return;

            for (int i = 0; i < instance.Objectives.Count; i++)
            {
                var obj = instance.Objectives[i];
                if (obj.Id == objectiveId)
                {
                    obj.CurrentAmount = Mathf.Min(obj.CurrentAmount + amount, obj.TargetAmount);
                    
                    if (obj.CurrentAmount >= obj.TargetAmount)
                        obj.Completed = true;

                    instance.Objectives[i] = obj;
                    OnObjectiveUpdated?.Invoke(instance, obj);

                    // Check if all objectives complete
                    if (AreAllObjectivesComplete(instance))
                    {
                        CompleteQuest(instance.Id);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Set objective progress directly.
        /// </summary>
        public void SetObjectiveProgress(string questId, string objectiveId, int progress)
        {
            var instance = GetActiveQuest(questId);
            if (instance == null) return;

            for (int i = 0; i < instance.Objectives.Count; i++)
            {
                var obj = instance.Objectives[i];
                if (obj.Id == objectiveId)
                {
                    obj.CurrentAmount = Mathf.Clamp(progress, 0, obj.TargetAmount);
                    obj.Completed = obj.CurrentAmount >= obj.TargetAmount;
                    instance.Objectives[i] = obj;
                    OnObjectiveUpdated?.Invoke(instance, obj);

                    if (AreAllObjectivesComplete(instance))
                        CompleteQuest(instance.Id);

                    break;
                }
            }
        }

        /// <summary>
        /// Complete a quest.
        /// </summary>
        public void CompleteQuest(string questInstanceId)
        {
            if (!_activeQuests.ContainsKey(questInstanceId))
                return;

            var instance = _activeQuests[questInstanceId];
            instance.State = QuestState.Completed;
            _activeQuests[questInstanceId] = instance;

            _completedQuestIds.Add(instance.QuestTemplateId);
            OnQuestCompleted?.Invoke(instance);

            Debug.Log($"[QuestSystem] Completed quest: {instance.Title}");
        }

        /// <summary>
        /// Turn in a completed quest and grant rewards.
        /// </summary>
        public void TurnInQuest(string questInstanceId, Action<List<QuestReward>> onRewardsGranted)
        {
            if (!_activeQuests.ContainsKey(questInstanceId))
                return;

            var instance = _activeQuests[questInstanceId];
            if (instance.State != QuestState.Completed)
            {
                Debug.LogWarning($"[QuestSystem] Cannot turn in incomplete quest: {instance.Title}");
                return;
            }

            instance.State = QuestState.TurnedIn;
            _activeQuests[questInstanceId] = instance;
            _activeQuests.Remove(questInstanceId);

            onRewardsGranted?.Invoke(instance.Rewards);
            Debug.Log($"[QuestSystem] Turned in quest: {instance.Title}");
        }

        /// <summary>
        /// Fail a quest.
        /// </summary>
        public void FailQuest(string questInstanceId, string reason = "")
        {
            if (!_activeQuests.ContainsKey(questInstanceId))
                return;

            var instance = _activeQuests[questInstanceId];
            instance.State = QuestState.Failed;
            _activeQuests[questInstanceId] = instance;
            _activeQuests.Remove(questInstanceId);

            OnQuestFailed?.Invoke(instance);
            Debug.LogWarning($"[QuestSystem] Failed quest: {instance.Title} - {reason}");
        }

        /// <summary>
        /// Get an active quest by instance ID.
        /// </summary>
        public QuestInstance GetActiveQuest(string questInstanceId)
        {
            return _activeQuests.TryGetValue(questInstanceId, out var instance) ? instance : null;
        }

        /// <summary>
        /// Get all active quests.
        /// </summary>
        public List<QuestInstance> GetAllActiveQuests()
        {
            return new List<QuestInstance>(_activeQuests.Values);
        }

        /// <summary>
        /// Check if a quest template is currently active.
        /// </summary>
        public bool IsQuestActive(string questTemplateId)
        {
            foreach (var kvp in _activeQuests)
            {
                if (kvp.Value.QuestTemplateId == questTemplateId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a quest template has been completed.
        /// </summary>
        public bool IsQuestCompleted(string questTemplateId)
        {
            return _completedQuestIds.Contains(questTemplateId);
        }

        /// <summary>
        /// Generate procedural side quest.
        /// </summary>
        public QuestData GenerateProceduralQuest(string type, string factionId, int difficulty)
        {
            var quest = new QuestData();
            
            switch (type)
            {
                case "bounty":
                    quest.Id = $"bounty_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    quest.Title = $"Bounty: Target {difficulty}";
                    quest.Description = $"Eliminate the designated target for {factionId}.";
                    quest.Objectives = new List<QuestObjective>
                    {
                        new QuestObjective
                        {
                            Id = "kill_target",
                            Type = ObjectiveType.Kill,
                            TargetAmount = 1,
                            Description = "Eliminate the target"
                        }
                    };
                    quest.Rewards = new List<QuestReward>
                    {
                        new QuestReward { Experience = 100 * difficulty, FactionReputation = 50 * difficulty }
                    };
                    break;

                case "escort":
                    quest.Id = $"escort_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    quest.Title = $"Escort Mission";
                    quest.Description = $"Safely escort the convoy to their destination.";
                    quest.Objectives = new List<QuestObjective>
                    {
                        new QuestObjective
                        {
                            Id = "escort_complete",
                            Type = ObjectiveType.Escort,
                            TargetAmount = 1,
                            Description = "Reach the destination"
                        }
                    };
                    quest.Rewards = new List<QuestReward>
                    {
                        new QuestReward { Experience = 150 * difficulty, FactionReputation = 75 * difficulty }
                    };
                    break;

                case "fetch":
                    quest.Id = $"fetch_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                    quest.Title = $"Resource Run";
                    quest.Description = $"Gather and deliver the requested supplies.";
                    quest.Objectives = new List<QuestObjective>
                    {
                        new QuestObjective
                        {
                            Id = "collect_resources",
                            Type = ObjectiveType.Collect,
                            TargetAmount = 10 * difficulty,
                            Description = "Collect resources"
                        },
                        new QuestObjective
                        {
                            Id = "deliver_resources",
                            Type = ObjectiveType.Deliver,
                            TargetAmount = 1,
                            Description = "Deliver to outpost"
                        }
                    };
                    quest.Rewards = new List<QuestReward>
                    {
                        new QuestReward { Experience = 80 * difficulty, FactionReputation = 40 * difficulty }
                    };
                    break;
            }

            quest.FactionId = factionId;
            quest.Difficulty = difficulty;
            quest.IsProcedural = true;

            return quest;
        }

        private bool AreAllObjectivesComplete(QuestInstance instance)
        {
            foreach (var obj in instance.Objectives)
            {
                if (!obj.Completed)
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Runtime quest instance.
    /// </summary>
    [Serializable]
    public class QuestInstance
    {
        public string Id;
        public string QuestTemplateId;
        public string Title;
        public string Description;
        public QuestState State;
        public List<QuestObjective> Objectives;
        public List<QuestReward> Rewards;
        public DateTime StartTime;
        public DateTime? CompletedTime;
    }
}
