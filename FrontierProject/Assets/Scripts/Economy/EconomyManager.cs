using System;
using UnityEngine;
using Frontier.Core;

namespace Frontier.Economy
{
    /// <summary>
    /// Manages supply/demand pricing, currencies, trade routes, and market dynamics.
    /// Integrated with the central EventBus for cross-system communication.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }
        
        [SerializeField] private MarketData[] allMarkets;
        [SerializeField] private CurrencyDefinition[] currencies;
        [SerializeField] private float priceUpdateInterval = 60f; // Update every minute
        
        private NativeHashMap<int, MarketState> marketStates;
        private NativeHashMap<int, float> globalPrices; // Item ID -> base price
        private float lastUpdateTime;
        
        public enum CurrencyType
        {
            Scrap,          // Common currency
            Credits,        // Faction currency
            AnomalyShards,  // Rare currency
            Fuel,           // Barter currency
            FoodUnits,      // Barter currency
            DataCores       // Tech currency
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Initialize();
        }
        
        private void Initialize()
        {
            marketStates = new NativeHashMap<int, MarketState>(allMarkets.Length);
            globalPrices = new NativeHashMap<int, float>(100);
            
            foreach (var market in allMarkets)
            {
                var state = new MarketState
                {
                    marketId = market.id,
                    locationId = market.locationId,
                    factionId = market.factionId,
                    supply = new float[market.tradableItems.Length],
                    demand = new float[market.tradableItems.Length],
                    prices = new float[market.tradableItems.Length],
                    tradeVolume = 0,
                    lastTradeTime = 0
                };
                
                // Initialize supply/demand/prices
                for (int i = 0; i < market.tradableItems.Length; i++)
                {
                    state.supply[i] = market.baseSupply[i];
                    state.demand[i] = market.baseDemand[i];
                    state.prices[i] = CalculatePrice(market.tradableItems[i], state.supply[i], state.demand[i]);
                    
                    // Set global price if not set
                    if (!globalPrices.TryGetValue(market.tradableItems[i], out _))
                    {
                        globalPrices.Add(market.tradableItems[i], market.basePrices[i]);
                    }
                }
                
                marketStates.Add(market.id, state);
            }
        }
        
        private float CalculatePrice(int itemId, float supply, float demand)
        {
            if (!globalPrices.TryGetValue(itemId, out float basePrice))
                basePrice = 10f;
            
            // Supply/demand modifier
            float ratio = demand / Mathf.Max(supply, 1);
            float priceModifier = Mathf.Clamp(ratio, 0.2f, 5f);
            
            return basePrice * priceModifier;
        }
        
        public float GetPrice(int marketId, int itemId)
        {
            if (!marketStates.TryGetValue(marketId, out var state))
                return GetGlobalPrice(itemId);
            
            var market = GetMarketData(marketId);
            for (int i = 0; i < market.tradableItems.Length; i++)
            {
                if (market.tradableItems[i] == itemId)
                    return state.prices[i];
            }
            
            return GetGlobalPrice(itemId);
        }
        
        public float GetGlobalPrice(int itemId)
        {
            return globalPrices.TryGetValue(itemId, out float price) ? price : 10f;
        }
        
        public bool ExecuteTrade(int marketId, int itemId, int quantity, bool isBuying)
        {
            if (!marketStates.TryGetValue(marketId, out var state))
                return false;
            
            var market = GetMarketData(marketId);
            int itemIndex = Array.IndexOf(market.tradableItems, itemId);
            if (itemIndex < 0) return false;
            
            float price = state.prices[itemIndex];
            float totalCost = price * quantity;
            
            if (isBuying)
            {
                // Player buying from market
                if (state.supply[itemIndex] < quantity)
                    return false; // Not enough supply
                
                state.supply[itemIndex] -= quantity;
                state.demand[itemIndex] += quantity * 0.1f; // Increase demand
            }
            else
            {
                // Player selling to market
                state.supply[itemIndex] += quantity;
                state.demand[itemIndex] = Mathf.Max(0, state.demand[itemIndex] - quantity * 0.1f);
            }
            
            // Update price based on new supply/demand
            state.prices[itemIndex] = CalculatePrice(itemId, state.supply[itemIndex], state.demand[itemIndex]);
            
            state.tradeVolume += quantity;
            state.lastTradeTime = Time.time;
            
            marketStates[marketId] = state;
            
            // Fire event through unified EventBus
            EventBus<TradeExecuted>.Raise(new TradeExecuted
            {
                marketId = marketId,
                itemId = itemId,
                quantity = quantity,
                price = price,
                isBuying = isBuying
            });
            
            // Also publish economy fluctuation event for narrative system integration
            EventBus<EconomyFluctuationEvent>.Raise(new EconomyFluctuationEvent
            {
                ItemID = itemId,
                PriceChange = price,
                MarketID = marketId,
                EventType = isBuying ? EconomyFluctuationType.BuyPressure : EconomyFluctuationType.SellPressure
            });
            
            return true;
        }
        
