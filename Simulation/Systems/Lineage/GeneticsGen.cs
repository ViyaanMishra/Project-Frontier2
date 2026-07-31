using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Lineage
{
    /// <summary>
    /// Advanced genetics system with Mendelian inheritance, polygenic traits,
    /// dynamic pedigree construction, and mutation modeling.
    /// </summary>
    [Serializable]
    public struct Individual : IComponentData
    {
        public int IndividualID;
        public int FamilyID;
        public int Generation;
        
        // Parentage
        public int FatherID;
        public int MotherID;
        public bool IsAdopted;
        
        // Demographics
        public Gender Gender;
        public int Age;
        public float BirthDate;
        public float? DeathDate;
        public VitalStatus Status;
        
        // Genetic makeup
        public Genotype Genotype;
        public Phenotype Phenotype;
        
        // Life outcomes
        public float HealthIndex; // 0-1
        public float Intelligence; // 0-1
        public float Personality_Openness;
        public float Personality_Conscientiousness;
        public float Personality_Extraversion;
        public float Personality_Agreeableness;
        public float Personality_Neuroticism;
        
        // Social
        public int SocialClass;
        public float Wealth;
        public int EducationLevel;
        public int OccupationID;
        
        // Reproductive
        public int OffspringCount;
        public NativeArray<int> OffspringIDs;
        public float Fertility; // 0-1
        public bool IsSterile;
    }
    
    public enum Gender
    {
        Male,
        Female
    }
    
    public enum VitalStatus
    {
        Alive,
        Deceased,
        Missing,
        Unknown
    }
    
    [Serializable]
    public struct Genotype
    {
        // Chromosome pairs (simplified representation)
        public NativeArray<GenePair> AutosomalGenes; // 22 pairs
        public GenePair SexChromosomes; // XX or XY
        
        // Mitochondrial DNA (maternal only)
        public ulong MitochondrialHaplogroup;
        
        // Genetic markers
        public uint[] SNPs; // Single nucleotide polymorphisms
        
        // Mutation tracking
        public NativeArray<Mutation> DeNovoMutations;
        public float MutationRate;
        
        // Epigenetic markers
        public NativeArray<float> MethylationPatterns;
        public float EpigeneticAge; // vs chronological age
    }
    
    [Serializable]
    public struct GenePair
    {
        public Allele MaternalAllele;
        public Allele PaternalAllele;
        public GeneType Type;
        public bool IsImprinted; // Parent-of-origin effects
        public float ExpressionLevel; // 0-1
    }
    
    [Serializable]
    public struct Allele
    {
        public uint GeneID;
        public byte VariantCode; // Specific allele variant
        public float EffectSize; // Magnitude of effect
        public bool IsDominant;
        public bool IsRecessive;
        public bool IsCodominant;
    }
    
    public enum GeneType
    {
        Physical,      // Appearance traits
        Physiological, // Body functions
        Cognitive,     // Mental abilities
        Behavioral,    // Personality tendencies
        DiseaseRisk,   // Health predispositions
        Metabolic      // Processing efficiencies
    }
    
    [Serializable]
    public struct Phenotype
    {
        // Physical traits
        public float Height; // cm
        public float Weight; // kg
        public float BMI;
        public float3 SkinColor; // RGB
        public float EyeColorHue; // 0-1
        public HairType HairType;
        public float MelaninLevel; // 0-1
        
        // Physiological traits
        public float MetabolicRate;
        public float ImmuneStrength;
        public float Endurance;
        public float Strength;
        public float Agility;
        
        // Cognitive traits
        public float IQ_Estimate;
        public float MemoryCapacity;
        public float ProcessingSpeed;
        public float Creativity;
        
        // Behavioral tendencies
        public float RiskTaking;
        public float Aggression;
        public float Sociability;
        public float ImpulseControl;
        
        // Health indicators
        public float OverallHealth;
        public NativeArray<float> DiseaseRisks; // Per disease type
        public float LongevityPotential;
    }
    
    public enum HairType
    {
        Straight,
        Wavy,
        Curly,
        Coily
    }
    
    [Serializable]
    public struct Mutation : IComponentData
    {
        public int MutationID;
        public MutationType Type;
        public int GeneID;
        public int IndividualID;
        public bool IsDeNovo; // New mutation, not inherited
        public bool IsHeritable;
        public float EffectMagnitude;
        public bool IsBeneficial;
        public bool IsDeleterious;
        public bool IsNeutral;
        public string Description;
        public int OriginGeneration;
    }
    
    public enum MutationType
    {
        PointMutation,      // Single base change
        Insertion,          // Added bases
        Deletion,           // Removed bases
        Duplication,        // Copied segment
        Inversion,          // Reversed segment
        Translocation,      // Moved to different chromosome
        Frameshift,         // Reading frame altered
        CopyNumberVariant   // Gene copy number changed
    }
    
    [Serializable]
    public struct Pedigree : IComponentData
    {
        public int FamilyID;
        public int FounderIndividualID;
        public int CurrentGeneration;
        public int TotalMembers;
        public int LivingMembers;
        
        // Structure
        public NativeArray<int> AllMemberIDs;
        public NativeArray<int> CurrentGenerationIDs;
        public NativeArray<RelationshipLink> Relationships;
        
        // Genetics tracking
        public float InbreedingCoefficient; // F statistic
        public float GeneticDiversity; // Heterozygosity
        public NativeArray<float> AlleleFrequencies;
        
        // Dynasty metrics
        public float Prestige; // Social standing over time
        public float WealthAccumulated;
        public float PowerIndex;
        public int GenerationsOfProminence;
        
        // Hereditary conditions
        public NativeArray<int> CarriedDiseaseAlleles;
        public float GeneticLoad; // Deleterious mutations
    }
    
    [Serializable]
    public struct RelationshipLink
    {
        public int IndividualA_ID;
        public int IndividualB_ID;
        public RelationshipType Type;
        public float Relatedness; // Coefficient of relationship
        public int CommonAncestorCount;
    }
    
    public enum RelationshipType
    {
        ParentChild,
        Sibling,
        HalfSibling,
        GrandparentGrandchild,
        UncleNephew,
        FirstCousin,
        SecondCousin,
        Spouse,
        Adoptive
    }
    
    [Serializable]
    public struct SuccessionLaw : IComponentData
    {
        public LawType Type;
        public int DynastyID;
        public int CurrentRulerID;
        public int HeirID;
        public NativeArray<int> SuccessionLine; // Ordered list
        
        // Rules
        public bool IsMalePreference;
        public bool IsAbsolutePrimogeniture; // Firstborn regardless of gender
        public bool IsSalic; // Male only
        public bool IsElective;
        public bool IsMeritocratic; // Based on ability
        
        // Requirements
        public float MinimumLegitimacy; // 0-1
        public int MinimumAge;
        public bool RequiresReligiousApproval;
        public bool RequiresNobleSupport;
        
        // Current situation
        public float SuccessionStability; // 0-1
        public int ClaimantCount; // Competing claims
        public float CivilWarRisk; // 0-1
    }
    
    public enum LawType
    {
        Primogeniture,      // Eldest child inherits
        MalePrimogeniture,  // Eldest male child
        Ultimogeniture,     // Youngest child
        Partible,           // Divided among all children
        Gavelkind,          // Equal division
        Elective,           // Chosen by council
        Meritocratic,       // Most capable heir
        Appointment         // Ruler names successor
    }
    
    public class GeneticsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Process inheritance at birth
            // Calculate polygenic trait expression
            // Track mutation accumulation
            // Update pedigree relationships
            // Calculate inbreeding coefficients
            // Model genetic drift
            // Process natural selection pressures
        }
    }
    
    /// <summary>
    /// Helper methods for genetic calculations.
    /// </summary>
    public static class GeneticsCalculator
    {
        public static float CalculateRelatedness(RelationshipType relationship)
        {
            return relationship switch
            {
                RelationshipType.ParentChild => 0.5f,
                RelationshipType.Sibling => 0.5f,
                RelationshipType.HalfSibling => 0.25f,
                RelationshipType.GrandparentGrandchild => 0.25f,
                RelationshipType.UncleNephew => 0.25f,
                RelationshipType.FirstCousin => 0.125f,
                RelationshipType.SecondCousin => 0.03125f,
                _ => 0f
            };
        }
        
        public static float CalculateInbreedingCoefficient(NativeArray<RelationshipLink> pedigreeLinks)
        {
            // Wright's path coefficient method
            float F = 0f;
            
            // Sum over all paths through common ancestors
            // F = Σ(0.5)^(n1+n2+1) * (1 + FA)
            // where n1, n2 are generations to common ancestor
            
            // Simplified implementation
            for (int i = 0; i < pedigreeLinks.Length; i++)
            {
                var link = pedigreeLinks[i];
                if (link.Relatedness > 0.125f) // Consanguineous
                {
                    F += math.pow(0.5f, 1.0f / link.Relatedness) * 0.5f;
                }
            }
            
            return math.min(1f, F);
        }
        
        public static Allele InheritAllele(Allele maternal, Allele paternal, float mutationRate)
        {
            // Randomly select one allele from each parent
            bool inheritMaternal = UnityEngine.Random.value < 0.5f;
            Allele inherited = inheritMaternal ? maternal : paternal;
            
            // Check for mutation
            if (UnityEngine.Random.value < mutationRate)
            {
                // Apply de novo mutation
                inherited.VariantCode++; // Simplified mutation
                inherited.EffectSize *= 1.0f + (UnityEngine.Random.value - 0.5f) * 0.2f;
            }
            
            return inherited;
        }
        
        public static float CalculatePolygenicTrait(NativeArray<Allele> alleles, NativeArray<float> effectSizes)
        {
            float traitValue = 0.5f; // Base value
            
            for (int i = 0; i < alleles.Length && i < effectSizes.Length; i++)
            {
                float contribution = alleles[i].EffectSize * effectSizes[i];
                if (alleles[i].IsDominant)
                {
                    traitValue += contribution;
                }
                else
                {
                    traitValue += contribution * 0.5f; // Recessive partial expression
                }
            }
            
            return math.clamp(traitValue, 0f, 1f);
        }
    }
}
