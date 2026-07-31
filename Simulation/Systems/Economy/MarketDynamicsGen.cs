using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Component representing a market node in the global economy simulation.
    /// Tracks supply, demand, prices, and inventory for multiple commodities.
    /// </summary>
    [Serializable]
    public struct MarketNode : IComponentData
    {
        public int MarketID;
        public float2 Position; // Regional coordinates
        public MarketType Type;
        public int ConnectedMarketsCount;
        
        // Commodity tracking (simplified for 5 key resources)
        public CommodityState GrainState;
        public CommodityState MetalState;
        public CommodityState EnergyState;
        public CommodityState LuxuryState;
        public CommodityState TechState;
        
        // Economic indicators
        public float LocalInflationRate;
        public float UnemploymentRate;
        public float AverageWage;
        public float TaxRate;
        public float CorruptionIndex; // 0-1
        
        // Temporal data
        public int LastUpdateTick;
        public float PriceVolatility;
    }

    public enum MarketType
    {
        Rural,
        Urban,
        Industrial,
        Commercial,
        Port,
        BlackMarket
    }

    [Serializable]
    public struct CommodityState
    {
        public float CurrentSupply;
        public float CurrentDemand;
        public float BasePrice;
        public float CurrentPrice;
        public float PriceElasticity;
        public float InventoryLevel;
        public float ProductionRate;
        public float ConsumptionRate;
        public float ImportRate;
        public float ExportRate;
        public float PriceTrend; // Derivative of price over time
        public float SupplyShockModifier; // External events
        public int StockpileDays; // Days of supply remaining
        
        public void CalculateEquilibriumPrice(float globalBasePrice)
        {
            // Supply-Demand equilibrium with elasticity
            float supplyDemandRatio = math.max(0.01f, CurrentSupply / math.max(0.01f, CurrentDemand));
            float elasticityFactor = math.pow(supplyDemandRatio, -PriceElasticity);
            
            CurrentPrice = globalBasePrice * elasticityFactor * SupplyShockModifier;
            
            // Add some inertia/smoothing
            CurrentPrice = math.lerp(CurrentPrice, CurrentPrice, 0.1f);
            
            // Update trend
            PriceTrend = (CurrentPrice - BasePrice) / math.max(1, LastUpdateTick);
        }
        
        public int LastUpdateTick;
    }

    [Serializable]
    public struct TradeRoute : IComponentData
    {
        public int RouteID;
        public int SourceMarketID;
        public int DestinationMarketID;
        public float Distance;
        public float TransportCostMultiplier;
        public float Capacity;
        public float CurrentFlow;
        public bool IsActive;
        public bool IsBlocked; // Due to disasters, war, etc.
        public float BlockadeDuration;
        public int GoodsInTransit;
        public float TransitTime;
    }

    [Serializable]
    public struct EconomicPolicy : IComponentData
    {
        public PolicyType Type;
        public float Intensity; // 0-1
        public int DurationTicks;
        public int RemainingTicks;
        public float BudgetAllocation;
        public bool IsActive;
        public int TargetMarketID;
        public CommodityType TargetCommodity;
    }

    public enum PolicyType
    {
        Tariff,
        Subsidy,
        PriceControl,
        QuantitativeEasing,
        Austerity,
        Stimulus,
        Rationing,
        Embargo,
        MinimumWage,
        TaxCut
    }

    public enum CommodityType
    {
        Grain,
        Metal,
        Energy,
        Luxury,
        Tech
    }

    public class EconomySystemBase : SystemBase
    {
        protected override void OnUpdate()
        {
            // Core economic simulation logic implemented in specialized systems
        }
    }
}
