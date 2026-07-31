using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Central banking system with monetary policy, interest rates,
    /// money supply tracking, and inflation control mechanisms.
    /// </summary>
    [Serializable]
    public struct CentralBank : IComponentData
    {
        public int BankID;
        public float BaseInterestRate;
        public float TargetInflationRate;
        public float CurrentInflationRate;
        public float MoneySupplyM1;
        public float MoneySupplyM2;
        public float ForeignReserves;
        public float GovernmentDebt;
        public float DebtToGDPRatio;
        
        // Policy tools
        public float ReserveRequirement;
        public float DiscountRate;
        public float QuantitativeEasingVolume;
        public bool IsQEActive;
        
        // Economic targets
        public float TargetUnemploymentRate;
        public float GDPGrowthTarget;
        
        // Temporal data
        public int LastPolicyChangeTick;
        public int PolicyCooldownTicks;
        public MonetaryPolicyStance CurrentStance;
    }
    
    public enum MonetaryPolicyStance
    {
        Accommodative,    // Low rates, high liquidity
        Neutral,          // Balanced
        Restrictive,      // High rates, low liquidity
        QuantitativeEasing,
        QuantitativeTightening
    }
    
    [Serializable]
    public struct CommercialBank : IComponentData
    {
        public int BankID;
        public int RegionID;
        public float TotalAssets;
        public float TotalDeposits;
        public float TotalLoans;
        public float NonPerformingLoans;
        public float NPLRatio; // Non-performing loan ratio
        public float CapitalAdequacyRatio;
        public float LiquidityRatio;
        public float LendingRate; // Interest rate charged to borrowers
        public float DepositRate; // Interest rate paid to depositors
        public float Spread; // Difference between lending and deposit rates
        
        // Risk metrics
        public float CreditRiskExposure;
        public float MarketRiskExposure;
        public float OperationalRiskScore;
        public BankHealthStatus HealthStatus;
        
        // Operations
        public float LoanOriginationRate;
        public float DefaultRate;
        public int ActiveLoanCount;
    }
    
    public enum BankHealthStatus
    {
        Healthy,
        Stressed,
        Critical,
        Insolvent,
        UnderCapitalized
    }
    
    [Serializable]
    public struct StockMarket : IComponentData
    {
        public int MarketID;
        public string MarketName;
        public float TotalMarketCap;
        public float MainIndex; // e.g., S&P equivalent
        public float IndexChangePercent;
        public float VolatilityIndex; // VIX equivalent
        public float PERatio; // Average P/E ratio
        public float DividendYield;
        public float TradingVolume;
        public bool IsOpen;
        public float CircuitBreakerLevel; // Threshold for trading halt
        public bool IsHalted; // Trading halted due to crash
        
        // Sector indices
        public float TechnologyIndex;
        public float FinancialIndex;
        public float EnergyIndex;
        public float HealthcareIndex;
        public float ConsumerIndex;
        public float IndustrialIndex;
    }
    
    [Serializable]
    public struct PubliclyTradedCompany : IComponentData
    {
        public int CompanyID;
        public int StockMarketID;
        public string TickerSymbol;
        public float SharePrice;
        public long SharesOutstanding;
        public float MarketCap;
        public float EarningsPerShare;
        public float PriceToEarningsRatio;
        public float PriceToBookRatio;
        public float DebtToEquityRatio;
        public float ReturnOnEquity;
        public float ProfitMargin;
        public float RevenueGrowthRate;
        public float DividendPerShare;
        public float Beta; // Market correlation
        
        // Technical indicators
        public float MovingAverage50;
        public float MovingAverage200;
        public float RelativeStrengthIndex; // RSI
        public TrendDirection Trend;
    }
    
    public enum TrendDirection
    {
        StrongUptrend,
        Uptrend,
        Sideways,
        Downtrend,
        StrongDowntrend
    }
    
    [Serializable]
    public struct EconomicIndicator : IComponentData
    {
        public IndicatorType Type;
        public float CurrentValue;
        public float PreviousValue;
        public float YearAgoValue;
        public float Trend; // Rate of change
        public float Volatility;
        public int LastUpdateTick;
        public bool IsLeading; // Predicts future economic activity
        public bool IsLagging; // Confirms past trends
        public bool IsCoincident; // Moves with economy
    }
    
    public enum IndicatorType
    {
        GDP,
        InflationCPI,
        InflationPPI,
        UnemploymentRate,
        ConsumerConfidence,
        ManufacturingPMI,
        ServicesPMI,
        RetailSales,
        HousingStarts,
        TradeBalance,
        BudgetDeficit,
        Productivity,
        WageGrowth
    }
    
    public class BankingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Process interest rate transmission
            // Calculate loan defaults based on economic conditions
            // Update bank health metrics
            // Simulate stock market movements
        }
    }
}
