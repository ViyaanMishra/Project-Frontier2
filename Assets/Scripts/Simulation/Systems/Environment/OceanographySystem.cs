using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Environment
{
    /// <summary>
    /// Oceanography System simulating thermohaline circulation, ocean acidification,
    /// dead zones, coral bleaching, and marine ecosystem dynamics.
    /// </summary>
    [Serializable]
    public struct OceanographyComponent : IComponentData
    {
        // Thermohaline Circulation (Global Conveyor Belt)
        public float AMOCStrength; // Atlantic Meridional Overturning Circulation (Sv)
        public float AMOCTrend; // Positive = strengthening, Negative = weakening
        public float PacificOverturningIndex;
        public float SouthernOceanMixingRate;
        
        // Physical Properties
        public float AvgSurfaceTemperature;
        public float AvgSalinity; // PSU (Practical Salinity Units)
        public float StratificationIndex; // 0-1, higher = more layered
        public float MixedLayerDepth; // meters
        
        // Chemical Properties
        public float SurfacePH; // Ocean acidification (pre-industrial: 8.2)
        public float AragoniteSaturation; // Critical for shell-forming organisms
        public float DissolvedOxygenLevel; // mmol/L
        public float CarbonUptakeRate; // GtC/year
        
        // Ecological Health
        public float CoralCoveragePercent;
        public float BleachingSeverity; // 0-1
        public float DeadZoneAreaKm2; // Hypoxic areas
        public float PhytoplanktonBiomass; // Base of food web
        public float FishStockHealth; // 0-1 relative to MSY
        
        // Sea Level & Ice
        public float GlobalMeanSeaLevel; // meters above baseline
        public float ThermalExpansionContribution;
        public float IceSheetMeltContribution;
        public float GlacierMeltContribution;
    }

    [Serializable]
    public struct OceanCurrentElement : IBufferElementData
    {
        public int CurrentType; // 0=Gyre, 1=Boundary, 2=Equatorial, 3=Deep
        public float VelocityMagnitude; // m/s
        public float Temperature; // °C
        public float Salinity; // PSU
        public float TransportVolume; // Sverdrups
        public bool IsWeakening;
        public float EddyKineticEnergy;
        public int RegionId;
    }

    [Serializable]
    public struct MarineEcosystemElement : IBufferElementData
    {
        public int EcosystemType; // 0=CoralReef, 1=KelpForest, 2=Mangrove, 3=OpenOcean, 4=DeepSea
        public float BiodiversityIndex; // Shannon index approximation
        public float PrimaryProductivity; // gC/m²/day
        public float TrophicLevelAvg;
        public bool IsHypoxic; // Dead zone
        public float AcidificationStress; // 0-1
        public float WarmingStress; // 0-1
        public float HumanImpactScore; // 0-1
    }

    public class OceanographySystem : SystemBase
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

            // Update Macro Oceanographic State
            Entities
                .WithAll<OceanographyComponent>()
                .ForEach((ref OceanographyComponent ocean) =>
                {
                    // 1. AMOC Weakening (Climate Change Impact)
                    // Freshwater from melting ice reduces salinity, weakens circulation
                    float freshwaterInput = ocean.IceSheetMeltContribution + ocean.GlacierMeltContribution;
                    float amocWeakeningRate = freshwaterInput * 0.05f;
                    
                    ocean.AMOCTrend = -amocWeakeningRate;
                    ocean.AMOCStrength *= (1f - (amocWeakeningRate * deltaTime));
                    
                    // Tipping point check (~30% reduction from normal ~15 Sv)
                    if (ocean.AMOCStrength < 10f)
                    {
                        // Potential collapse scenario
                        ocean.AMOCTrend *= 1.5f; // Accelerating decline
                        ocean.StratificationIndex = math.min(1f, ocean.StratificationIndex + deltaTime * 0.01f);
                    }

                    // 2. Ocean Acidification
                    // CO2 absorption lowers pH
                    float co2Absorption = ocean.CarbonUptakeRate * 0.0001f;
                    ocean.SurfacePH = math.max(7.5f, ocean.SurfacePH - (co2Absorption * deltaTime));
                    
                    // Aragonite saturation drops with lower pH
                    ocean.AragoniteSaturation *= (1f - (co2Absorption * deltaTime * 0.5f));
                    
                    // Calcifying organisms stressed below saturation = 1
                    if (ocean.AragoniteSaturation < 1.0f)
                    {
                        ocean.CoralCoveragePercent *= (1f - deltaTime * 0.02f);
                        ocean.BleachingSeverity = math.min(1f, ocean.BleachingSeverity + deltaTime * 0.01f);
                    }

                    // 3. Warming and Stratification
                    // Warmer surface = more stratified = less nutrient mixing
                    float warmingRate = 0.02f; // °C per decade mock
                    ocean.AvgSurfaceTemperature += warmingRate * deltaTime;
                    
                    ocean.StratificationIndex = math.min(1f, 
                        ocean.StratificationIndex + (warmingRate * deltaTime * 0.1f));
                    
                    // Reduced mixing affects phytoplankton
                    float mixingReduction = ocean.StratificationIndex * 0.3f;
                    ocean.PhytoplanktonBiomass *= (1f - (mixingReduction * deltaTime * 0.05f));

                    // 4. Deoxygenation
                    // Warmer water holds less oxygen; stratification prevents replenishment
                    float oxygenSolubilityLoss = ocean.AvgSurfaceTemperature * 0.01f;
                    float ventilationReduction = ocean.StratificationIndex * 0.2f;
                    
                    ocean.DissolvedOxygenLevel = math.max(0f, 
                        ocean.DissolvedOxygenLevel - ((oxygenSolubilityLoss + ventilationReduction) * deltaTime * 0.1f));
                    
                    // Dead zones expand when oxygen drops below threshold (~2 mg/L or ~60 mmol/m³)
                    if (ocean.DissolvedOxygenLevel < 0.06f)
                    {
                        ocean.DeadZoneAreaKm2 *= (1f + deltaTime * 0.05f);
                    }
                    else
                    {
                        ocean.DeadZoneAreaKm2 = math.max(0f, ocean.DeadZoneAreaKm2 - deltaTime * 10f);
                    }

                    // 5. Sea Level Rise
                    // Thermal expansion + ice melt
                    float thermalExpansion = ocean.AvgSurfaceTemperature * 0.001f;
                    float totalMelt = ocean.IceSheetMeltContribution + ocean.GlacierMeltContribution;
                    
                    ocean.ThermalExpansionContribution = thermalExpansion;
                    ocean.GlobalMeanSeaLevel += (thermalExpansion + totalMelt) * deltaTime * 0.01f;

                    // 6. Fish Stock Response
                    // Warming shifts ranges, acidification affects recruitment
                    float habitatLoss = (1f - ocean.CoralCoveragePercent) * 0.2f;
                    float productivityChange = (ocean.PhytoplanktonBiomass - 1f) * 0.3f;
                    
                    ocean.FishStockHealth = math.max(0f, math.min(1f, 
                        ocean.FishStockHealth + (productivityChange - habitatLoss) * deltaTime * 0.02f));

                    _random = random;
                }).WithoutBurst().Run();

            // Update Ocean Currents
            Entities
                .WithAll<OceanCurrentElement>()
                .ForEach((ref OceanCurrentElement current) =>
                {
                    // Boundary currents intensify with climate change (western boundaries)
                    if (current.CurrentType == 1)
                    {
                        current.VelocityMagnitude *= (1f + deltaTime * 0.001f);
                        current.EddyKineticEnergy *= (1f + deltaTime * 0.002f);
                    }
                    
                    // Deep currents slow with reduced formation
                    if (current.CurrentType == 3)
                    {
                        current.TransportVolume *= (1f - deltaTime * 0.0005f);
                        current.IsWeakening = true;
                    }

                    // Temperature and salinity advection (simplified)
                    current.Temperature += random.NextFloat(-0.001f, 0.001f) * deltaTime;
                    current.Salinity += random.NextFloat(-0.0005f, 0.0005f) * deltaTime;
                }).WithoutBurst().Run();

            // Update Marine Ecosystems
            Entities
                .WithAll<MarineEcosystemElement>()
                .ForEach((ref MarineEcosystemElement eco) =>
                {
                    // Coral Reefs: highly sensitive to warming and acidification
                    if (eco.EcosystemType == 0)
                    {
                        float bleachingThreshold = 1.5f; // °C above normal
                        float tempStress = math.max(0f, (eco.WarmingStress - bleachingThreshold) * 0.2f);
                        
                        eco.BiodiversityIndex *= (1f - ((tempStress + eco.AcidificationStress) * deltaTime * 0.05f));
                        
                        if (eco.AcidificationStress > 0.5f)
                        {
                            eco.PrimaryProductivity *= (1f - deltaTime * 0.03f);
                        }
                    }
                    // Kelp Forests: benefit from cooling, harmed by warming
                    else if (eco.EcosystemType == 1)
                    {
                        if (eco.WarmingStress > 0.3f)
                        {
                            eco.BiodiversityIndex *= (1f - deltaTime * 0.02f);
                            // Range contraction toward poles
                        }
                        else
                        {
                            eco.PrimaryProductivity *= (1f + deltaTime * 0.01f);
                        }
                    }
                    // Mangroves: threatened by sea level rise but can migrate
                    else if (eco.EcosystemType == 2)
                    {
                        float slrThreat = 0.5f; // Mock sea level rise rate
                        float migrationCapacity = 0.3f; // Can migrate inland if space exists
                        float netLoss = slrThreat - migrationCapacity;
                        
                        if (netLoss > 0)
                        {
                            eco.HumanImpactScore = math.min(1f, eco.HumanImpactScore + deltaTime * 0.01f);
                        }
                    }
                    // Open Ocean: affected by stratification and deoxygenation
                    else if (eco.EcosystemType == 3)
                    {
                        if (eco.IsHypoxic)
                        {
                            eco.BiodiversityIndex = math.max(0f, eco.BiodiversityIndex - deltaTime * 0.05f);
                            eco.TrophicLevelAvg = math.max(1f, eco.TrophicLevelAvg - deltaTime * 0.01f);
                        }
                    }

                    // Universal human impact accumulation
                    eco.HumanImpactScore = math.min(1f, 
                        eco.HumanImpactScore + deltaTime * 0.001f);
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to simulate marine protected areas and conservation interventions
    /// </summary>
    public class MarineConservationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<MarineEcosystemElement>()
                .ForEach((ref MarineEcosystemElement eco) =>
                {
                    // MPA designation reduces human impact
                    bool IsInMPA = false; // Would come from governance system
                    if (IsInMPA)
                    {
                        eco.HumanImpactScore = math.max(0f, 
                            eco.HumanImpactScore - Time.DeltaTime * 0.02f);
                        eco.BiodiversityIndex = math.min(2f, 
                            eco.BiodiversityIndex + Time.DeltaTime * 0.005f);
                    }

                    // Restoration efforts (coral gardening, kelp replanting)
                    bool IsBeingRestored = false;
                    if (IsBeingRestored && eco.EcosystemType == 0)
                    {
                        eco.CoralCoveragePercent = math.min(1f, 
                            eco.CoralCoveragePercent + Time.DeltaTime * 0.01f);
                    }

                    // Fishing restrictions help stock recovery
                    bool HasFishingBan = false;
                    if (HasFishingBan)
                    {
                        eco.TrophicLevelAvg = math.min(4f, 
                            eco.TrophicLevelAvg + Time.DeltaTime * 0.01f);
                    }
                }).WithoutBurst().Run();
        }
    }
}
