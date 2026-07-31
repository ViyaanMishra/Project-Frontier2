using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Politics
{
    /// <summary>
    /// Intelligence Agency System simulating spy networks, covert operations, 
    /// signals intelligence, counter-intelligence, and geopolitical espionage.
    /// </summary>
    [Serializable]
    public struct IntelligenceAgencyComponent : IComponentData
    {
        // Agency Capabilities
        public float HumanIntelligenceCapacity; // HUMINT - spies on ground
        public float SignalsIntelligenceCapacity; // SIGINT - intercepts
        public float CyberOperationsCapacity; // Offensive/Defensive cyber
        public float CovertActionCapacity; // Regime change, sabotage
        public float AnalysisCapacity; // Processing intel
        
        // Network Metrics
        public int ActiveAgents;
        public int DoubleAgents;
        public int CompromisedAssets;
        public float NetworkSecurityScore; // 0-1
        
        // Operations
        public int ActiveCovertOps;
        public int SuccessfulOpsYTD;
        public int FailedOpsYTD;
        public float OpSecBreaches;
        
        // Budget & Resources
        public double ClassifiedBudget;
        public double BlackBudget; // Off-books
        public float ResourceAdequacy; // 0-1
        
        // Counter-Intelligence
        public float CounterSpyEffectiveness;
        public float LeakDetectionRate;
        public float MolesDetectedYTD;
        
        // Geopolitical Reach
        public int CountriesWithPresence;
        public float GlobalCoveragePercent;
    }

    [Serializable]
    public struct SpyNetworkElement : IBufferElementData
    {
        public Entity TargetNation;
        public int AgentCount;
        public float CoverageQuality; // 0-1
        public bool IsCompromised;
        public bool HasDoubleAgent;
        public float IntelFlowRate; // Bits per tick
        public int LastMajorSuccess; // Tick count
        public float RiskOfExposure; // 0-1 per tick
    }

    [Serializable]
    public struct CovertOperationElement : IBufferElementData
    {
        public int OperationType; // 0=Coup, 1=Sabotage, 2=Assassination, 3=Propaganda, 4=CyberAttack
        public Entity TargetEntity;
        public float SuccessProbability;
        public float ResourcesCommitted;
        public float Progress; // 0-1
        public bool IsDetected;
        public float PlausibleDeniability; // 0-1
        public int ExpectedCompletionTick;
    }

    public class IntelligenceAgencySystem : SystemBase
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

            // Update Agency Macro Metrics
            Entities
                .WithAll<IntelligenceAgencyComponent>()
                .ForEach((ref IntelligenceAgencyComponent agency) =>
                {
                    // 1. Network Security Dynamics
                    // More agents = higher risk of compromise
                    float agentRisk = agency.ActiveAgents * 0.0001f;
                    float counterIntelBenefit = agency.CounterSpyEffectiveness * 0.5f;
                    
                    float netRisk = agentRisk - counterIntelBenefit;
                    agency.NetworkSecurityScore = math.max(0f, math.min(1f, 
                        agency.NetworkSecurityScore - (netRisk * deltaTime)));

                    // 2. Double Agent Detection
                    if (agency.DoubleAgents > 0)
                    {
                        float detectionChance = agency.LeakDetectionRate * deltaTime;
                        int detected = (int)(agency.DoubleAgents * detectionChance);
                        if (detected > 0)
                        {
                            agency.DoubleAgents -= detected;
                            agency.MolesDetectedYTD += detected;
                            agency.NetworkSecurityScore = math.min(1f, agency.NetworkSecurityScore + 0.05f);
                        }
                    }

                    // 3. Covert Operation Success Rate
                    float baseSuccessRate = 0.7f;
                    float capacityBonus = (agency.HumanIntelligenceCapacity + agency.SignalsIntelligenceCapacity) * 0.1f;
                    float secPenalty = (1f - agency.NetworkSecurityScore) * 0.3f;
                    
                    float effectiveSuccessRate = math.max(0.1f, math.min(0.95f, 
                        baseSuccessRate + capacityBonus - secPenalty));

                    // Process active operations (simplified - detailed in buffer)
                    if (agency.ActiveCovertOps > 0)
                    {
                        int expectedSuccesses = (int)(agency.ActiveCovertOps * effectiveSuccessRate * deltaTime * 0.1f);
                        int expectedFailures = (int)(agency.ActiveCovertOps * (1f - effectiveSuccessRate) * deltaTime * 0.1f);
                        
                        agency.SuccessfulOpsYTD += expectedSuccesses;
                        agency.FailedOpsYTD += expectedFailures;
                        agency.ActiveCovertOps = math.max(0, agency.ActiveCovertOps - expectedSuccesses - expectedFailures);
                    }

                    // 4. OPSEC Breaches
                    if (agency.OpSecBreaches > 0)
                    {
                        // Breaches degrade network security
                        agency.NetworkSecurityScore *= (1f - (agency.OpSecBreaches * deltaTime * 0.01f));
                        agency.OpSecBreaches = math.max(0, agency.OpSecBreaches - deltaTime * 0.1f);
                    }

                    // 5. Global Coverage
                    agency.GlobalCoveragePercent = math.min(100f, agency.CountriesWithPresence * 0.5f);

                    _random = random;
                }).WithoutBurst().Run();

            // Update Spy Networks per Country
            Entities
                .WithAll<SpyNetworkElement>()
                .ForEach((ref SpyNetworkElement network) =>
                {
                    // Intel Flow based on coverage and security
                    if (!network.IsCompromised)
                    {
                        float flowEfficiency = network.CoverageQuality * (1f - network.RiskOfExposure);
                        network.IntelFlowRate = math.lerp(network.IntelFlowRate, flowEfficiency * 100f, deltaTime * 0.05f);
                        
                        // Chance of recruiting new assets
                        float recruitChance = network.CoverageQuality * deltaTime * 0.01f;
                        if (random.NextFloat() < recruitChance)
                        {
                            network.AgentCount++;
                            network.CoverageQuality = math.min(1f, network.CoverageQuality + 0.01f);
                        }
                    }
                    else
                    {
                        // Compromised network feeds false intel
                        network.IntelFlowRate = -math.abs(network.IntelFlowRate);
                        network.AgentCount = math.max(0, network.AgentCount - (int)(deltaTime * 0.5f));
                    }

                    // Exposure risk accumulates
                    network.RiskOfExposure = math.min(1f, network.RiskOfExposure + deltaTime * 0.001f);
                    
                    // If has double agent, high risk of major compromise
                    if (network.HasDoubleAgent)
                    {
                        network.RiskOfExposure = math.min(1f, network.RiskOfExposure + deltaTime * 0.01f);
                        if (network.RiskOfExposure > 0.9f && !network.IsCompromised)
                        {
                            network.IsCompromised = true;
                        }
                    }
                }).WithoutBurst().Run();

            // Update Covert Operations
            Entities
                .WithAll<CovertOperationElement>()
                .ForEach((ref CovertOperationElement op) =>
                {
                    // Progress toward completion
                    if (op.Progress < 1f)
                    {
                        float progressRate = op.ResourcesCommitted * 0.01f;
                        op.Progress = math.min(1f, op.Progress + (progressRate * deltaTime));
                        
                        // Detection risk during operation
                        float detectionRisk = (1f - op.PlausibleDeniability) * 0.001f;
                        if (random.NextFloat() < detectionRisk)
                        {
                            op.IsDetected = true;
                            op.PlausibleDeniability *= 0.5f;
                        }
                    }

                    // Adjust success probability based on operation type
                    switch (op.OperationType)
                    {
                        case 0: // Coup - hardest
                            op.SuccessProbability = math.min(op.SuccessProbability, 0.3f);
                            break;
                        case 1: // Sabotage
                            op.SuccessProbability = math.min(op.SuccessProbability, 0.6f);
                            break;
                        case 2: // Assassination
                            op.SuccessProbability = math.min(op.SuccessProbability, 0.5f);
                            break;
                        case 3: // Propaganda
                            op.SuccessProbability = math.min(op.SuccessProbability, 0.8f);
                            break;
                        case 4: // Cyber Attack
                            op.SuccessProbability = math.min(op.SuccessProbability, 0.7f);
                            break;
                    }
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to handle inter-agency coordination and intelligence sharing
    /// </summary>
    public class IntelligenceSharingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Five Eyes style alliances share intel
            // Adversaries feed disinformation
            
            Entities
                .WithAll<IntelligenceAgencyComponent>()
                .ForEach((ref IntelligenceAgencyComponent agency) =>
                {
                    // Allied sharing boosts SIGINT
                    float alliedBonus = 0.1f; // Mock alliance check
                    agency.SignalsIntelligenceCapacity = math.min(1f, 
                        agency.SignalsIntelligenceCapacity + (alliedBonus * Time.DeltaTime * 0.05f));
                    
                    // Adversary disinformation degrades HUMINT
                    float adversaryThreat = 0.05f; // Mock threat level
                    agency.HumanIntelligenceCapacity = math.max(0f, 
                        agency.HumanIntelligenceCapacity - (adversaryThreat * Time.DeltaTime * 0.02f));
                }).WithoutBurst().Run();
        }
    }
}
