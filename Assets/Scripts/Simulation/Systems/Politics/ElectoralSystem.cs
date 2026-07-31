using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Politics
{
    /// <summary>
    /// Advanced Electoral System simulating gerrymandering, campaign finance tracking,
    /// voting systems (FPTP, PR, Ranked Choice), voter suppression, and political realignment.
    /// </summary>
    [Serializable]
    public struct ElectoralSystemComponent : IComponentData
    {
        // System Type
        public int VotingSystemType; // 0=FPTP, 1=Proportional, 2=RankedChoice, 3=TwoRound
        public int DistrictCount;
        public int TotalSeats;
        
        // Districting
        public float GerrymanderIndex; // 0-1, higher = more manipulated
        public float EfficiencyGap; // Measure of partisan bias
        public float CompactnessScore; // 0-1, geometric compactness
        public float MalapportionmentIndex; // Population deviation between districts
        
        // Campaign Finance
        public double TotalCampaignSpending;
        public double DarkMoneyAmount; // Untraceable donations
        public double PublicFundingAmount;
        public float CorporateInfluenceIndex; // 0-1
        public float ForeignInterferenceAmount;
        
        // Voter Dynamics
        public int RegisteredVoters;
        public int TurnoutLastElection;
        public float TurnoutTrend; // Increasing or decreasing
        public float VoterSuppressionIndex; // 0-1
        public int DisenfranchisedCount;
        
        // Polling & Prediction
        public float PollingAccuracy; // Historical MAE
        public float SwingVoterPercent;
        public float LateDeciderPercent;
        
        // Election Integrity
        public float AuditCoverage; // % of races audited
        public float CertificationDelayDays;
        public int ContestedRaces;
    }

    [Serializable]
    public struct ElectoralDistrictElement : IBufferElementData
    {
        public int DistrictId;
        public int Population;
        public int RegisteredVoters;
        public float PartisanIndex; // -1 (Left) to +1 (Right)
        public float IncumbencyAdvantage; // 0-1
        public bool IsGerrymandered;
        public float MarginOfVictoryLast; // %
        public double SpendingByIncumbent;
        public double SpendingByChallenger;
        public float PollingLead; // Current poll margin
        public int ProjectedWinnerParty; // 0, 1, 2...
    }

    [Serializable]
    public struct DonationElement : IBufferElementData
    {
        public Entity DonorEntity;
        public Entity RecipientEntity;
        public double Amount;
        public int DonorType; // 0=Individual, 1=PAC, 2=Corp, 3=Union, 4=Foreign
        public bool IsDarkMoney; // 501(c)(4) etc
        public bool IsMatched; // Public match
        public int TickReceived;
    }

    public class ElectoralSystem : SystemBase
    {
        private Random _random;

        protected override void OnCreate()
        {
            _random = new Random((uint)DateTime.Now.Ticks);
        }

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            var random = _random;

            // Update Macro Electoral Metrics
            Entities
                .WithAll<ElectoralSystemComponent>()
                .ForEach((ref ElectoralSystemComponent electoral) =>
                {
                    // 1. Gerrymandering Impact on Efficiency Gap
                    // Higher gerrymander = larger efficiency gap (wasted votes)
                    float baseEfficiencyGap = electoral.GerrymanderIndex * 0.15f;
                    electoral.EfficiencyGap = math.lerp(electoral.EfficiencyGap, baseEfficiencyGap, deltaTime * 0.02f);
                    
                    // Malapportionment check (one person one vote violation)
                    if (electoral.MalapportionmentIndex > 0.1f)
                    {
                        electoral.VoterSuppressionIndex = math.max(electoral.VoterSuppressionIndex, 
                            electoral.MalapportionmentIndex * 0.5f);
                    }

                    // 2. Campaign Finance Dynamics
                    // Dark money grows with weak regulation
                    float regulatoryWeakness = 1.0f - electoral.AuditCoverage;
                    electoral.DarkMoneyAmount *= (1f + (regulatoryWeakness * deltaTime * 0.05f));
                    
                    // Corporate influence scales with spending
                    float totalPrivateSpending = electoral.TotalCampaignSpending - electoral.PublicFundingAmount;
                    electoral.CorporateInfluenceIndex = math.min(1f, totalPrivateSpending * 0.0000001f);

                    // 3. Turnout Dynamics
                    // Suppression reduces turnout, competitive races increase it
                    float competitivenessBonus = electoral.SwingVoterPercent * 0.1f;
                    float suppressionPenalty = electoral.VoterSuppressionIndex * 0.15f;
                    
                    float targetTurnout = 0.6f + competitivenessBonus - suppressionPenalty;
                    float currentTurnoutRate = (float)electoral.TurnoutLastElection / math.max(1, electoral.RegisteredVoters);
                    
                    electoral.TurnoutTrend = targetTurnout - currentTurnoutRate;
                    electoral.TurnoutLastElection = (int)(electoral.RegisteredVoters * 
                        math.lerp(currentTurnoutRate, targetTurnout, deltaTime * 0.01f));

                    // 4. Disenfranchisement
                    // Felony laws, ID requirements, etc.
                    if (electoral.VoterSuppressionIndex > 0.3f)
                    {
                        electoral.DisenfranchisedCount = (int)(electoral.RegisteredVoters * 
                            electoral.VoterSuppressionIndex * 0.1f);
                    }
                    else
                    {
                        electoral.DisenfranchisedCount = math.max(0, 
                            electoral.DisenfranchisedCount - (int)(deltaTime * 10f));
                    }

                    // 5. Foreign Interference Detection
                    if (electoral.ForeignInterferenceAmount > 0)
                    {
                        // Chance of exposure based on audit coverage
                        float detectionChance = electoral.AuditCoverage * deltaTime * 0.01f;
                        if (random.NextFloat() < detectionChance)
                        {
                            electoral.ForeignInterferenceAmount *= 0.5f; // Exposed and reduced
                            electoral.ContestedRaces++;
                        }
                    }

                    // 6. Late Deciders and Polling Accuracy
                    electoral.LateDeciderPercent = math.lerp(electoral.LateDeciderPercent, 
                        electoral.SwingVoterPercent * 0.5f, deltaTime * 0.02f);
                    
                    // Polling gets worse with more late deciders
                    electoral.PollingAccuracy = math.max(0.01f, 
                        0.03f + (electoral.LateDeciderPercent * 0.05f));

                    _random = random;
                }).WithoutBurst().Run();

            // Update Districts
            Entities
                .WithAll<ElectoralDistrictElement>()
                .ForEach((ref ElectoralDistrictElement district) =>
                {
                    // Incumbency advantage decays over time without spending
                    float spendingRatio = district.SpendingByIncumbent / math.max(1, district.SpendingByChallenger);
                    float incumbentBoost = math.log(spendingRatio + 1) * 0.05f;
                    
                    district.IncumbencyAdvantage = math.lerp(district.IncumbencyAdvantage, 
                        0.05f + incumbentBoost, deltaTime * 0.01f);

                    // Polling lead fluctuates
                    float volatility = 0.02f * (1f - district.IncumbencyAdvantage);
                    district.PollingLead += random.NextFloat(-volatility, volatility) * deltaTime;
                    
                    // Gerrymandered districts have biased margins
                    if (district.IsGerrymandered)
                    {
                        float biasDirection = math.sign(district.PartisanIndex);
                        district.MarginOfVictoryLast = math.abs(district.MarginOfVictoryLast) + (biasDirection * 0.05f);
                    }

                    // Project winner based on polling + incumbency + partisanship
                    float projectedMargin = district.PollingLead + 
                                           (district.IncumbencyAdvantage * 0.03f) + 
                                           (district.PartisanIndex * 0.1f);
                    
                    district.ProjectedWinnerParty = projectedMargin > 0 ? 1 : 0;
                    
                    // Close races attract more spending
                    if (math.abs(projectedMargin) < 0.05f)
                    {
                        district.SpendingByIncumbent *= (1f + deltaTime * 0.02f);
                        district.SpendingByChallenger *= (1f + deltaTime * 0.02f);
                    }
                }).WithoutBurst().Run();

            // Process Donations
            Entities
                .WithAll<DonationElement>()
                .ForEach((ref DonationElement donation) =>
                {
                    // Dark money has higher impact but risk of exposure
                    if (donation.IsDarkMoney)
                    {
                        // Effective multiplier for influence
                        double effectiveAmount = donation.Amount * 1.5f;
                        // Risk of exposure accumulates
                        // (handled by ElectoralSystemComponent audit logic)
                    }
                    
                    // Foreign donations are illegal but happen
                    if (donation.DonorType == 4) // Foreign
                    {
                        // If detected, severe consequences (handled externally)
                        donation.Amount = 0; // Laundered through shell companies
                    }
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to simulate redistricting cycles and gerrymandering optimization
    /// </summary>
    public class RedistrictingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Runs every 10 years (census cycle)
            // Optimizes district boundaries for partisan advantage
            
            Entities
                .WithAll<ElectoralSystemComponent>()
                .ForEach((ref ElectoralSystemComponent electoral) =>
                {
                    // Mock redistricting logic
                    // In full implementation, would use graph partitioning algorithms
                    
                    int censusYear = 2020;
                    int currentYear = 2024; // Mock
                    int yearsSinceCensus = currentYear - censusYear;
                    
                    if (yearsSinceCensus >= 10)
                    {
                        // Redistricting occurs
                        // Party in power maximizes their seats
                        float partisanBias = 0.1f; // Mock ruling party advantage
                        electoral.GerrymanderIndex = math.min(1f, electoral.GerrymanderIndex + partisanBias);
                        electoral.MalapportionmentIndex = random.NextFloat(0.01f, 0.05f);
                        
                        // Reset cycle
                        // censusYear = currentYear; // Would need persistent storage
                    }
                }).WithoutBurst().Run();
        }
    }
}
