using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Crypto and Shadow Economy System simulating decentralized finance, blockchain mining,
    /// money laundering, tax havens, informal markets, and currency collapse scenarios.
    /// </summary>
    [Serializable]
    public struct ShadowEconomyComponent : IComponentData
    {
        // Cryptocurrency Metrics
        public float TotalCryptoMarketCap;
        public float BitcoinDominance;
        public float StablecoinVolume;
        public float DefiTotalValueLocked;
        public float MiningHashRate;
        public float MiningEnergyConsumptionTWh;
        
        // Shadow Economy Size
        public float InformalEconomySizePercent; // % of GDP
        public float CashInCirculationRatio; // High ratio indicates shadow activity
        public float BarterTradeVolume;
        
        // Illicit Finance
        public double MoneyLaunderingVolume;
        public double TerroristFinancingVolume;
        public double TaxEvasionAmount;
        public int ActiveTaxHavens;
        public float ShellCompanyCount;
        
        // Currency Stability
        public float HyperinflationRisk; // 0-1 probability
        public float DollarizationLevel; // % of transactions in foreign currency
        public float BlackMarketExchangePremium; // % over official rate
        
        // Regulatory Pressure
        public float AmlEnforcementStrength; // Anti-Money Laundering
        public float KycComplianceRate; // Know Your Customer
        public float CryptoRegulationIndex; // 0 = banned, 1 = fully legal
    }

    [Serializable]
    public struct CryptoAssetElement : IBufferElementData
    {
        public int AssetType; // 0=BTC, 1=ETH, 2=Stablecoin, 3=PrivacyCoin, 4=CBDC
        public float MarketCap;
        public float DailyVolume;
        public float Volatility30Day;
        public float MiningDifficulty;
        public float TransactionFeeAvg;
        public bool IsPrivacyFocused;
        public bool IsStateBacked; // CBDC
        public float RegulatoryRiskScore; // 0-1
    }

    [Serializable]
    public struct InformalMarketElement : IBufferElementData
    {
        public int SectorType; // Agriculture, Construction, Domestic Work, etc.
        public float EstimatedSize;
        public float AvgWageVsFormal; // Ratio
        public bool IsIllegalGood; // Drugs, counterfeit, etc.
        public float EnforcementRisk; // Probability of crackdown
        public float CorruptionBribeRate; // Avg bribe as % of transaction
    }

    public class ShadowEconomySystem : SystemBase
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

            // Update Macro Shadow Economy Metrics
            Entities
                .WithAll<ShadowEconomyComponent>()
                .ForEach((ref ShadowEconomyComponent shadow) =>
                {
                    // 1. Cryptocurrency Dynamics
                    // Crypto grows when trust in fiat drops (high inflation, capital controls)
                    float fiatTrustDecay = shadow.HyperinflationRisk * 0.5f + (shadow.BlackMarketExchangePremium * 0.01f);
                    float cryptoAdoptionGrowth = math.max(0f, fiatTrustDecay - shadow.CryptoRegulationIndex * 0.2f);
                    
                    shadow.TotalCryptoMarketCap *= (1f + (cryptoAdoptionGrowth * deltaTime));
                    
                    // Mining adjusts to price and energy costs
                    float miningProfitability = (shadow.TotalCryptoMarketCap / 1000f) - (shadow.MiningEnergyConsumptionTWh * 0.05f);
                    if (miningProfitability > 0)
                    {
                        shadow.MiningHashRate *= (1f + (miningProfitability * deltaTime * 0.01f));
                        shadow.MiningEnergyConsumptionTWh += deltaTime * 0.001f * miningProfitability;
                    }
                    else
                    {
                        shadow.MiningHashRate *= (1f - (math.abs(miningProfitability) * deltaTime * 0.02f));
                        shadow.MiningEnergyConsumptionTWh = math.max(0, shadow.MiningEnergyConsumptionTWh - deltaTime * 0.001f);
                    }

                    // 2. Informal Economy Expansion
                    // Grows with high taxes, regulation, and weak enforcement
                    float taxBurden = 0.3f; // Assumed from Economy system
                    float regulatoryBurden = 1.0f - shadow.AmlEnforcementStrength;
                    float informalGrowthDriver = (taxBurden + regulatoryBurden) * 0.1f;
                    
                    shadow.InformalEconomySizePercent = math.min(80f, 
                        shadow.InformalEconomySizePercent + (informalGrowthDriver * deltaTime));
                    
                    // Cash ratio correlates with informal activity
                    shadow.CashInCirculationRatio = shadow.InformalEconomySizePercent * 0.015f;

                    // 3. Money Laundering & Tax Evasion
                    // Scales with informal economy and available havens
                    float launderingCapacity = shadow.ActiveTaxHavens * shadow.ShellCompanyCount * 0.0001f;
                    shadow.MoneyLaunderingVolume = math.min(shadow.TaxEvasionAmount * 0.5f, 
                        launderingCapacity * shadow.InformalEconomySizePercent);
                    
                    // Enforcement reduces evasion
                    float enforcementReduction = shadow.AmlEnforcementStrength * shadow.KycComplianceRate;
                    shadow.TaxEvasionAmount *= (1f - (enforcementReduction * deltaTime * 0.05f));

                    // 4. Currency Crisis Dynamics
                    // Black market premium explodes when reserves drop or inflation spikes
                    if (shadow.HyperinflationRisk > 0.5f)
                    {
                        shadow.BlackMarketExchangePremium *= (1f + (shadow.HyperinflationRisk * deltaTime * 0.2f));
                        shadow.DollarizationLevel = math.min(0.95f, shadow.DollarizationLevel + deltaTime * 0.01f);
                    }
                    else
                    {
                        shadow.BlackMarketExchangePremium = math.lerp(shadow.BlackMarketExchangePremium, 0.05f, deltaTime * 0.02f);
                        shadow.DollarizationLevel = math.max(0f, shadow.DollarizationLevel - deltaTime * 0.005f);
                    }

                    // 5. Privacy Coin Demand
                    // Increases with regulatory pressure
                    float privacyDemand = shadow.CryptoRegulationIndex * shadow.AmlEnforcementStrength;
                    // Logic handled in asset buffer iteration

                    _random = random;
                }).WithoutBurst().Run();

            // Update Crypto Assets
            Entities
                .WithAll<CryptoAssetElement>()
                .ForEach((ref CryptoAssetElement asset) =>
                {
                    // Privacy coins boom under regulation
                    if (asset.IsPrivacyFocused)
                    {
                        float regulatoryPush = 1.0f - asset.RegulatoryRiskScore;
                        asset.MarketCap *= (1f + (regulatoryPush * Time.DeltaTime * 0.05f));
                        asset.TransactionFeeAvg *= (1f + Time.DeltaTime * 0.01f); // High demand = high fees
                    }
                    // CBDCs grow with state support
                    else if (asset.IsStateBacked)
                    {
                        asset.MarketCap *= (1f + Time.DeltaTime * 0.02f);
                        asset.Volatility30Day = math.min(asset.Volatility30Day, 0.01f); // Stable by design
                    }
                    // Standard crypto follows market cycles
                    else
                    {
                        float marketMomentum = random.NextFloat(-0.05f, 0.05f);
                        asset.MarketCap *= (1f + (marketMomentum * Time.DeltaTime));
                        asset.Volatility30Day = math.lerp(asset.Volatility30Day, 0.6f, Time.DeltaTime * 0.01f);
                    }

                    // Mining difficulty adjusts to hash rate (simplified)
                    if (asset.AssetType == 0 || asset.AssetType == 1) // BTC/ETH
                    {
                        asset.MiningDifficulty *= (1f + Time.DeltaTime * 0.001f);
                    }
                }).WithoutBurst().Run();

            // Update Informal Markets
            Entities
                .WithAll<InformalMarketElement>()
                .ForEach((ref InformalMarketElement market) =>
                {
                    // Illegal goods have higher margins but higher risk
                    if (market.IsIllegalGood)
                    {
                        float riskPremium = market.EnforcementRisk * 0.5f;
                        market.AvgWageVsFormal = 2.0f + riskPremium; // 2x formal wage + risk premium
                        market.CorruptionBribeRate = market.EnforcementRisk * 0.3f;
                    }
                    else
                    {
                        // Legal informal sector (e.g., street vendors, day labor)
                        market.AvgWageVsFormal = math.lerp(market.AvgWageVsFormal, 0.7f, Time.DeltaTime * 0.01f);
                        market.CorruptionBribeRate = math.lerp(market.CorruptionBribeRate, 0.05f, Time.DeltaTime * 0.02f);
                    }

                    // Enforcement crackdowns reduce size temporarily
                    if (market.EnforcementRisk > 0.8f)
                    {
                        market.EstimatedSize *= (1f - Time.DeltaTime * 0.1f);
                    }
                    else
                    {
                        market.EstimatedSize *= (1f + Time.DeltaTime * 0.005f);
                    }
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to simulate bank runs and currency collapses
    /// </summary>
    public class FinancialCrisisSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ShadowEconomyComponent>()
                .ForEach((ref ShadowEconomyComponent shadow) =>
                {
                    // Minsky Moment Trigger
                    // When private debt + leverage exceeds threshold, crisis probability spikes
                    float systemicLeverage = 2.5f; // Mock value from Banking system
                    float crisisThreshold = 3.0f;
                    
                    if (systemicLeverage > crisisThreshold)
                    {
                        shadow.HyperinflationRisk = math.min(1.0f, shadow.HyperinflationRisk + Time.DeltaTime * 0.05f);
                        shadow.BlackMarketExchangePremium *= (1f + Time.DeltaTime * 0.1f);
                    }
                    else
                    {
                        shadow.HyperinflationRisk = math.max(0f, shadow.HyperinflationRisk - Time.DeltaTime * 0.01f);
                    }

                    // Capital Flight Detection
                    // If black market premium > 20%, capital controls likely
                    if (shadow.BlackMarketExchangePremium > 0.2f)
                    {
                        shadow.CryptoRegulationIndex = math.max(0f, shadow.CryptoRegulationIndex - Time.DeltaTime * 0.02f);
                        shadow.InformalEconomySizePercent += Time.DeltaTime * 0.01f;
                    }
                }).WithoutBurst().Run();
        }
    }
}
