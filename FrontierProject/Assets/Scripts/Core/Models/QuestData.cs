using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Quest data structure for main storyline, procedural side quests, and faction questlines.
    /// </summary>
    [Serializable]
    public struct QuestData
    {
        public EntityGUID guid;
        public int questId;
        public string questName;
        public string description;
        public QuestType questType;
        public QuestCategory category;
        
        // Story progression
        public int chapterNumber; // For main quest (1-12)
        public int factionId;     // For faction quests (-1 if none)
        public bool isMainStory;
        public bool isRepeatable;
        
        // Prerequisites
        public int[] prerequisiteQuestIds;
        public int minimumLevel;
        public float minimumFactionReputation;
        public int[] requiredTechIds;
        
        // Objectives (up to 5 per quest)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public QuestObjective[] objectives;
        
        // Rewards
        public int experienceReward;
        public int[] itemRewards;
        public int[] itemRewardQuantities;
        public float currencyReward;
        public int factionReputationReward;
        public int unlockRecipeId;
        public int unlockTechId;
        public bool grantsTitle;
        public string titleGranted;
        
        // Failure conditions
        public bool canFail;
        public float timeLimit; // 0 if no limit
        public int[] failureConditions;
        
        // Dialogue
        public int dialogueTreeId;
        public int giverNPCId;
        public int turnInNPCId;
        
        // State tracking
        public QuestState currentState;
        public int currentObjectiveIndex;
        public float startTime;
        public float completionTime;
        
        public void Initialize(int id, string name, QuestType type, QuestCategory cat)
        {
            questId = id;
            questName = name;
            questType = type;
            category = cat;
            objectives = new QuestObjective[5];
            prerequisiteQuestIds = new int[0];
            requiredTechIds = new int[0];
            itemRewards = new int[0];
            itemRewardQuantities = new int[0];
            failureConditions = new int[0];
            isMainStory = false;
            isRepeatable = false;
            canFail = false;
            currentState = QuestState.NotStarted;
        }
        
        public bool IsAvailable(int playerLevel, float factionRep, int[] completedQuests, int[] ownedTechs)
        {
            if (currentState != QuestState.NotStarted && currentState != QuestState.Failed)
                return false;
                
            if (playerLevel < minimumLevel) return false;
            if (factionRep < minimumFactionReputation) return false;
            
            // Check prerequisites
            foreach (int prereqId in prerequisiteQuestIds)
            {
                bool found = false;
                foreach (int completed in completedQuests)
                {
                    if (completed == prereqId) { found = true; break; }
                }
                if (!found) return false;
            }
            
            // Check tech requirements
            foreach (int techId in requiredTechIds)
            {
                bool found = false;
                foreach (int owned in ownedTechs)
                {
                    if (owned == techId) { found = true; break; }
                }
                if (!found) return false;
            }
            
            return true;
        }
        
        public void UpdateObjectives(QuestObjectiveType targetType, int targetId, int currentValue)
        {
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].objectiveType == targetType && 
                    objectives[i].targetId == targetId &&
                    !objectives[i].isCompleted)
                {
                    objectives[i].currentValue = Mathf.Min(currentValue, objectives[i].targetValue);
                    if (objectives[i].currentValue >= objectives[i].targetValue)
                    {
                        objectives[i].isCompleted = true;
                        OnObjectiveCompleted(i);
                    }
                    break;
                }
            }
        }
        
        private void OnObjectiveCompleted(int objectiveIndex)
        {
            currentObjectiveIndex = objectiveIndex + 1;
            
            // Check if all objectives complete
            bool allComplete = true;
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].targetValue > 0 && !objectives[i].isCompleted)
                {
                    allComplete = false;
                    break;
                }
            }
            
            if (allComplete)
            {
                currentState = QuestState.Completed;
                completionTime = UnityEngine.Time.time;
            }
        }
        
        public bool IsComplete()
        {
            return currentState == QuestState.Completed;
        }
        
        public bool HasFailed(float currentTime)
        {
            if (!canFail) return false;
            if (timeLimit <= 0) return false;
            return (currentTime - startTime) > timeLimit;
        }
    }
    
    [Serializable]
    public struct QuestObjective
    {
        public QuestObjectiveType objectiveType;
        public int targetId; // Item ID, NPC ID, Location ID, etc.
        public int targetValue; // Required amount
        public int currentValue; // Current progress
        public bool isCompleted;
        public string customDescription;
        public Vector3 locationHint;
        public int optionalLinkId; // Link to optional sub-objective
    }
    
    public enum QuestObjectiveType
    {
        None,
        KillEntity,           // targetId = entity type
        CollectItem,          // targetId = item ID
        DeliverItem,          // targetId = item ID
        ReachLocation,        // targetId = location ID
        TalkToNPC,            // targetId = NPC ID
        EscortNPC,            // targetId = NPC ID
        DefendLocation,       // targetId = location ID
        DestroyBuilding,      // targetId = building type
        BuildStructure,       // targetId = building type
        CraftItem,            // targetId = item ID
        ResearchTech,         // targetId = tech ID
        SurviveTime,          // targetValue = seconds
        TravelDistance,       // targetValue = meters
        DiscoverArea,         // targetId = area ID
        EliminateFaction,     // targetId = faction ID
        GainReputation,       // targetId = faction ID, value = rep amount
        TradeItem,            // targetId = item ID
        HackTerminal,         // targetId = terminal ID
        ActivateDevice,       // targetId = device ID
        RetrieveData,         // targetId = data core ID
        CaptureZone,          // targetId = zone ID
        RescueHostage,        // targetId = hostage ID
        PlantExplosive,       // targetId = location ID
        ScanAnomaly,          // targetId = anomaly ID
        StabilizeReality      // targetId = anchor ID
    }
    
    public enum QuestType
    {
        MainStory,
        FactionQuest,
        SideQuest,
        Bounty,
        Escort,
        Delivery,
        Exploration,
        Survival,
        Construction,
        Research,
        Trading,
        Assassination,
        Defense,
        Rescue,
        Special
    }
    
    public enum QuestCategory
    {
        Combat,
        Exploration,
        Crafting,
        Social,
        Story,
        Faction,
        Economy,
        Survival,
        Mystery,
        Anomaly
    }
    
    public enum QuestState
    {
        NotStarted,
        Active,
        Completed,
        Failed,
        Abandoned
    }
    
    /// <summary>
    /// Static quest database with main storyline and procedural generation templates.
    /// </summary>
    public static class QuestDatabase
    {
        public static QuestData[] MainStoryQuests;
        public static QuestData[][] FactionQuests; // Per faction
        public static QuestTemplate[] ProceduralTemplates;
        
        static QuestDatabase()
        {
            InitializeMainStory();
            InitializeFactionQuests();
            InitializeProceduralTemplates();
        }
        
        private static void InitializeMainStory()
        {
            // 12 chapters of main storyline
            MainStoryQuests = new QuestData[24]; // ~2 quests per chapter
            int index = 0;
            
            // Chapter 1: Awakening
            MainStoryQuests[index++] = CreateQuest_Awakening();
            MainStoryQuests[index++] = CreateQuest_FirstContact();
            
            // Chapter 2: The Settlement
            MainStoryQuests[index++] = CreateQuest_EstablishBase();
            MainStoryQuests[index++] = CreateQuest_SupplyRun();
            
            // Chapter 3: Faction Politics
            MainStoryQuests[index++] = CreateQuest_FactionMeeting();
            MainStoryQuests[index++] = CreateQuest_ProveYourself();
            
            // Continue through all 12 chapters...
            // Chapters 4-12 would cover escalating Continuum Array threat
        }
        
        private static void InitializeFactionQuests()
        {
            // 5 factions, each with unique questline
            FactionQuests = new QuestData[5][];
            
            // Faction 0: United Colonies
            FactionQuests[0] = new QuestData[15];
            FactionQuests[0][0] = CreateQuest_UC_Recruitment();
            
            // Faction 1: Free Traders Guild
            FactionQuests[1] = new QuestData[15];
            
            // Faction 2: Tech Ascendancy
            FactionQuests[2] = new QuestData[15];
            
            // Faction 3: Nature's Children
            FactionQuests[3] = new QuestData[15];
            
            // Faction 4: Anomaly Cult
            FactionQuests[4] = new QuestData[15];
        }
        
        private static void InitializeProceduralTemplates()
        {
            ProceduralTemplates = new QuestTemplate[50];
            int index = 0;
            
            // Bounty templates
            ProceduralTemplates[index++] = CreateBountyTemplate_KillHostiles();
            ProceduralTemplates[index++] = CreateBountyTemplate_CaptureTarget();
            
            // Escort templates
            ProceduralTemplates[index++] = CreateEscortTemplate_Trader();
            ProceduralTemplates[index++] = CreateEscortTemplate_Refugee();
            
            // Delivery templates
            ProceduralTemplates[index++] = CreateDeliveryTemplate_Supplies();
            ProceduralTemplates[index++] = CreateDeliveryTemplate_Urgent();
            
            // Resource run templates
            ProceduralTemplates[index++] = CreateResourceTemplate_Mining();
            ProceduralTemplates[index++] = CreateResourceTemplate_Harvesting();
            
            // Rescue templates
            ProceduralTemplates[index++] = CreateRescueTemplate_Survivors();
            ProceduralTemplates[index++] = CreateRescueTemplate_Hostage();
            
            // Defense templates
            ProceduralTemplates[index++] = CreateDefenseTemplate_Base();
            ProceduralTemplates[index++] = CreateDefenseTemplate_Convoy();
        }
        
        #region Main Story Quests
        private static QuestData CreateQuest_Awakening()
        {
            var quest = new QuestData();
            quest.Initialize(1, "Awakening", QuestType.MainStory, QuestCategory.Story);
            quest.isMainStory = true;
            quest.chapterNumber = 1;
            quest.description = "You wake up in a crashed escape pod with no memory. Assess your situation and find shelter.";
            quest.objectives[0] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.ReachLocation, 
                targetValue = 1, 
                customDescription = "Reach the crash site beacon" 
            };
            quest.objectives[1] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.CollectItem, 
                targetId = (int)ItemType.MedKit,
                targetValue = 1, 
                customDescription = "Find medical supplies" 
            };
            quest.objectives[2] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.SurviveTime, 
                targetValue = 300, // 5 minutes
                customDescription = "Survive the first night" 
            };
            quest.experienceReward = 500;
            quest.itemRewards = new int[] { (int)ItemType.PurifiedWater, (int)ItemType.CookedRations };
            quest.itemRewardQuantities = new int[] { 3, 2 };
            quest.giverNPCId = -1; // Auto-start
            quest.turnInNPCId = -1;
            return quest;
        }
        
        private static QuestData CreateQuest_FirstContact()
        {
            var quest = new QuestData();
            quest.Initialize(2, "First Contact", QuestType.MainStory, QuestCategory.Social);
            quest.isMainStory = true;
            quest.chapterNumber = 1;
            quest.prerequisiteQuestIds = new int[] { 1 };
            quest.description = "A mysterious radio signal beckons. Investigate its source and make contact.";
            quest.objectives[0] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.ReachLocation,
                targetValue = 1,
                customDescription = "Follow the signal to its origin"
            };
            quest.objectives[1] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.TalkToNPC,
                targetValue = 1,
                customDescription = "Speak with the survivor"
            };
            quest.experienceReward = 750;
            quest.factionReputationReward = 100;
            quest.unlockRecipeId = 5;
            return quest;
        }
        
        private static QuestData CreateQuest_EstablishBase()
        {
            var quest = new QuestData();
            quest.Initialize(3, "Establish Base", QuestType.MainStory, QuestCategory.Crafting);
            quest.isMainStory = true;
            quest.chapterNumber = 2;
            quest.prerequisiteQuestIds = new int[] { 2 };
            quest.description = "Build a sustainable base of operations. Construct essential structures.";
            quest.objectives[0] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.BuildStructure,
                targetId = (int)WorkbenchType.BasicBench,
                targetValue = 1,
                customDescription = "Build a workbench"
            };
            quest.objectives[1] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.BuildStructure,
                targetId = (int)BuildingType.WoodenShack,
                targetValue = 1,
                customDescription = "Construct shelter"
            };
            quest.objectives[2] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.CraftItem,
                targetId = (int)ItemType.WoodenPlanks,
                targetValue = 20,
                customDescription = "Craft building materials"
            };
            quest.experienceReward = 1000;
            quest.unlockRecipeId = 25;
            return quest;
        }
        #endregion
        
        #region Faction Quests
        private static QuestData CreateQuest_UC_Recruitment()
        {
            var quest = new QuestData();
            quest.Initialize(100, "United Colonies Recruitment", QuestType.FactionQuest, QuestCategory.Faction);
            quest.factionId = 0;
            quest.minimumFactionReputation = 0;
            quest.description = "The United Colonies are always looking for capable individuals. Prove your worth.";
            quest.objectives[0] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.KillEntity,
                targetValue = 10,
                customDescription = "Eliminate hostile wildlife"
            };
            quest.objectives[1] = new QuestObjective 
            { 
                objectiveType = QuestObjectiveType.CollectItem,
                targetId = (int)ItemType.AnomalyShard,
                targetValue = 3,
                customDescription = "Collect anomaly samples"
            };
            quest.experienceReward = 500;
            quest.factionReputationReward = 250;
            quest.giverNPCId = 1001; // UC Recruiter
            return quest;
        }
        #endregion
        
        #region Procedural Templates
        private static QuestTemplate CreateBountyTemplate_KillHostiles()
        {
            return new QuestTemplate
            {
                templateType = QuestType.Bounty,
                baseExperience = 300,
                baseReputation = 100,
                objectiveTemplate = new ObjectiveTemplate 
                { 
                    type = QuestObjectiveType.KillEntity,
                    minValue = 3,
                    maxValue = 10,
                    entityTypeRange = new int[] { 1, 5 } // Enemy type range
                },
                validFactions = new int[] { 0, 1, 2 },
                difficultyMultiplier = 1.0f
            };
        }
        
        private static QuestTemplate CreateEscortTemplate_Trader()
        {
            return new QuestTemplate
            {
                templateType = QuestType.Escort,
                baseExperience = 500,
                baseReputation = 150,
                objectiveTemplate = new ObjectiveTemplate
                {
                    type = QuestObjectiveType.EscortNPC,
                    minValue = 1,
                    maxValue = 1,
                    escortRouteId = 0 // Will be randomized
                },
                validFactions = new int[] { 1 }, // Traders
                difficultyMultiplier = 1.2f
            };
        }
        
        private static QuestTemplate CreateDeliveryTemplate_Supplies()
        {
            return new QuestTemplate
            {
                templateType = QuestType.Delivery,
                baseExperience = 200,
                baseReputation = 75,
                objectiveTemplate = new ObjectiveTemplate
                {
                    type = QuestObjectiveType.DeliverItem,
                    minValue = 1,
                    maxValue = 5,
                    itemTypeRange = new int[] { 10, 30 }
                },
                validFactions = new int[] { 0, 1, 2, 3 },
                difficultyMultiplier = 0.8f
            };
        }
        #endregion
        
        public static QuestData GenerateProceduralQuest(QuestTemplate template, int seed)
        {
            UnityEngine.Random.InitState(seed);
            
            var quest = new QuestData();
            quest.Initialize(
                UnityEngine.Random.Range(1000, 9999),
                GetRandomQuestName(template.templateType),
                template.templateType,
                QuestCategory.Combat
            );
            
            // Randomize objectives based on template
            quest.objectives[0] = new QuestObjective
            {
                objectiveType = template.objectiveTemplate.type,
                targetValue = UnityEngine.Random.Range(
                    template.objectiveTemplate.minValue,
                    template.objectiveTemplate.maxValue + 1
                )
            };
            
            quest.experienceReward = Mathf.RoundToInt(template.baseExperience * template.difficultyMultiplier);
            quest.factionReputationReward = Mathf.RoundToInt(template.baseReputation * template.difficultyMultiplier);
            
            return quest;
        }
        
        private static string GetRandomQuestName(QuestType type)
        {
            string[] prefixes = { "The", "Operation", "Mission", "Contract" };
            string[] combatNames = { "Cleansing", "Elimination", "Strike", "Purge" };
            string[] deliveryNames = { "Supply Run", "Delivery", "Transport", "Handoff" };
            string[] escortNames = { "Protection", "Escort Duty", "Safe Passage", "Guardian" };
            
            return type switch
            {
                QuestType.Bounty => $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {combatNames[UnityEngine.Random.Range(0, combatNames.Length)]}",
                QuestType.Delivery => $"{deliveryNames[UnityEngine.Random.Range(0, deliveryNames.Length)]} #{UnityEngine.Random.Range(1, 99)}",
                QuestType.Escort => $"{escortNames[UnityEngine.Random.Range(0, escortNames.Length)]}",
                _ => $"Quest #{UnityEngine.Random.Range(1, 999)}"
            };
        }
    }
    
    [Serializable]
    public struct QuestTemplate
    {
        public QuestType templateType;
        public int baseExperience;
        public int baseReputation;
        public ObjectiveTemplate objectiveTemplate;
        public int[] validFactions;
        public float difficultyMultiplier;
        public int levelRangeMin;
        public int levelRangeMax;
    }
    
    [Serializable]
    public struct ObjectiveTemplate
    {
        public QuestObjectiveType type;
        public int minValue;
        public int maxValue;
        public int[] entityTypeRange;
        public int[] itemTypeRange;
        public int escortRouteId;
    }
}
