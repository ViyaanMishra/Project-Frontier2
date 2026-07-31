using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Global Trade Network System simulating international commerce, shipping logistics,
    /// port congestion, trade embargoes, and balance of payments with high-fidelity modeling.
    /// </summary>
    [Serializable]
    public struct TradeNetworkComponent : IComponentData
    {
        // Global Metrics
        public float GlobalTradeVolume;
        public float GlobalTradeGrowthYoY;
        public float ProtectionismIndex; // 0-1, higher = more tariffs/barriers
        
        // Balance of Payments
        public double CurrentAccountBalance;
        public double CapitalAccountBalance;
        public double ForeignReserves;
        public double ExternalDebt;
        
        // Exchange Rates (vs USD baseline)
        public float ExchangeRate;
        public float ExchangeRateVolatility;
        
        // Logistics Health
        public float GlobalShippingCapacityUtilization; // 0-1
        public float AvgPortWaitTimeDays;
        public float ContainerFreightIndex; // Price index
        public int BlockedChokepoints; // e.g., Suez, Panama
        
        // Sanctions & Embargoes
        public int ActiveSanctions;
        public int ActiveEmbargoes;
        public float SanctionsEvasionRate; // % of trade that bypasses sanctions
        
        // Supply Chain Resilience
        public float SupplyChainDiversificationIndex; // Higher = less fragile
        public float JustInTimeVulnerability; // Higher = more prone to disruption
    }

    [Serializable]
    public struct TradeRouteElement : IBufferElementData
    {
        public Entity OriginCountry;
        public Entity DestinationCountry;
        public int CommodityType; // Enum mapping
        public float VolumePerYear;
        public float TariffRate;
        public float TransportCostPerUnit;
        public float TransitTimeDays;
        public bool IsEmbargoed;
        public bool IsSanctioned;
        public float SmugglingVolume; // Illegal trade
        public float RouteSecurityRisk; // 0-1 piracy/war risk
    }

    public class GlobalTradeSystem : SystemBase
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

            // Update Global Trade Aggregates
            Entities
                .WithAll<TradeNetworkComponent>()
                .ForEach((ref TradeNetworkComponent trade) =>
                {
                    // 1. Calculate Protectionism Impact
                    // Higher protectionism reduces volume growth
                    float baseGrowth = 0.03f; // 3% natural growth
                    float protectionismDrag = trade.ProtectionismIndex * 0.05f;
                    float effectiveGrowth = math.max(-0.1f, baseGrowth - protectionismDrag);
                    
                    trade.GlobalTradeGrowthYoY = math.lerp(trade.GlobalTradeGrowthYoY, effectiveGrowth, deltaTime * 0.1f);
                    trade.GlobalTradeVolume *= (1f + (trade.GlobalTradeGrowthYoY * deltaTime));

                    // 2. Logistics Congestion Model
                    // If volume approaches capacity, wait times explode non-linearly
                    float capacityHeadroom = 1.0f - trade.GlobalShippingCapacityUtilization;
                    if (capacityHeadroom < 0.1f)
                    {
                        // Exponential queue growth when near capacity
                        trade.AvgPortWaitTimeDays = math.lerp(trade.AvgPortWaitTimeDays, 
                            2.0f + (1.0f / (capacityHeadroom + 0.001f)) * 0.5f, 
                            deltaTime * 0.2f);
                        trade.ContainerFreightIndex *= (1f + deltaTime * 0.1f); // Prices spike
                    }
                    else
                    {
                        trade.AvgPortWaitTimeDays = math.lerp(trade.AvgPortWaitTimeDays, 1.5f, deltaTime * 0.05f);
                        trade.ContainerFreightIndex = math.lerp(trade.ContainerFreightIndex, 100f, deltaTime * 0.02f);
                    }

                    // 3. Chokepoint Disruption
                    if (trade.BlockedChokepoints > 0)
                    {
                        float disruptionFactor = trade.BlockedChokepoints * 0.15f; // 15% loss per chokepoint
                        trade.GlobalShippingCapacityUtilization += disruptionFactor;
                        trade.TransitTimeDays = transitTimeDays + (trade.BlockedChokepoints * 5f); // Simplified
                    }

                    // 4. Sanctions Evasion
                    // Smuggling increases with sanction severity
                    float totalSanctionedVolume = trade.ActiveSanctions * 1000f; // Mock volume
                    trade.SmugglingVolume = totalSanctionedVolume * trade.SanctionsEvasionRate;
                    
                    // 5. Exchange Rate Dynamics
                    // Trade surplus strengthens currency
                    float tradeBalanceEffect = (float)trade.CurrentAccountBalance * 0.0001f;
                    trade.ExchangeRate *= (1f + (tradeBalanceEffect * deltaTime));
                    trade.ExchangeRateVolatility = math.abs(trade.ExchangeRateVolatility * 0.99f) + (math.abs(tradeBalanceEffect) * deltaTime);

                    // 6. Supply Chain Resilience
                    // Diversification reduces JIT vulnerability
                    trade.JustInTimeVulnerability = math.max(0f, 1.0f - trade.SupplyChainDiversificationIndex);
                    
                    _random = random;
                }).WithoutBurst().Run();

            // Update Individual Trade Routes
            Entities
                .WithAll<TradeRouteElement>()
                .ForEach((ref TradeRouteElement route) =>
                {
                    if (route.IsEmbargoed || route.IsSanctioned)
                    {
                        // Legal trade drops, smuggling rises
                        float legalReduction = deltaTime * 0.5f;
                        route.VolumePerYear = math.max(0, route.VolumePerYear - (route.VolumePerYear * legalReduction));
                        
                        // Smuggling fills the gap based on evasion rate
                        float smuggledIncrease = route.VolumePerYear * 0.8f * 0.3f * deltaTime; // 30% evasion capture
                        route.SmugglingVolume += smuggledIncrease;
                        
                        // Risk premium on transport cost
                        route.TransportCostPerUnit *= (1f + deltaTime * 0.05f);
                    }
                    else
                    {
                        // Normal route dynamics
                        // Security risk affects cost
                        if (route.RouteSecurityRisk > 0.5f)
                        {
                            route.TransportCostPerUnit *= (1f + (route.RouteSecurityRisk * deltaTime * 0.1f));
                        }
                        else
                        {
                            route.TransportCostPerUnit = math.lerp(route.TransportCostPerUnit, 10f, deltaTime * 0.01f);
                        }
                    }
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to calculate comparative advantage and trade flow optimization
    /// </summary>
    public class TradeFlowOptimizationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Implement gravity model of trade: Flow = (GDP_i * GDP_j) / Distance^2
            // Adjusted for tariffs, common language, colonial history, etc.
            
            Entities
                .WithAll<TradeRouteElement>()
                .ForEach((ref TradeRouteElement route) =>
                {
                    // Gravity Model Calculation (simplified)
                    // In full implementation, would fetch GDP components from Origin/Destination entities
                    
                    float economicMass = 10000f; // Mock combined GDP in billions
                    float distanceFactor = 1.0f / (route.TransitTimeDays * route.TransitTimeDays);
                    
                    float potentialFlow = economicMass * distanceFactor;
                    
                    // Adjust for tariffs
                    float tariffBarrier = 1.0f - route.TariffRate;
                    
                    // Target volume
                    float targetVolume = potentialFlow * tariffBarrier * 0.01f; // Scaling factor
                    
                    // Smoothly adjust current volume to target
                    route.VolumePerYear = math.lerp(route.VolumePerYear, targetVolume, Time.DeltaTime * 0.05f);
                }).WithoutBurst().Run();
        }
    }
}
