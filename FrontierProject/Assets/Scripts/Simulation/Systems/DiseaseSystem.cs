using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Frontier.Core;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Vector-based pathogen simulation with mutation, transmission vectors, and quarantine mechanics.
    /// Tracks disease spread through populations with realistic incubation, infection, and recovery cycles.
    /// </summary>
    public struct Pathogen
    {
        public Guid ID;
        public FixedString64Bytes Name;
        public float IncubationPeriod;      // Hours before symptoms appear
        public float InfectionDuration;     // Total illness duration
        public float TransmissionRate;      // Base R0 value
        public float MortalityRate;         // Death probability
        public float MutationRate;          // Chance per day to mutate
        public bool Airborne;
        public bool Waterborne;
        public bool ContactBased;
        public bool VectorBased;            // Insect/animal carriers
        public int ResistanceLevel;         // 0-10 resistance to treatments
        public float SymptomSeverity;       // 0-1 severity multiplier
        
        public PathogenSymptom Symptoms;
    }
    
    [Flags]
    public enum PathogenSymptom
    {
        None = 0,
        Fever = 1 << 0,
        Cough = 1 << 1,
        Fatigue = 1 << 2,
        Nausea = 1 << 3,
        Rash = 1 << 4,
        Bleeding = 1 << 5,
        Neurological = 1 << 6,
        Respiratory = 1 << 7,
        OrganFailure = 1 << 8
    }
    
    public struct InfectionState
    {
        public Guid PathogenID;
        public float TimeSinceInfection;    // Hours
        public float SymptomOnsetTime;
        public HealthStage Stage;           // Incubating, Symptomatic, Critical, Recovering, Immune
        public float Severity;              // Current symptom severity 0-1
        public bool IsQuarantined;
        public bool IsReceivingTreatment;
        public int TreatmentEffectiveness;
        public float ImmunityLevel;         // Post-recovery immunity 0-1
        public NativeList<Guid> ExposureHistory;
    }
    
    public enum HealthStage
    {
        Healthy,
        Exposed,
        Incubating,
        Symptomatic,
        Critical,
        Recovering,
        Immune,
        Deceased
    }
    
    public class DiseaseSystem : IDisposable
    {
        private NativeHashMap<Guid, Pathogen> _pathogens;
        private NativeHashMap<EntityGUID, NativeList<InfectionState>> _infections;
        private NativeList<DiseaseOutbreak> _activeOutbreaks;
        private readonly EventBus _eventBus;
        private readonly float _tickHours;
        
        public DiseaseSystem(EventBus eventBus, float tickHours = 0.25f)
        {
            _eventBus = eventBus;
            _tickHours = tickHours;
            _pathogens = new NativeHashMap<Guid, Pathogen>(64, Allocator.Persistent);
            _infections = new NativeHashMap<EntityGUID, NativeList<InfectionState>>(1024, Allocator.Persistent);
            _activeOutbreaks = new NativeList<DiseaseOutbreak>(16, Allocator.Persistent);
            
            RegisterDefaultPathogens();
        }
        
        private void RegisterDefaultPathogens()
        {
            // Common Cold
            RegisterPathogen(new Pathogen
            {
                ID = Guid.NewGuid(),
                Name = "Common Cold",
                IncubationPeriod = 24f,
                InfectionDuration = 168f,
                TransmissionRate = 2.5f,
                MortalityRate = 0.001f,
                MutationRate = 0.1f,
                Airborne = true,
                ContactBased = true,
                SymptomSeverity = 0.3f,
                Symptoms = PathogenSymptom.Fever | PathogenSymptom.Cough | PathogenSymptom.Fatigue
            });
            
            // Influenza
            RegisterPathogen(new Pathogen
            {
                ID = Guid.NewGuid(),
                Name = "Influenza",
                IncubationPeriod = 48f,
                InfectionDuration = 336f,
                TransmissionRate = 3.5f,
                MortalityRate = 0.01f,
                MutationRate = 0.15f,
                Airborne = true,
                ContactBased = true,
                SymptomSeverity = 0.6f,
                Symptoms = PathogenSymptom.Fever | PathogenSymptom.Cough | PathogenSymptom.Fatigue | PathogenSymptom.Respiratory
            });
            
            // Dysentery
            RegisterPathogen(new Pathogen
            {
                ID = Guid.NewGuid(),
                Name = "Dysentery",
                IncubationPeriod = 72f,
                InfectionDuration = 504f,
                TransmissionRate = 2.0f,
                MortalityRate = 0.05f,
                MutationRate = 0.08f,
                Waterborne = true,
                ContactBased = true,
                SymptomSeverity = 0.7f,
                Symptoms = PathogenSymptom.Nausea | PathogenSymptom.Fever | PathogenSymptom.Fatigue
            });
            
            // Anomaly Plague (fictional)
            RegisterPathogen(new Pathogen
            {
                ID = Guid.NewGuid(),
                Name = "Anomaly Plague",
                IncubationPeriod = 120f,
                InfectionDuration = 720f,
                TransmissionRate = 1.8f,
                MortalityRate = 0.35f,
                MutationRate = 0.4f,
                Airborne = true,
                Waterborne = true,
                ContactBased = true,
                VectorBased = true,
                ResistanceLevel = 8,
                SymptomSeverity = 0.95f,
                Symptoms = PathogenSymptom.Fever | PathogenSymptom.Bleeding | PathogenSymptom.Neurological | PathogenSymptom.OrganFailure
            });
        }
        
        public void RegisterPathogen(Pathogen pathogen)
        {
            if (!_pathogens.ContainsKey(pathogen.ID))
            {
                _pathogens.Add(pathogen.ID, pathogen);
                _eventBus.Publish(new PathogenRegisteredEvent { PathogenID = pathogen.ID });
            }
        }
        
        public void ExposeEntity(EntityGUID entity, Guid pathogenID, float exposureIntensity = 1.0f)
        {
            if (!_pathogens.TryGetValue(pathogenID, out var pathogen))
                return;
                
            float infectionChance = pathogen.TransmissionRate * exposureIntensity * 0.01f;
            
            if (UnityEngine.Random.value > infectionChance)
                return;
                
            if (!_infections.ContainsKey(entity))
            {
                _infections.Add(entity, new NativeList<InfectionState>(Allocator.Persistent));
            }
            
            var infections = _infections[entity];
            infections.Add(new InfectionState
            {
                PathogenID = pathogenID,
                TimeSinceInfection = 0f,
                SymptomOnsetTime = pathogen.IncubationPeriod,
                Stage = HealthStage.Exposed,
                Severity = 0f,
                IsQuarantined = false,
                IsReceivingTreatment = false,
                ExposureHistory = new NativeList<Guid>(Allocator.Temp)
            });
            
            _eventBus.Publish(new EntityExposedEvent 
            { 
                EntityID = entity, 
                PathogenID = pathogenID,
                ExposureIntensity = exposureIntensity
            });
        }
        
        [BurstCompile]
        public struct DiseaseTickJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<EntityGUID> Entities;
            [NativeDisableContainerSafetyRestriction] public NativeHashMap<EntityGUID, NativeList<InfectionState>> Infections;
            [ReadOnly] public NativeHashMap<Guid, Pathogen> Pathogens;
            public float DeltaHours;
            public Random Random;
            
            public void Execute(int index)
            {
                var entity = Entities[index];
                if (!Infections.ContainsKey(entity))
                    return;
                    
                var infections = Infections[entity];
                for (int i = infections.Length - 1; i >= 0; i--)
                {
                    var infection = infections[i];
                    infection.TimeSinceInfection += DeltaHours;
                    
                    if (!Pathogens.TryGetValue(infection.PathogenID, out var pathogen))
                        continue;
                    
                    // Progress through stages
                    if (infection.Stage == HealthStage.Exposed && infection.TimeSinceInfection >= pathogen.IncubationPeriod)
                    {
                        infection.Stage = HealthStage.Symptomatic;
                        infection.Severity = pathogen.SymptomSeverity;
                    }
                    else if (infection.Stage == HealthStage.Symptomatic)
                    {
                        // Check for critical condition
                        if (infection.Severity > 0.8f && Random.NextFloat(0, 1) < pathogen.MortalityRate * DeltaHours / 24f)
                        {
                            infection.Stage = HealthStage.Critical;
                        }
                        
                        // Recovery check
                        if (infection.TimeSinceInfection >= pathogen.InfectionDuration)
                        {
                            infection.Stage = HealthStage.Recovering;
                            infection.ImmunityLevel = 0.8f; // 80% immunity post-recovery
                        }
                    }
                    else if (infection.Stage == HealthStage.Recovering)
                    {
                        infection.Severity = Mathf.Max(0, infection.Severity - DeltaHours / 48f);
                        if (infection.Severity <= 0.01f)
                        {
                            infection.Stage = HealthStage.Immune;
                        }
                    }
                    
                    // Treatment effectiveness
                    if (infection.IsReceivingTreatment)
                    {
                        infection.Severity *= (1f - infection.TreatmentEffectiveness * 0.01f);
                    }
                    
                    // Mutation chance
                    if (pathogen.MutationRate > 0 && Random.NextFloat(0, 1) < pathogen.MutationRate * DeltaHours / 24f)
                    {
                        // Mutate pathogen properties
                        pathogen.TransmissionRate *= Random.NextFloat(0.9f, 1.1f);
                        pathogen.MortalityRate *= Random.NextFloat(0.9f, 1.1f);
                        pathogen.ResistanceLevel = Mathf.Clamp(pathogen.ResistanceLevel + Random.Next(-1, 2), 0, 10);
                    }
                    
                    infections[i] = infection;
                }
            }
        }
        
        public void Tick(float deltaTime)
        {
            var entities = new NativeArray<EntityGUID>(_infections.GetKeyArray(Allocator.Temp), Allocator.Temp);
            var job = new DiseaseTickJob
            {
                Entities = entities,
                Infections = _infections,
                Pathogens = _pathogens,
                DeltaHours = _tickHours,
                Random = new Random((uint)UnityEngine.Time.frameCount)
            };
            
            job.Schedule(entities.Length, 64).Complete();
            entities.Dispose();
            
            // Check for outbreaks
            DetectOutbreaks();
        }
        
        private void DetectOutbreaks()
        {
            // Group infections by pathogen and location
            var outbreakCounts = new NativeHashMap<Guid, int>(Allocator.Temp);
            
            foreach (var kvp in _infections)
            {
                foreach (var infection in kvp.Value)
                {
                    if (infection.Stage == HealthStage.Symptomatic || infection.Stage == HealthStage.Critical)
                    {
                        if (outbreakCounts.ContainsKey(infection.PathogenID))
                        {
                            outbreakCounts[infection.PathogenID]++;
                        }
                        else
                        {
                            outbreakCounts.Add(infection.PathogenID, 1);
                        }
                    }
                }
            }
            
            // Declare outbreaks when threshold exceeded
            foreach (var kvp in outbreakCounts)
            {
                if (kvp.Value >= 5 && !_activeOutbreaks.Any(o => o.PathogenID == kvp.Key))
                {
                    _activeOutbreaks.Add(new DiseaseOutbreak
                    {
                        PathogenID = kvp.Key,
                        InfectedCount = kvp.Value,
                        DeclaredTime = UnityEngine.Time.time,
                        Severity = OutbreakSeverity.Local
                    });
                    
                    _eventBus.Publish(new OutbreakDeclaredEvent 
                    { 
                        PathogenID = kvp.Key,
                        InfectedCount = kvp.Value
                    });
                }
            }
            
            outbreakCounts.Dispose();
        }
        
        public void QuarantineEntity(EntityGUID entity)
        {
            if (_infections.ContainsKey(entity))
            {
                var infections = _infections[entity];
                for (int i = 0; i < infections.Length; i++)
                {
                    var infection = infections[i];
                    infection.IsQuarantined = true;
                    infections[i] = infection;
                }
                
                _eventBus.Publish(new EntityQuarantinedEvent { EntityID = entity });
            }
        }
        
        public void TreatEntity(EntityGUID entity, int treatmentQuality)
        {
            if (_infections.ContainsKey(entity))
            {
                var infections = _infections[entity];
                for (int i = 0; i < infections.Length; i++)
                {
                    var infection = infections[i];
                    infection.IsReceivingTreatment = true;
                    infection.TreatmentEffectiveness = treatmentQuality;
                    infections[i] = infection;
                }
                
                _eventBus.Publish(new EntityTreatedEvent { EntityID = entity, TreatmentQuality = treatmentQuality });
            }
        }
        
        public bool IsEntityInfected(EntityGUID entity)
        {
            return _infections.ContainsKey(entity) && _infections[entity].Length > 0;
        }
        
        public HealthStage GetEntityHealthStage(EntityGUID entity)
        {
            if (!_infections.ContainsKey(entity) || _infections[entity].Length == 0)
                return HealthStage.Healthy;
                
            var worstStage = HealthStage.Healthy;
            foreach (var infection in _infections[entity])
            {
                if ((int)infection.Stage > (int)worstStage)
                    worstStage = infection.Stage;
            }
            return worstStage;
        }
        
        public void Dispose()
        {
            foreach (var kvp in _infections)
            {
                kvp.Value.Dispose();
            }
            _pathogens.Dispose();
            _infections.Dispose();
            _activeOutbreaks.Dispose();
        }
    }
    
    public struct DiseaseOutbreak
    {
        public Guid PathogenID;
        public int InfectedCount;
        public float DeclaredTime;
        public OutbreakSeverity Severity;
    }
    
    public enum OutbreakSeverity
    {
        Local,
        Regional,
        Epidemic,
        Pandemic
    }
    
    // Events
    public struct PathogenRegisteredEvent
    {
        public Guid PathogenID;
    }
    
    public struct EntityExposedEvent
    {
        public EntityGUID EntityID;
        public Guid PathogenID;
        public float ExposureIntensity;
    }
    
    public struct EntityQuarantinedEvent
    {
        public EntityGUID EntityID;
    }
    
    public struct EntityTreatedEvent
    {
        public EntityGUID EntityID;
        public int TreatmentQuality;
    }
    
    public struct OutbreakDeclaredEvent
    {
        public Guid PathogenID;
        public int InfectedCount;
    }
}