        public void UpdateTradeRoute(int routeId, bool wasSuccessful)
        {
            // Successful trade routes increase supply at destination
            // Failed routes (ambushed) decrease supply and increase prices
            var route = GetTradeRoute(routeId);
            if (route == null) return;
            
            if (wasSuccessful)
            {
                if (marketStates.TryGetValue(route.destinationMarketId, out var destState))
                {
                    var market = GetMarketData(route.destinationMarketId);
                    for (int i = 0; i < route.cargoItems.Length; i++)
                    {
                        int itemIndex = Array.IndexOf(market.tradableItems, route.cargoItems[i]);
                        if (itemIndex >= 0)
                        {
                            destState.supply[itemIndex] += route.cargoQuantities[i];
                            destState.prices[itemIndex] = CalculatePrice(
                                route.cargoItems[i], 
                                destState.supply[itemIndex], 
                                destState.demand[itemIndex]
                            );
                        }
                    }
                    marketStates[route.destinationMarketId] = destState;
                }
            }
            else
            {
                // Ambushed - cargo lost, prices spike
                if (marketStates.TryGetValue(route.destinationMarketId, out var destState))
                {
                    var market = GetMarketData(route.destinationMarketId);
                    for (int i = 0; i < route.cargoItems.Length; i++)
                    {
                        int itemIndex = Array.IndexOf(market.tradableItems, route.cargoItems[i]);
                        if (itemIndex >= 0)
                        {
                            destState.demand[itemIndex] *= 1.5f; // Spike demand
                            destState.prices[itemIndex] *= 1.3f; // Price increase
                        }
                    }
                    marketStates[route.destinationMarketId] = destState;
                }
            }
        }
        
        public void ApplyWorldEvent(WorldEventType eventType, int affectedItemId)
        {
            // Apply global price changes based on world events
            switch (eventType)
            {
                case WorldEventType.Scarcity:
                    // Reduce supply everywhere
                    foreach (var kvp in marketStates)
                    {
                        var state = kvp.Value;
                        var market = GetMarketData(kvp.Key);
                        for (int i = 0; i < market.tradableItems.Length; i++)
                        {
                            if (market.tradableItems[i] == affectedItemId)
                            {
                                state.supply[i] *= 0.7f;
                                state.prices[i] = CalculatePrice(affectedItemId, state.supply[i], state.demand[i]);
                            }
                        }
                        marketStates[kvp.Key] = state;
                    }
                    break;
                    
                case WorldEventType.Surplus:
                    // Increase supply
                    foreach (var kvp in marketStates)
                    {
                        var state = kvp.Value;
                        var market = GetMarketData(kvp.Key);
                        for (int i = 0; i < market.tradableItems.Length; i++)
                        {
                            if (market.tradableItems[i] == affectedItemId)
                            {
                                state.supply[i] *= 1.5f;
                                state.prices[i] = CalculatePrice(affectedItemId, state.supply[i], state.demand[i]);
                            }
                        }
                        marketStates[kvp.Key] = state;
                    }
                    break;
                    
                case WorldEventType.War:
                    // Weapons/ammo prices spike
                    break;
                    
                case WorldEventType.Disaster:
                    // Food/medicine prices spike
                    break;
            }
        }
        
        private MarketData GetMarketData(int marketId)
        {
            foreach (var market in allMarkets)
            {
                if (market.id == marketId) return market;
            }
            return default;
        }
        
        private TradeRoute GetTradeRoute(int routeId)
        {
            // Would fetch from a trade route database
            return null;
        }
        
        private void Update()
        {
            // Periodic price updates
            if (Time.time - lastUpdateTime >= priceUpdateInterval)
            {
                UpdatePrices();
                lastUpdateTime = Time.time;
            }
        }
        
        private void UpdatePrices()
        {
            // Gradually normalize prices toward global averages
            foreach (var kvp in marketStates)
            {
                var state = kvp.Value;
                var market = GetMarketData(kvp.Key);
                
                for (int i = 0; i < market.tradableItems.Length; i++)
                {
                    // Natural supply decay (consumption)
                    state.supply[i] = Mathf.Max(0, state.supply[i] - market.consumptionRates[i] * priceUpdateInterval);
                    
                    // Natural demand fluctuation
                    state.demand[i] = Mathf.MoveTowards(state.demand[i], market.baseDemand[i], 0.5f);
                    
                    // Recalculate price
                    state.prices[i] = CalculatePrice(market.tradableItems[i], state.supply[i], state.demand[i]);
                }
                
                marketStates[kvp.Key] = state;
            }
        }
    }
    
    [Serializable]
    public struct MarketData
    {
        public int id;
        public string name;
        public int locationId;
        public int factionId;
        public int[] tradableItems;
        public float[] baseSupply;
        public float[] baseDemand;
        public float[] basePrices;
        public float[] consumptionRates;
        public bool isBlackMarket;
        public int securityLevel;
    }
    
    [Serializable]
    public struct MarketState
    {
        public int marketId;
        public int locationId;
        public int factionId;
        public float[] supply;
        public float[] demand;
        public float[] prices;
        public int tradeVolume;
        public float lastTradeTime;
    }
    
    [Serializable]
    public struct CurrencyDefinition
    {
        public CurrencyType type;
        public string name;
        public string symbol;
        public Color color;
        public float exchangeRateToScrap;
        public bool isFactionSpecific;
        public int factionId;
    }
    
    [Serializable]
    public struct TradeRoute
    {
        public int id;
        public int originMarketId;
        public int destinationMarketId;
        public int[] cargoItems;
        public int[] cargoQuantities;
        public float travelTime;
        public float ambushRisk;
        public int caravanEntityId;
    }
    
    public enum WorldEventType
    {
        Scarcity,
        Surplus,
        War,
        Disaster,
        Discovery,
        Embargo,
        Festival
    }
    
    public struct TradeExecuted
    {
        public int marketId;
        public int itemId;
        public int quantity;
        public float price;
        public bool isBuying;
    }

    /// <summary>
    /// Event for economy fluctuations that can trigger narrative events.
    /// </summary>
    public struct EconomyFluctuationEvent
    {
        public int ItemID;
        public float PriceChange;
        public int MarketID;
        public EconomyFluctuationType EventType;
    }

    public enum EconomyFluctuationType
    {
        BuyPressure,
        SellPressure,
        Scarcity,
        Surplus,
        TradeRouteDisrupted,
        MarketCrash,
        MarketBoom
    }
}
