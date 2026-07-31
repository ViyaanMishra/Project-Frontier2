using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Advanced supply chain simulation with logistics bottlenecks,
    /// production chains, and multi-stage manufacturing.
    /// </summary>
    public class SupplyChainGen : SystemBase
    {
        private EntityQuery marketQuery;
        private EntityQuery tradeRouteQuery;
        private EntityQuery factoryQuery;
        
        protected override void OnCreate()
        {
            marketQuery = GetEntityQuery(typeof(MarketNode));
            tradeRouteQuery = GetEntityQuery(typeof(TradeRoute));
            factoryQuery = GetEntityQuery(typeof(FactoryNode));
            
            RequireForUpdate(marketQuery);
        }
        
        protected override void OnUpdate()
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
            
            // Process supply chain logistics
            new SupplyChainJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ECB = ecb,
                SimulationTick = (int)SystemAPI.Time.ElapsedTime
            }.ScheduleParallel();
        }
    }
    
    [Serializable]
    public struct FactoryNode : IComponentData
    {
        public int FactoryID;
        public int MarketID; // Associated market
        public FactoryType Type;
        public float Efficiency; // 0-1
        public float CapacityUtilization; // 0-1
        public int WorkerCount;
        public float AutomationLevel; // 0-1
        
        // Input/Output tracking
        public CommodityType InputCommodity;
        public float InputRate;
        public CommodityType OutputCommodity;
        public float OutputRate;
        public float ProductionCost;
        
        // Supply chain position
        public int Tier; // 0=raw extraction, 1=processing, 2=manufacturing, 3=assembly
        public NativeArray<int> SupplierFactoryIDs; // Dependencies
        public NativeArray<int> CustomerFactoryIDs; // Downstream
        
        // Operational state
        public bool IsOperational;
        public float DowntimeRemaining;
        public string DowntimeReason;
        public float MaintenanceLevel; // 0-1, degrades over time
        public float LastMaintenanceTick;
    }
    
    public enum FactoryType
    {
        Extraction, // Mines, farms, oil wells
        Processing, // Refineries, smelters
        Manufacturing, // Component production
        Assembly, // Final goods
        PowerPlant,
        LogisticsHub
    }
    
    [Serializable]
    public struct LogisticsBottleneck : IComponentData
    {
        public int LocationID; // Market or Route ID
        public BottleneckType Type;
        public float Severity; // 0-1
        public float ThroughputReduction; // Percentage
        public float DelayAdded; // Time units
        public int AffectedTradeRoutes;
        public float CostIncreaseMultiplier;
        public int DurationTicks;
        public int RemainingTicks;
        public string Cause; // Labor strike, disaster, war, infrastructure failure
    }
    
    public enum BottleneckType
    {
        Infrastructure,
        LaborShortage,
        Regulatory,
        ResourceScarcity,
        Geopolitical,
        NaturalDisaster,
        TechnicalFailure
    }
    
    [Serializable]
    public struct ProductionChain : IComponentData
    {
        public int ChainID;
        public CommodityType FinalProduct;
        public NativeArray<int> StageFactoryIDs; // Ordered list of factories
        public NativeArray<float> StageEfficiencies;
        public float TotalChainEfficiency;
        public float BottleneckFactor; // Determined by least efficient stage
        public float AverageProductionTime;
        public float TotalCost;
        public bool IsDisrupted;
        public int DisruptedStageIndex;
    }
    
    public struct SupplyChainJob : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        [ReadOnly] public int SimulationTick;
        
        public void Execute(in DynamicBuffer<FactoryLink> factoryLinks, in DynamicBuffer<SupplyLink> supplyLinks)
        {
            // Process factory dependencies and material flows
            for (int i = 0; i < factoryLinks.Length; i++)
            {
                var link = factoryLinks[i];
                // Check if upstream factory is operational
                // Calculate material flow based on efficiency and bottlenecks
                // Update inventory levels
            }
            
            for (int i = 0; i < supplyLinks.Length; i++)
            {
                var link = supplyLinks[i];
                // Process goods movement through supply chain
                // Apply bottleneck delays
                // Update transit times
            }
        }
    }
    
    public struct FactoryLink : IBufferElementData
    {
        public int SourceFactoryID;
        public int TargetFactoryID;
        public float FlowRate;
        public float Capacity;
        public bool IsBlocked;
    }
    
    public struct SupplyLink : IBufferElementData
    {
        public int SourceID;
        public int TargetID;
        public CommodityType Commodity;
        public float Quantity;
        public float TransitProgress; // 0-1
        public float ExpectedArrivalTick;
        public bool IsDelayed;
        public float DelayAmount;
    }
}
