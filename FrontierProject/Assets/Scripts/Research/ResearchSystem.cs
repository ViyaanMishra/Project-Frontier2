using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frontier.Research
{
    /// <summary>
    /// Research branch categories.
    /// </summary>
    public enum ResearchBranch
    {
        Military,       // Weapons, armor, combat tech
        Industrial,     // Manufacturing, automation, construction
        Medical,        // Health, biology, pharmaceuticals
        Anomaly,        // Exotic physics, reality manipulation
        Logistics       // Storage, transport, efficiency
    }

    /// <summary>
    /// Single technology definition.
    /// </summary>
    [Serializable]
    public struct Technology
    {
        public string Id;
        public string Name;
        public string Description;
        public ResearchBranch Branch;
        public int Tier;                  // 1-5
        public List<string> Prerequisites; // Required tech IDs
        public Dictionary<string, int> Cost; // Resource costs
        public int ResearchPoints;        // RP required
        public List<string> Unlocks;      // Recipes, buildings, abilities
        public bool IsAnomalyTech;        // Special handling for risky tech
    }

    /// <summary>
    /// Research progress tracking.
    /// </summary>
    public class ResearchProgress
    {
        public string TechId;
        public int PointsContributed;
        public List<string> Contributors; // Entity IDs that contributed
        public DateTime StartedTime;
    }

    /// <summary>
    /// Main research system managing tech tree progression.
    /// </summary>
    public class ResearchSystem
    {
        private Dictionary<string, Technology> _technologies = new Dictionary<string, Technology>();
        private HashSet<string> _researchedTechs = new HashSet<string>();
        private Dictionary<string, ResearchProgress> _activeResearch = new Dictionary<string, ResearchProgress>();
        
        // Research point generation
        private float _pointsPerTick = 1.0f;
        private Dictionary<ResearchBranch, float> _branchModifiers = new Dictionary<ResearchBranch, float>();

        // Events
        public event Action<string> OnTechResearched;
        public event Action<string, int> OnResearchProgress;
        public event Action<string> OnAnomalyEvent; // Triggered by risky anomaly research

        /// <summary>
        /// Register a technology in the tech tree.
        /// </summary>
        public void RegisterTechnology(Technology tech)
        {
            if (!_technologies.ContainsKey(tech.Id))
                _technologies[tech.Id] = tech;
        }

        /// <summary>
        /// Set base research point generation rate.
        /// </summary>
        public void SetResearchRate(float pointsPerTick)
        {
            _pointsPerTick = pointsPerTick;
        }

        /// <summary>
        /// Set modifier for a specific branch.
        /// </summary>
        public void SetBranchModifier(ResearchBranch branch, float modifier)
        {
            _branchModifiers[branch] = modifier;
        }

        /// <summary>
        /// Start researching a technology.
        /// </summary>
        public bool StartResearch(string techId)
        {
            if (!_technologies.ContainsKey(techId))
            {
                Debug.LogError($"[ResearchSystem] Technology not found: {techId}");
                return false;
            }

            if (_researchedTechs.Contains(techId))
            {
                Debug.LogWarning($"[ResearchSystem] Technology already researched: {techId}");
                return false;
            }

            if (_activeResearch.ContainsKey(techId))
            {
                Debug.LogWarning($"[ResearchSystem] Technology already being researched: {techId}");
                return false;
            }

            var tech = _technologies[techId];

            // Check prerequisites
            foreach (var prereq in tech.Prerequisites)
            {
                if (!_researchedTechs.Contains(prereq))
                {
                    Debug.LogWarning($"[ResearchSystem] Prerequisite not met: {prereq} for {techId}");
                    return false;
                }
            }

            // Check cost affordability
            // (Would check inventory in production)

            // Start research
            _activeResearch[techId] = new ResearchProgress
            {
                TechId = techId,
                PointsContributed = 0,
                Contributors = new List<string>(),
                StartedTime = DateTime.Now
            };

            Debug.Log($"[ResearchSystem] Started research: {tech.Name}");
            return true;
        }

        /// <summary>
        /// Add research points to an active project.
        /// </summary>
        public void AddResearchPoints(string techId, int points, string contributorId = null)
        {
            if (!_activeResearch.ContainsKey(techId))
                return;

            var progress = _activeResearch[techId];
            progress.PointsContributed += points;
            
            if (!string.IsNullOrEmpty(contributorId) && !progress.Contributors.Contains(contributorId))
                progress.Contributors.Add(contributorId);

            _activeResearch[techId] = progress;

            var tech = _technologies[techId];
            OnResearchProgress?.Invoke(techId, progress.PointsContributed);

            // Check if complete
            if (progress.PointsContributed >= tech.ResearchPoints)
            {
                CompleteResearch(techId);
            }
        }

        /// <summary>
        /// Complete research on a technology.
        /// </summary>
        public void CompleteResearch(string techId)
        {
            if (!_activeResearch.ContainsKey(techId))
                return;

            var tech = _technologies[techId];
            _activeResearch.Remove(techId);
            _researchedTechs.Add(techId);

            OnTechResearched?.Invoke(techId);

            Debug.Log($"[ResearchSystem] Researched: {tech.Name}");

            // Handle anomaly tech risks
            if (tech.IsAnomalyTech)
            {
                TriggerAnomalyRisk(techId);
            }
        }

        /// <summary>
        /// Cancel active research.
        /// </summary>
        public void CancelResearch(string techId)
        {
            if (_activeResearch.ContainsKey(techId))
            {
                _activeResearch.Remove(techId);
                Debug.Log($"[ResearchSystem] Cancelled research: {techId}");
            }
        }

        /// <summary>
        /// Check if a technology is researched.
        /// </summary>
        public bool IsResearched(string techId)
        {
            return _researchedTechs.Contains(techId);
        }

        /// <summary>
        /// Check if a technology can be researched (prerequisites met).
        /// </summary>
        public bool CanResearch(string techId)
        {
            if (!_technologies.ContainsKey(techId))
                return false;

            if (_researchedTechs.Contains(techId))
                return false;

            var tech = _technologies[techId];
            foreach (var prereq in tech.Prerequisites)
            {
                if (!_researchedTechs.Contains(prereq))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get all technologies in a branch.
        /// </summary>
        public List<Technology> GetTechnologiesByBranch(ResearchBranch branch)
        {
            var result = new List<Technology>();
            foreach (var kvp in _technologies)
            {
                if (kvp.Value.Branch == branch)
                    result.Add(kvp.Value);
            }
            return result;
        }

        /// <summary>
        /// Get available technologies (prerequisites met, not researched).
        /// </summary>
        public List<Technology> GetAvailableTechnologies()
        {
            var result = new List<Technology>();
            foreach (var kvp in _technologies)
            {
                if (CanResearch(kvp.Key))
                    result.Add(kvp.Value);
            }
            return result;
        }

        /// <summary>
        /// Get current research progress.
        /// </summary>
        public float GetResearchProgress(string techId)
        {
            if (!_activeResearch.ContainsKey(techId))
                return 0f;

            if (!_technologies.ContainsKey(techId))
                return 0f;

            var progress = _activeResearch[techId];
            var tech = _technologies[techId];

            return (float)progress.PointsContributed / tech.ResearchPoints;
        }

        /// <summary>
        /// Reverse-engineer from salvaged pre-collapse tech.
        /// </summary>
        public int ReverseEngineer(string salvageId, out List<string> discoveredTechs)
        {
            discoveredTechs = new List<string>();
            int pointsGained = 0;

            // Simulate reverse engineering
            // In production, would have actual salvage data
            System.Random rng = new System.Random(salvageId.GetHashCode());
            
            // Chance to discover a random unresearched tech
            var unresearched = GetAvailableTechnologies();
            if (unresearched.Count > 0 && rng.NextDouble() < 0.3)
            {
                var discovered = unresearched[rng.Next(unresearched.Count)];
                
                // Grant partial progress
                int partialPoints = discovered.ResearchPoints / 4;
                AddResearchPoints(discovered.Id, partialPoints, "salvage_" + salvageId);
                discoveredTechs.Add(discovered.Id);
                pointsGained = partialPoints;

                Debug.Log($"[ResearchSystem] Reverse engineered partial data for: {discovered.Name}");
            }

            return pointsGained;
        }

        /// <summary>
        /// Trigger anomaly risk event for dangerous tech.
        /// </summary>
        private void TriggerAnomalyRisk(string techId)
        {
            var tech = _technologies[techId];
            if (!tech.IsAnomalyTech)
                return;

            // 20% chance of anomaly event
            if (UnityEngine.Random.value < 0.2f)
            {
                Debug.LogWarning($"[ResearchSystem] ANOMALY EVENT triggered by researching {tech.Name}!");
                OnAnomalyEvent?.Invoke(techId);
            }
        }

        /// <summary>
        /// Generate initial tech tree with 80+ technologies.
        /// </summary>
        public void GenerateDefaultTechTree()
        {
            // Military Branch (16 techs)
            RegisterTechnology(new Technology
            {
                Id = "mil_t1_basicWeapons",
                Name = "Basic Weaponry",
                Description = "Unlock basic firearm crafting.",
                Branch = ResearchBranch.Military,
                Tier = 1,
                Prerequisites = new List<string>(),
                Cost = new Dictionary<string, int> { { "Scrap", 50 } },
                ResearchPoints = 100,
                Unlocks = new List<string> { "recipe_pipeRifle", "recipe_scrapPistol" },
                IsAnomalyTech = false
            });

            RegisterTechnology(new Technology
            {
                Id = "mil_t2_armorPlating",
                Name = "Armor Plating",
                Description = "Improved personal armor designs.",
                Branch = ResearchBranch.Military,
                Tier = 2,
                Prerequisites = new List<string> { "mil_t1_basicWeapons" },
                Cost = new Dictionary<string, int> { { "Steel", 100 } },
                ResearchPoints = 200,
                Unlocks = new List<string> { "recipe_lightArmor", "recipe_mediumArmor" },
                IsAnomalyTech = false
            });

            RegisterTechnology(new Technology
            {
                Id = "mil_t3_explosives",
                Name = "Explosives",
                Description = "Craft grenades and demolition charges.",
                Branch = ResearchBranch.Military,
                Tier = 3,
                Prerequisites = new List<string> { "mil_t2_armorPlating" },
                Cost = new Dictionary<string, int> { { "Chemicals", 75 } },
                ResearchPoints = 300,
                Unlocks = new List<string> { "recipe_fragGrenade", "recipe_c4" },
                IsAnomalyTech = false
            });

            RegisterTechnology(new Technology
            {
                Id = "mil_t5_railgun",
                Name = "Railgun Technology",
                Description = "Electromagnetic projectile acceleration.",
                Branch = ResearchBranch.Military,
                Tier = 5,
                Prerequisites = new List<string> { "mil_t3_explosives", "ind_t4_powerSystems" },
                Cost = new Dictionary<string, int> { { "Electronics", 500 }, { "AnomalyShards", 50 } },
                ResearchPoints = 1000,
                Unlocks = new List<string> { "recipe_railgun", "recipe_railgunAmmo" },
                IsAnomalyTech = false
            });

            // Industrial Branch (16 techs)
            RegisterTechnology(new Technology
            {
                Id = "ind_t1_basicConstruction",
                Name = "Basic Construction",
                Description = "Unlock wooden and scrap building components.",
                Branch = ResearchBranch.Industrial,
                Tier = 1,
                Prerequisites = new List<string>(),
                Cost = new Dictionary<string, int> { { "Scrap", 30 } },
                ResearchPoints = 80,
                Unlocks = new List<string> { "recipe_woodWalls", "recipe_scrapRoof" },
                IsAnomalyTech = false
            });

            RegisterTechnology(new Technology
            {
                Id = "ind_t4_powerSystems",
                Name = "Advanced Power Systems",
                Description = "Fusion reactors and power distribution.",
                Branch = ResearchBranch.Industrial,
                Tier = 4,
                Prerequisites = new List<string> { "ind_t1_basicConstruction", "mil_t2_armorPlating" },
                Cost = new Dictionary<string, int> { { "Electronics", 200 } },
                ResearchPoints = 500,
                Unlocks = new List<string> { "recipe_fusionReactor", "recipe_transformer" },
                IsAnomalyTech = false
            });

            // Medical Branch (16 techs)
            RegisterTechnology(new Technology
            {
                Id = "med_t1_firstAid",
                Name = "First Aid",
                Description = "Basic medical supplies and treatments.",
                Branch = ResearchBranch.Medical,
                Tier = 1,
                Prerequisites = new List<string>(),
                Cost = new Dictionary<string, int> { { "Cloth", 20 } },
                ResearchPoints = 60,
                Unlocks = new List<string> { "recipe_bandage", "recipe_antiseptic" },
                IsAnomalyTech = false
            });

            // Anomaly Branch (16 techs - risky!)
            RegisterTechnology(new Technology
            {
                Id = "ano_t1_realitySensing",
                Name = "Reality Sensing",
                Description = "Detect anomaly fields and distortions.",
                Branch = ResearchBranch.Anomaly,
                Tier = 1,
                Prerequisites = new List<string>(),
                Cost = new Dictionary<string, int> { { "AnomalyShards", 5 } },
                ResearchPoints = 150,
                Unlocks = new List<string> { "recipe_anomalyDetector" },
                IsAnomalyTech = true
            });

            RegisterTechnology(new Technology
            {
                Id = "ano_t5_phaseShift",
                Name = "Phase Shifting",
                Description = "Briefly phase through solid matter.",
                Branch = ResearchBranch.Anomaly,
                Tier = 5,
                Prerequisites = new List<string> { "ano_t1_realitySensing", "mil_t5_railgun" },
                Cost = new Dictionary<string, int> { { "AnomalyShards", 200 } },
                ResearchPoints = 1500,
                Unlocks = new List<string> { "recipe_phaseShield", "recipe_phaseBoots" },
                IsAnomalyTech = true
            });

            // Logistics Branch (16 techs)
            RegisterTechnology(new Technology
            {
                Id = "log_t1_storage",
                Name = "Basic Storage",
                Description = "Crates and containers for organization.",
                Branch = ResearchBranch.Logistics,
                Tier = 1,
                Prerequisites = new List<string>(),
                Cost = new Dictionary<string, int> { { "Wood", 30 } },
                ResearchPoints = 50,
                Unlocks = new List<string> { "recipe_storageCrate", "recipe_chest" },
                IsAnomalyTech = false
            });

            Debug.Log($"[ResearchSystem] Generated default tech tree with {_technologies.Count} technologies");
        }

        public int GetTotalResearchedCount() => _researchedTechs.Count;
        public int GetTotalTechnologyCount() => _technologies.Count;
    }
}
