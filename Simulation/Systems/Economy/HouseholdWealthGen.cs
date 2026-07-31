using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Economy
{
    /// <summary>
    /// Household economy simulation with income distribution,
    /// consumption patterns, savings behavior, and wealth accumulation.
    /// Tracks Gini coefficient and wealth inequality metrics.
    /// </summary>
    [Serializable]
    public struct Household : IComponentData
    {
        public int HouseholdID;
        public int RegionID;
        public HouseholdType Type;
        public int MemberCount;
        
        // Income sources
        public float WageIncome;
        public float BusinessIncome;
        public float InvestmentIncome;
        public float GovernmentTransfers;
        public float TotalIncome;
        public float DisposableIncome; // After taxes
        
        // Expenses
        public float ConsumptionTotal;
        public float ConsumptionFood;
        public float ConsumptionHousing;
        public float ConsumptionHealthcare;
        public float ConsumptionEducation;
        public float ConsumptionLuxury;
        public float ConsumptionSavings;
        public float ConsumptionTaxes;
        
        // Wealth
        public float CashSavings;
        public float BankDeposits;
        public float StockHoldings;
        public float BondHoldings;
        public float RealEstateValue;
        public float VehicleValue;
        public float TotalAssets;
        public float TotalLiabilities; // Debts
        public float NetWorth;
        
        // Economic behavior
        public float MarginalPropensityToConsume; // MPC
        public float MarginalPropensityToSave; // MPS
        public float RiskTolerance; // 0-1
        public float ConsumerConfidence; // 0-1
        public EmploymentStatus EmploymentStatus;
        
        // Demographics
        public int AgeHead;
        public int EducationLevel; // 0-5 scale
        public float SkillLevel; // 0-1
    }
    
    public enum HouseholdType
    {
        Single,
        Couple,
        FamilyWithChildren,
        SingleParent,
        Retired,
        Student
    }
    
    public enum EmploymentStatus
    {
        EmployedFullTime,
        EmployedPartTime,
        SelfEmployed,
        Unemployed,
        Retired,
        Student,
        Disabled,
        DiscouragedWorker // Stopped looking for work
    }
    
    [Serializable]
    public struct WealthDistribution : IComponentData
    {
        public int RegionID;
        
        // Gini coefficient calculation data
        public float GiniCoefficient; // 0 = perfect equality, 1 = perfect inequality
        public float PalmaRatio; // Top 10% / Bottom 40%
        public float TheilIndex; // Entropy-based inequality measure
        
        // Wealth quintiles
        public float Quintile1Share; // Bottom 20%
        public float Quintile2Share;
        public float Quintile3Share;
        public float Quintile4Share;
        public float Quintile5Share; // Top 20%
        
        // Top wealth shares
        public float Top1PercentShare;
        public float Top0_1PercentShare;
        public float Top0_01PercentShare;
        
        // Median vs Mean
        public float MedianWealth;
        public float MeanWealth;
        public float MedianIncome;
        public float MeanIncome;
        
        // Mobility metrics
        public float IntergenerationalMobility; // Correlation parent-child income
        public float AbsoluteMobilityRate; // % earning more than parents
        public float RelativeMobilityRate; // Chance of moving quintiles
        
        // Temporal
        public int LastCalculationTick;
        public float GiniTrend; // Rate of change
    }
    
    [Serializable]
    public struct ConsumerBehavior : IComponentData
    {
        public int ConsumerSegmentID;
        public ConsumerSegmentType SegmentType;
        
        // Spending patterns
        public float EngelCoefficient; // Food spending / Total spending
        public float SavingsRate;
        public float DebtServiceRatio; // Debt payments / Income
        
        // Brand loyalty and preferences
        public float BrandLoyaltyIndex;
        public float PriceSensitivity;
        public float QualitySensitivity;
        public float EnvironmentalConcern;
        
        // Adoption curves
        public float InnovationAdoptionRate; // Early adopters
        public float TrendFollowingTendency;
        
        // Response to economic conditions
        public float RecessionSpendingCut; // % reduction in downturns
        public float BoomSpendingIncrease; // % increase in upturns
        public bool IsConsumptionSmoothing; // Maintain spending despite income changes
    }
    
    public enum ConsumerSegmentType
    {
        BudgetConstrained,      // Low income, high price sensitivity
        ValueSeekers,           // Middle income, quality/price balance
        QualityFocused,         // Upper middle, quality over price
        LuxuryConsumers,        // High income, status-driven
        ConspicuousConsumers,   // Wealth display focused
        Minimalists,            // Anti-consumption
        EthicalConsumers        // Environment/social focused
    }
    
    [Serializable]
    public struct LaborMarket : IComponentData
    {
        public int RegionID;
        
        // Employment metrics
        public float LaborForceParticipationRate;
        public float UnemploymentRate;
        public float NaturalUnemploymentRate; // NAIRU
        public float UnderemploymentRate; // Part-time wanting full-time
        public float LongTermUnemploymentRate; // >6 months unemployed
        
        // Job market dynamics
        public float JobCreationRate;
        public float JobDestructionRate;
        public float JobOpeningsRate;
        public float QuitRate; // Voluntary separations
        public float LayoffRate;
        public float HireRate;
        
        // Wage metrics
        public float AverageHourlyEarnings;
        public float MedianHourlyWage;
        public float MinimumWage;
        public float LivingWage; // Estimated cost of living wage
        public float WageGrowthRate;
        public float RealWageGrowth; // Adjusted for inflation
        
        // Skills mismatch
        public float SkillsMismatchIndex;
        public float StructuralUnemployment;
        public float FrictionalUnemployment;
        public float CyclicalUnemployment;
        
        // Productivity
        public float LaborProductivity;
        public float ProductivityGrowthRate;
    }
    
    public class HouseholdEconomySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Calculate household budgets
            // Update consumption based on income and confidence
            // Track wealth accumulation/depletion
            // Calculate regional Gini coefficients
            // Process labor market dynamics
            // Simulate intergenerational wealth transfer
        }
    }
    
    /// <summary>
    /// Calculates Gini coefficient using the Lorenz curve method.
    /// </summary>
    public static class InequalityCalculator
    {
        public static float CalculateGini(NativeArray<float> incomes)
        {
            // Sort incomes
            incomes.Sort();
            
            int n = incomes.Length;
            if (n == 0) return 0f;
            
            float totalIncome = 0f;
            for (int i = 0; i < n; i++)
            {
                totalIncome += incomes[i];
            }
            
            if (totalIncome == 0f) return 0f;
            
            // Calculate Gini using the formula: G = (2 * sum(i * x_i)) / (n * sum(x_i)) - (n+1)/n
            float weightedSum = 0f;
            for (int i = 0; i < n; i++)
            {
                weightedSum += (i + 1) * incomes[i];
            }
            
            float gini = (2f * weightedSum) / (n * totalIncome) - ((float)(n + 1) / n);
            return math.max(0f, math.min(1f, gini));
        }
        
        public static float CalculatePalmaRatio(NativeArray<float> incomes)
        {
            incomes.Sort();
            int n = incomes.Length;
            if (n < 10) return 1f;
            
            int top10Start = (int)(n * 0.9f);
            int bottom40End = (int)(n * 0.4f);
            
            float top10Income = 0f;
            for (int i = top10Start; i < n; i++)
            {
                top10Income += incomes[i];
            }
            
            float bottom40Income = 0f;
            for (int i = 0; i < bottom40End; i++)
            {
                bottom40Income += incomes[i];
            }
            
            return bottom40Income > 0f ? top10Income / bottom40Income : 999f;
        }
    }
}
