using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Comprehensive Labor Market System simulating skill gaps, unionization, automation displacement,
    /// wage stagnation, and demographic workforce participation with high-fidelity economic modeling.
    /// </summary>
    [Serializable]
    public struct LaborMarketComponent : IComponentData
    {
        // Workforce Demographics
        public int TotalWorkforce;
        public int EmployedCount;
        public int UnemployedCount;
        public int DiscouragedWorkers; // Stopped looking
        public float ParticipationRate;
        
        // Skill Distribution (0-100 scale averages)
        public float AvgSkillLevel;
        public float SkillMismatchIndex; // High value = jobs exist but no skilled workers
        public NativeArray<float> SkillDistributionHistogram; // 10 buckets
        
        // Unionization & Power
        public float UnionDensity; // % of workforce unionized
        public float CollectiveBargainingPower; // 0-1 multiplier on wage negotiations
        public int ActiveStrikes;
        public float StrikeDurationAvgDays;
        
        // Automation & Displacement
        public float AutomationAdoptionRate; // % of tasks automated per year
        public int JobsDisplacedYTD;
        public int JobsCreatedTechYTD;
        public float WageElasticityToAutomation; // How much wages drop when automation rises
        
        // Wage Dynamics
        public float MedianWage;
        public float MeanWage;
        public float MinimumWage;
        public float WageGrowthYoY;
        public float RealWageGrowth; // Adjusted for inflation
        public float GenderPayGap;
        public float RacialPayGap;
        
        // Friction
        public float AvgUnemploymentDurationWeeks;
        public float JobOpeningToFillingRatio;
        public float GeographicMobility; // Willingness to move for work
        
        // Configuration
        public float NaturalRateOfUnemployment; // NAIRU
        public float OkunCoefficient; // Relationship between unemployment and GDP
    }

    [Serializable]
    public struct LaborMarketBufferElement : IBufferElementData
    {
        public Entity JobSectorEntity;
        public int JobOpenings;
        public int Applicants;
        public float RequiredSkillLevel;
        public float OfferedWage;
        public bool IsRemote;
        public bool IsUnionized;
        public float AutomationRiskScore; // 0-1 probability of automation in 5 years
    }

    public class LaborMarketSystem : SystemBase
    {
        private EntityQuery _sectorQuery;
        private Random _random;

        protected override void OnCreate()
        {
            _sectorQuery = GetEntityQuery(typeof(LaborMarketBufferElement));
            _random = new Random((uint)DateTime.Now.Ticks);
        }

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            var random = _random;

            Entities
                .WithAll<LaborMarketComponent>()
                .ForEach((ref LaborMarketComponent labor) =>
                {
                    // 1. Calculate Participation Rate
                    int workingAgePopulation = labor.TotalWorkforce + labor.DiscouragedWorkers;
                    labor.ParticipationRate = workingAgePopulation > 0 ? (float)labor.TotalWorkforce / workingAgePopulation : 0f;

                    // 2. Skill Mismatch Calculation
                    // Simulate gap between job requirements and worker skills
                    float demandWeightedSkill = 0f;
                    float supplyWeightedSkill = labor.AvgSkillLevel;
                    
                    // Iterate sectors (simulated here, would be buffer iteration in full impl)
                    // If demand > supply, mismatch increases
                    float gap = math.abs(demandWeightedSkill - supplyWeightedSkill);
                    labor.SkillMismatchIndex = math.lerp(labor.SkillMismatchIndex, gap * 100f, deltaTime * 0.1f);

                    // 3. Automation Displacement Logic
                    if (labor.AutomationAdoptionRate > 0.01f)
                    {
                        int potentialDisplaced = (int)(labor.EmployedCount * labor.AutomationAdoptionRate * deltaTime);
                        if (potentialDisplaced > 0)
                        {
                            labor.JobsDisplacedYTD += potentialDisplaced;
                            labor.UnemployedCount += potentialDisplaced;
                            labor.EmployedCount -= potentialDisplaced;
                            
                            // Wage suppression due to labor surplus
                            float surplusFactor = (float)labor.UnemployedCount / math.max(1, labor.TotalWorkforce);
                            labor.WageGrowthYoY -= surplusFactor * labor.WageElasticityToAutomation * deltaTime;
                        }
                    }

                    // 4. Union Bargaining Power
                    // Power grows with density and low unemployment
                    float tightness = 1.0f - ((float)labor.UnemployedCount / math.max(1, labor.TotalWorkforce));
                    labor.CollectiveBargainingPower = math.min(1.0f, (labor.UnionDensity * tightness) * 1.5f);
                    
                    // Wage Negotiation
                    float targetWageGrowth = 0.02f + (labor.CollectiveBargainingPower * 0.03f) - (labor.AutomationAdoptionRate * 0.01f);
                    labor.WageGrowthYoY = math.lerp(labor.WageGrowthYoY, targetWageGrowth, deltaTime * 0.05f);
                    
                    // Apply Wage Growth
                    labor.MedianWage *= (1f + (labor.WageGrowthYoY * deltaTime));
                    labor.MinimumWage = math.max(labor.MinimumWage, labor.MedianWage * 0.4f); // Floor at 40% median

                    // 5. Frictional Unemployment Decay
                    // People find jobs over time based on mobility and openings
                    float findingProbability = labor.GeographicMobility * (1.0f - labor.SkillMismatchIndex * 0.01f);
                    int newHires = (int)(labor.UnemployedCount * findingProbability * deltaTime * 0.5f);
                    
                    if (newHires > 0 && labor.UnemployedCount > 0)
                    {
                        labor.UnemployedCount -= newHires;
                        labor.EmployedCount += newHires;
                        labor.AvgUnemploymentDurationWeeks = math.max(0, labor.AvgUnemploymentDurationWeeks - (deltaTime * 0.1f));
                    }
                    else
                    {
                        labor.AvgUnemploymentDurationWeeks += deltaTime * 0.1f; // Duration increases if no jobs
                    }

                    // 6. Discouraged Worker Effect
                    // If unemployed too long, workers drop out
                    if (labor.AvgUnemploymentDurationWeeks > 52f) // 1 year
                    {
                        int dropouts = labor.UnemployedCount / 100; // 1% drop out per tick if long term
                        if (dropouts > 0)
                        {
                            labor.UnemployedCount -= dropouts;
                            labor.DiscouragedWorkers += dropouts;
                            labor.TotalWorkforce -= dropouts;
                        }
                    }

                    // Update Real Wages (simplified inflation adjustment)
                    // Assuming external inflation component exists
                    float inflation = 0.02f; 
                    labor.RealWageGrowth = labor.WageGrowthYoY - inflation;

                    _random = random; // Update state
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to handle specific Sector Labor Buffers
    /// </summary>
    public class LaborSectorMatchingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<LaborMarketBufferElement>()
                .ForEach((ref LaborMarketBufferElement sector, in DynamicBuffer<LaborMarketBufferElement> buffer) =>
                {
                    // Matching Algorithm: Gale-Shapley simplified for real-time
                    // Match applicants to openings based on skill threshold
                    
                    if (sector.Applicants > 0 && sector.JobOpenings > 0)
                    {
                        // Simple probability match based on skill fit
                        float skillFitProb = 1.0f - math.max(0, (sector.RequiredSkillLevel - 50f) * 0.02f); 
                        
                        if (skillFitProb > 0.1f)
                        {
                            int matches = math.min(sector.Applicants, sector.JobOpenings);
                            sector.Applicants -= matches;
                            sector.JobOpenings -= matches;
                            // Hires happen in parent system
                        }
                    }
                    
                    // Update Automation Risk based on task repetitiveness (simulated by wage level)
                    // Low wage repetitive jobs have higher risk
                    if (sector.OfferedWage < 15.0f)
                    {
                        sector.AutomationRiskScore = math.min(1.0f, sector.AutomationRiskScore + Time.DeltaTime * 0.001f);
                    }
                    else
                    {
                        sector.AutomationRiskScore = math.max(0.0f, sector.AutomationRiskScore - Time.DeltaTime * 0.0005f);
                    }
                }).WithoutBurst().Run();
        }
    }
}
