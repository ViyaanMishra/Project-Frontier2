using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Genetic engine for NPC DNA simulation.
    /// 64-bit DNA with crossover mutation and cloning degradation.
    /// </summary>
    public struct DNA
    {
        public ulong genes;
        
        // Gene bit allocations (64 bits total)
        // Bits 0-7: Physical traits (height, build, strength base)
        // Bits 8-15: Mental traits (intelligence, willpower, sanity base)
        // Bits 16-23: Skills affinity (learning speed bonuses)
        // Bits 24-31: Health factors (immunity, metabolism, longevity)
        // Bits 32-39: Appearance (hair, eyes, skin tone variants)
        // Bits 40-47: Personality markers (aggression, sociability, etc.)
        // Bits 48-55: Special traits (mutation resistance, anomaly sensitivity)
        // Bits 56-63: Reserved/version
        
        public static DNA CreateRandom()
        {
            return new DNA { genes = UnityEngine.Random.Value * ulong.MaxValue };
        }
        
        public static DNA CreateFromParents(DNA mother, DNA father)
        {
            ulong childGenes = 0;
            
            // Crossover: alternate between parents every 8 bits
            for (int i = 0; i < 8; i++)
            {
                ulong mask = ((ulong)0xFF) << (i * 8);
                if (UnityEngine.Random.value > 0.5f)
                    childGenes |= mother.genes & mask;
                else
                    childGenes |= father.genes & mask;
            }
            
            // Apply random mutations (1% chance per byte)
            for (int i = 0; i < 8; i++)
            {
                if (UnityEngine.Random.value < 0.01f)
                {
                    ulong mutationMask = ((ulong)0xFF) << (i * 8);
                    ulong mutation = ((ulong)(UnityEngine.Random.Value * 256)) << (i * 8);
                    childGenes = (childGenes & ~mutationMask) | (mutation & mutationMask);
                }
            }
            
            return new DNA { genes = childGenes };
        }
        
        public float GetTrait(int bitStart, int bitCount)
        {
            ulong mask = ((ulong)(1 << bitCount) - 1) << bitStart;
            ulong value = (genes & mask) >> bitStart;
            return (float)value / (float)((1 << bitCount) - 1);
        }
        
        public void SetTrait(int bitStart, int bitCount, float value)
        {
            ulong intValue = (ulong)(math.clamp(value, 0f, 1f) * ((1 << bitCount) - 1));
            ulong mask = ((ulong)(1 << bitCount) - 1) << bitStart;
            genes = (genes & ~mask) | (intValue << bitStart);
        }
        
        // Trait accessors
        public float PhysicalScore => GetTrait(0, 8);
        public float MentalScore => GetTrait(8, 8);
        public float SkillAffinity => GetTrait(16, 8);
        public float HealthFactor => GetTrait(24, 8);
        public float AppearanceVariant => GetTrait(32, 8);
        public float PersonalityMarker => GetTrait(40, 8);
        public float SpecialTrait => GetTrait(48, 8);
        
        public float MutationResistance => GetTrait(48, 4);
        public float AnomalySensitivity => GetTrait(52, 4);
        
        public override string ToString()
        {
            return $"DNA:{genes:X16}";
        }
    }
    
    public class GeneticEngine
    {
        private NativeList<DNA> _population;
        private int _generation;
        private float _cloningDegradationFactor;
        
        public int PopulationCount => _population.Length;
        public int Generation => _generation;
        
        public GeneticEngine(int initialPopulationSize = 100)
        {
            _population = new NativeList<DNA>(initialPopulationSize, Allocator.Persistent);
            _generation = 0;
            _cloningDegradationFactor = 0.05f; // 5% degradation per clone
            
            // Initialize random population
            for (int i = 0; i < initialPopulationSize; i++)
            {
                _population.Add(DNA.CreateRandom());
            }
        }
        
        public DNA Breed(int parentAIndex, int parentBIndex)
        {
            if (parentAIndex < 0 || parentAIndex >= _population.Length ||
                parentBIndex < 0 || parentBIndex >= _population.Length)
                return DNA.CreateRandom();
                
            DNA child = DNA.CreateFromParents(_population[parentAIndex], _population[parentBIndex]);
            _population.Add(child);
            return child;
        }
        
        public DNA Clone(int sourceIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _population.Length)
                return DNA.CreateRandom();
                
            DNA source = _population[sourceIndex];
            DNA clone = source;
            
            // Apply cloning degradation
            ulong degradedGenes = 0;
            for (int i = 0; i < 8; i++)
            {
                ulong mask = ((ulong)0xFF) << (i * 8);
                ulong geneByte = source.genes & mask;
                
                // Degrade by random amount up to degradation factor
                float degradation = UnityEngine.Random.value * _cloningDegradationFactor;
                float degradedValue = (geneByte >> (i * 8)) / 255.0f * (1f - degradation);
                ulong degradedByte = (ulong)(degradedValue * 255) << (i * 8);
                
                degradedGenes |= degradedByte & mask;
            }
            
            clone.genes = degradedGenes;
            _population.Add(clone);
            return clone;
        }
        
        public DNA Mutate(int sourceIndex, float mutationRate = 0.1f)
        {
            if (sourceIndex < 0 || sourceIndex >= _population.Length)
                return DNA.CreateRandom();
                
            DNA source = _population[sourceIndex];
            DNA mutated = source;
            
            // Apply targeted mutations
            ulong mutatedGenes = source.genes;
            for (int i = 0; i < 8; i++)
            {
                if (UnityEngine.Random.value < mutationRate)
                {
                    ulong mask = ((ulong)0xFF) << (i * 8);
                    ulong mutation = ((ulong)(UnityEngine.Random.Value * 256)) << (i * 8);
                    mutatedGenes = (mutatedGenes & ~mask) | (mutation & mask);
                }
            }
            
            mutated.genes = mutatedGenes;
            _population.Add(mutated);
            return mutated;
        }
        
        public void SelectFittest(int count)
        {
            // Simple selection: keep top N by physical + mental score
            // In real implementation, this would use fitness evaluation
            var sorted = new NativeList<(int index, float fitness)>(_population.Length, Allocator.Temp);
            
            for (int i = 0; i < _population.Length; i++)
            {
                var dna = _population[i];
                float fitness = dna.PhysicalScore * 0.5f + dna.MentalScore * 0.5f;
                sorted.Add((i, fitness));
            }
            
            // Sort descending by fitness (bubble sort for simplicity)
            for (int i = 0; i < sorted.Length - 1; i++)
            {
                for (int j = 0; j < sorted.Length - i - 1; j++)
                {
                    if (sorted[j].Item2 < sorted[j + 1].Item2)
                    {
                        var temp = sorted[j];
                        sorted[j] = sorted[j + 1];
                        sorted[j + 1] = temp;
                    }
                }
            }
            
            // Keep only top N
            if (count < sorted.Length)
            {
                var newPopulation = new NativeList<DNA>(count, Allocator.Persistent);
                for (int i = 0; i < count; i++)
                {
                    newPopulation.Add(_population[sorted[i].index]);
                }
                _population.Dispose();
                _population = newPopulation;
            }
            
            sorted.Dispose();
            _generation++;
        }
        
        public DNA GetDNA(int index)
        {
            if (index < 0 || index >= _population.Length) return default;
            return _population[index];
        }
        
        public void SetCloningDegradation(float factor)
        {
            _cloningDegradationFactor = math.clamp(factor, 0f, 0.5f);
        }
        
        public void Dispose()
        {
            if (_population.IsCreated) _population.Dispose();
        }
    }
}
