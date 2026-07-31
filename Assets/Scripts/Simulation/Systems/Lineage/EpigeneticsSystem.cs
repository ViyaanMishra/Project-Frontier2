using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Lineage
{
    /// <summary>
    /// Epigenetics System tracking trauma inheritance, gene expression modulation,
    /// environmental effects on DNA methylation, and transgenerational phenotypic changes.
    /// </summary>
    [Serializable]
    public struct EpigeneticsComponent : IComponentData
    {
        // Methylation Patterns
        public float GlobalMethylationLevel; // 0-1, genome-wide average
        public float StressResponseMethylation; // Specific to HPA axis genes
        public float MetabolicMethylation; // Insulin, obesity-related genes
        
        // Histone Modifications
        public float HistoneAcetylationLevel; // Gene activation marker
        public float HistoneMethylationLevel; // Can activate or repress
        
        // Environmental Exposures
        public float ToxinExposureCumulative;
        public float NutritionalDeficiencyScore; // 0-1, higher = worse
        public float ChronicStressIndex; // 0-1
        public float TraumaExposureCount;
        
        // Transgenerational Effects
        public int GenerationsSinceTrauma;
        public float InheritedEpimutations; // % of epigenetic marks passed on
        public float ReversalRate; // Natural erasure per generation
        
        // Phenotypic Expression
        public float AnxietyPredisposition; // 0-1
        public float DepressionPredisposition; // 0-1
        public float ObesityPredisposition; // 0-1
        public float ImmuneFunctionScore; // 0-1
        public float CognitiveResilience; // 0-1
    }

    [Serializable]
    public struct EpigeneticMarkerElement : IBufferElementData
    {
        public int GeneRegion; // 0=Promoter, 1=Enhancer, 2=Silencer, 3=CpG Island
        public string GeneName; // Hash or ID
        public float MethylationBeta; // 0-1 continuous value
        public float ExpressionLevel; // Resulting gene expression
        bool IsReversible;
        public int AgeOfOnset; // When mark appeared
        public int ParentOrigin; // 0=Mother, 1=Father, 2=Both
        public float EnvironmentalTriggerStrength;
    }

    [Serializable]
    public struct TransgenerationalMemoryElement : IBufferElementData
    {
        public int GenerationDepth; // How many generations back
        public int TraumaEventType; // 0=Famine, 1=War, 2=Abuse, 3=Toxin, 4=Neglect
        public float SeverityScore; // 0-1
        public float PersistenceFactor; // How strongly it persists
        public bool IsMaternallyInherited;
        public bool IsPaternallyInherited;
        public float PhenotypicEffectMagnitude;
    }

    public class EpigeneticsSystem : SystemBase
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

            // Update Macro Epigenetic State
            Entities
                .WithAll<EpigeneticsComponent>()
                .ForEach((ref EpigeneticsComponent epi) =>
                {
                    // 1. Stress Response Methylation (FKBP5, NR3C1 genes)
                    // Chronic stress increases methylation, blunting stress response
                    if (epi.ChronicStressIndex > 0.5f)
                    {
                        epi.StressResponseMethylation = math.min(1f, 
                            epi.StressResponseMethylation + (epi.ChronicStressIndex * deltaTime * 0.01f));
                        epi.AnxietyPredisposition = math.min(1f, 
                            epi.AnxietyPredisposition + (epi.StressResponseMethylation * deltaTime * 0.005f));
                    }
                    else
                    {
                        // Recovery in low-stress environment
                        epi.StressResponseMethylation = math.max(0f, 
                            epi.StressResponseMethylation - (deltaTime * 0.002f));
                    }

                    // 2. Nutritional Effects (Dutch Hunger Winter model)
                    // Poor nutrition affects metabolic gene methylation
                    if (epi.NutritionalDeficiencyScore > 0.3f)
                    {
                        epi.MetabolicMethylation = math.min(1f, 
                            epi.MetabolicMethylation + (epi.NutritionalDeficiencyScore * deltaTime * 0.008f));
                        epi.ObesityPredisposition = math.min(1f, 
                            epi.ObesityPredisposition + (epi.MetabolicMethylation * deltaTime * 0.003f));
                    }
                    else
                    {
                        epi.MetabolicMethylation = math.max(0f, 
                            epi.MetabolicMethylation - (deltaTime * 0.001f));
                    }

                    // 3. Toxin Exposure (BPA, heavy metals, etc.)
                    // Causes global hypomethylation initially, then hypermethylation
                    if (epi.ToxinExposureCumulative > 10f)
                    {
                        epi.GlobalMethylationLevel = math.max(0f, 
                            epi.GlobalMethylationLevel - (deltaTime * 0.005f)); // Hypomethylation
                        epi.ImmuneFunctionScore = math.max(0f, 
                            epi.ImmuneFunctionScore - (deltaTime * 0.002f));
                    }

                    // 4. Histone Acetylation (HDAC activity)
                    // Stress reduces acetylation (gene silencing)
                    epi.HistoneAcetylationLevel = math.lerp(epi.HistoneAcetylationLevel, 
                        1f - epi.ChronicStressIndex * 0.5f, deltaTime * 0.02f);
                    
                    // Low acetylation = reduced cognitive resilience
                    epi.CognitiveResilience = math.max(0f, 
                        epi.HistoneAcetylationLevel * 0.8f);

                    // 5. Transgenerational Inheritance
                    // Marks persist for ~3 generations typically
                    if (epi.GenerationsSinceTrauma > 0)
                    {
                        // Erasure rate per generation
                        float erasure = epi.ReversalRate * deltaTime;
                        epi.InheritedEpimutations = math.max(0f, 
                            epi.InheritedEpimutations - erasure);
                        
                        // Phenotypic effects fade with erasure
                        float phenotypicFade = epi.InheritedEpimutations / 0.3f; // Normalize
                        epi.DepressionPredisposition *= (1f - (erasure * phenotypicFade));
                        epi.AnxietyPredisposition *= (1f - (erasure * phenotypicFade));
                        
                        epi.GenerationsSinceTrauma += (int)(deltaTime * 0.01f); // Approximate gen time
                    }

                    // 6. Trauma Accumulation
                    // Multiple traumas have compounding effects
                    if (epi.TraumaExposureCount > 2)
                    {
                        float compoundingFactor = 1f + ((epi.TraumaExposureCount - 2) * 0.2f);
                        epi.StressResponseMethylation = math.min(1f, 
                            epi.StressResponseMethylation * compoundingFactor);
                    }

                    _random = random;
                }).WithoutBurst().Run();

            // Update Individual Epigenetic Markers
            Entities
                .WithAll<EpigeneticMarkerElement>()
                .ForEach((ref EpigeneticMarkerElement marker) =>
                {
                    // Expression level determined by methylation at promoter
                    if (marker.GeneRegion == 0) // Promoter
                    {
                        // High methylation at promoter = silenced gene
                        marker.ExpressionLevel = 1f - marker.MethylationBeta;
                    }
                    else if (marker.GeneRegion == 1) // Enhancer
                    {
                        // Methylation at enhancer reduces enhancement
                        marker.ExpressionLevel = 1f - (marker.MethylationBeta * 0.7f);
                    }
                    else if (marker.GeneRegion == 3) // CpG Island
                    {
                        // Dense methylation = stable silencing
                        if (marker.MethylationBeta > 0.7f)
                        {
                            marker.ExpressionLevel = 0f;
                            marker.IsReversible = false; // Locked silencing
                        }
                    }

                    // Environmental triggers can modify methylation
                    if (marker.EnvironmentalTriggerStrength > 0.5f)
                    {
                        float changeRate = marker.EnvironmentalTriggerStrength * deltaTime * 0.01f;
                        // Direction depends on gene type (stress genes up, others vary)
                        marker.MethylationBeta = math.clamp(marker.MethylationBeta + changeRate, 0f, 1f);
                    }

                    // Age-related drift
                    if (marker.AgeOfOnset > 0)
                    {
                        int currentAge = 50; // Mock
                        int yearsSinceOnset = currentAge - marker.AgeOfOnset;
                        float drift = yearsSinceOnset * 0.001f;
                        marker.MethylationBeta = math.clamp(marker.MethylationBeta + drift, 0f, 1f);
                    }
                }).WithoutBurst().Run();

            // Process Transgenerational Memory
            Entities
                .WithAll<TransgenerationalMemoryElement>()
                .ForEach((ref TransgenerationalMemoryElement memory) =>
                {
                    // Persistence decreases with each generation
                    float persistenceDecay = 0.3f; // 30% loss per generation
                    
                    if (memory.GenerationDepth > 0)
                    {
                        memory.PersistenceFactor *= (1f - persistenceDecay);
                        memory.PhenotypicEffectMagnitude = memory.SeverityScore * memory.PersistenceFactor;
                        
                        // Maternal vs Paternal differences
                        // Imprinted genes show parent-of-origin effects
                        if (memory.IsMaternallyInherited && !memory.IsPaternallyInherited)
                        {
                            // Some marks only persist through maternal line
                            memory.PersistenceFactor *= 1.2f;
                        }
                    }

                    // Severe trauma (famine, genocide) has longer persistence
                    if (memory.TraumaEventType == 0 || memory.TraumaEventType == 1)
                    {
                        memory.PersistenceFactor = math.min(1f, memory.PersistenceFactor + 0.1f);
                    }
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to simulate epigenetic therapy and reversal interventions
    /// </summary>
    public class EpigeneticTherapySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<EpigeneticsComponent>()
                .ForEach((ref EpigeneticsComponent epi) =>
                {
                    // HDAC Inhibitors increase acetylation
                    bool receivingHDACi = false; // Would come from medical system
                    if (receivingHDACi)
                    {
                        epi.HistoneAcetylationLevel = math.min(1f, 
                            epi.HistoneAcetylationLevel + Time.DeltaTime * 0.05f);
                        epi.CognitiveResilience = math.min(1f, 
                            epi.CognitiveResilience + Time.DeltaTime * 0.02f);
                    }

                    // DNMT Inhibitors reduce methylation
                    bool receivingDNMTi = false;
                    if (receivingDNMTi)
                    {
                        epi.GlobalMethylationLevel = math.max(0f, 
                            epi.GlobalMethylationLevel - Time.DeltaTime * 0.03f);
                        epi.StressResponseMethylation = math.max(0f, 
                            epi.StressResponseMethylation - Time.DeltaTime * 0.02f);
                    }

                    // Behavioral interventions (therapy, meditation, exercise)
                    bool receivingTherapy = true; // Mock
                    if (receivingTherapy)
                    {
                        epi.ChronicStressIndex = math.max(0f, 
                            epi.ChronicStressIndex - Time.DeltaTime * 0.01f);
                        // Indirectly improves all stress-related markers
                    }

                    // Environmental enrichment reverses some marks
                    float enrichmentLevel = 0.5f; // Mock
                    epi.ReversalRate = 0.1f + (enrichmentLevel * 0.1f);
                }).WithoutBurst().Run();
        }
    }
}
