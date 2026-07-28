using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Frontier.Core;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Tracks player behavior patterns and adapts enemy AI difficulty accordingly.
    /// Uses machine learning-inspired heuristics to analyze combat style, preferred weapons,
    /// movement patterns, and decision-making tendencies.
    /// </summary>
    public class NeuroEvolutionTracker : IDisposable
    {
        private NativeHashMap<PlayerBehaviorMetric, float> _behaviorMetrics;
        private NativeList<EnemyAdaptation> _activeAdaptations;
        private readonly EventBus _eventBus;
        private float _sessionTime;
        private int _analysisWindow;
        
        public PlayerProfile CurrentProfile { get; private set; }
        
        public NeuroEvolutionTracker(EventBus eventBus, int analysisWindow = 300)
        {
            _eventBus = eventBus;
            _analysisWindow = analysisWindow; // frames
            _behaviorMetrics = new NativeHashMap<PlayerBehaviorMetric, float>(32, Allocator.Persistent);
            _activeAdaptations = new NativeList<EnemyAdaptation>(16, Allocator.Persistent);
            
            InitializeMetrics();
            CurrentProfile = new PlayerProfile();
        }
        
        private void InitializeMetrics()
        {
            _behaviorMetrics.Add(PlayerBehaviorMetric.AggressiveEngagement, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.DefensivePositioning, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.StealthPreference, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.LongRangeCombat, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.CloseQuartersCombat, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.ExplosiveUsage, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.VehicleDependency, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.ResourceHoarder, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.RiskTaking, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.TacticalRetreat, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.TeamCoordination, 0f);
            _behaviorMetrics.Add(PlayerBehaviorMetric.Adaptability, 0f);
        }
        
        public void RecordEvent(PlayerActionEvent actionEvent)
        {
            _sessionTime++;
            
            switch (actionEvent.ActionType)
            {
                case PlayerActionType.WeaponFire:
                    if (actionEvent.Range > 50f)
                        IncrementMetric(PlayerBehaviorMetric.LongRangeCombat, 0.1f);
                    else
                        IncrementMetric(PlayerBehaviorMetric.CloseQuartersCombat, 0.1f);
                    
                    if (actionEvent.IsExplosive)
                        IncrementMetric(PlayerBehaviorMetric.ExplosiveUsage, 0.2f);
                    break;
                    
                case PlayerActionType.TakeDamage:
                    if (actionEvent.DamageTaken > 50f)
                        IncrementMetric(PlayerBehaviorMetric.RiskTaking, 0.15f);
                    break;
                    
                case PlayerActionType.UseCover:
                    IncrementMetric(PlayerBehaviorMetric.DefensivePositioning, 0.1f);
                    break;
                    
                case PlayerActionType.StealthKill:
                    IncrementMetric(PlayerBehaviorMetric.StealthPreference, 0.3f);
                    break;
                    
                case PlayerActionType.VehicleEnter:
                    IncrementMetric(PlayerBehaviorMetric.VehicleDependency, 0.1f);
                    break;
                    
                case PlayerActionType.ResourceCollect:
                    IncrementMetric(PlayerBehaviorMetric.ResourceHoarder, 0.05f);
                    break;
                    
                case PlayerActionType.Flee:
                    IncrementMetric(PlayerBehaviorMetric.TacticalRetreat, 0.2f);
                    break;
                    
                case PlayerActionType.Charge:
                    IncrementMetric(PlayerBehaviorMetric.AggressiveEngagement, 0.2f);
                    DecrementMetric(PlayerBehaviorMetric.DefensivePositioning, 0.1f);
                    break;
            }
            
            // Decay old metrics
            if (_sessionTime % _analysisWindow == 0)
            {
                DecayMetrics();
                AnalyzeAndAdapt();
            }
        }
        
        private void IncrementMetric(PlayerBehaviorMetric metric, float amount)
        {
            if (_behaviorMetrics.TryGetValue(metric, out var value))
            {
                _behaviorMetrics[metric] = Mathf.Clamp(value + amount, 0f, 1f);
            }
        }
        
        private void DecrementMetric(PlayerBehaviorMetric metric, float amount)
        {
            if (_behaviorMetrics.TryGetValue(metric, out var value))
            {
                _behaviorMetrics[metric] = Mathf.Max(value - amount, 0f);
            }
        }
        
        private void DecayMetrics()
        {
            var keys = _behaviorMetrics.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                _behaviorMetrics[key] *= 0.95f; // 5% decay per window
            }
            keys.Dispose();
        }
        
        private void AnalyzeAndAdapt()
        {
            // Build current player profile
            CurrentProfile = new PlayerProfile
            {
                AggressionLevel = GetMetric(PlayerBehaviorMetric.AggressiveEngagement),
                StealthLevel = GetMetric(PlayerBehaviorMetric.StealthPreference),
                RangePreference = GetMetric(PlayerBehaviorMetric.LongRangeCombat) - GetMetric(PlayerBehaviorMetric.CloseQuartersCombat),
                ExplosiveTendency = GetMetric(PlayerBehaviorMetric.ExplosiveUsage),
                VehicleReliance = GetMetric(PlayerBehaviorMetric.VehicleDependency),
                RiskProfile = GetMetric(PlayerBehaviorMetric.RiskTaking),
                Defensiveness = GetMetric(PlayerBehaviorMetric.DefensivePositioning),
                AdaptabilityScore = GetMetric(PlayerBehaviorMetric.Adaptability)
            };
            
            // Generate adaptations
            _activeAdaptations.Clear();
            
            if (CurrentProfile.AggressionLevel > 0.7f)
            {
                _activeAdaptations.Add(new EnemyAdaptation
                {
                    AdaptationType = EnemyAdaptationType.IncreasedDefensiveness,
                    Strength = CurrentProfile.AggressionLevel,
                    Duration = _analysisWindow * 2
                });
            }
            
            if (CurrentProfile.StealthLevel > 0.7f)
            {
                _activeAdaptations.Add(new EnemyAdaptation
                {
                    AdaptationType = EnemyAdaptationType.EnhancedDetection,
                    Strength = CurrentProfile.StealthLevel,
                    Duration = _analysisWindow * 2
                });
            }
            
            if (CurrentProfile.RangePreference > 0.5f)
            {
                _activeAdaptations.Add(new EnemyAdaptation
                {
                    AdaptationType = EnemyAdaptationType.CloseRangePressure,
                    Strength = CurrentProfile.RangePreference,
                    Duration = _analysisWindow * 2
                });
            }
            
            if (CurrentProfile.ExplosiveTendency > 0.6f)
            {
                _activeAdaptations.Add(new EnemyAdaptation
                {
                    AdaptationType = EnemyAdaptationType.DispersedFormations,
                    Strength = CurrentProfile.ExplosiveTendency,
                    Duration = _analysisWindow * 2
                });
            }
            
            if (CurrentProfile.VehicleReliance > 0.7f)
            {
                _activeAdaptations.Add(new EnemyAdaptation
                {
                    AdaptationType = EnemyAdaptationType.AntiVehicleFocus,
                    Strength = CurrentProfile.VehicleReliance,
                    Duration = _analysisWindow * 2
                });
            }
            
            // Publish adaptation events
            foreach (var adaptation in _activeAdaptations)
            {
                _eventBus.Publish(new EnemyAdaptedEvent
                {
                    Adaptation = adaptation,
                    PlayerProfile = CurrentProfile
                });
            }
        }
        
        private float GetMetric(PlayerBehaviorMetric metric)
        {
            return _behaviorMetrics.TryGetValue(metric, out var value) ? value : 0f;
        }
        
        public EnemyAdaptation GetActiveAdaptation(EnemyAdaptationType type)
        {
            foreach (var adaptation in _activeAdaptations)
            {
                if (adaptation.AdaptationType == type)
                    return adaptation;
            }
            return default;
        }
        
        public NativeArray<EnemyAdaptation> GetAllAdaptations()
        {
            return _activeAdaptations.ToArray(Allocator.Temp);
        }
        
        public void Dispose()
        {
            _behaviorMetrics.Dispose();
            _activeAdaptations.Dispose();
        }
    }
    
    public struct PlayerProfile
    {
        public float AggressionLevel;       // 0-1
        public float StealthLevel;          // 0-1
        public float RangePreference;       // -1 (close) to 1 (long)
        public float ExplosiveTendency;     // 0-1
        public float VehicleReliance;       // 0-1
        public float RiskProfile;           // 0-1
        public float Defensiveness;         // 0-1
        public float AdaptabilityScore;     // 0-1
    }
    
    public struct EnemyAdaptation
    {
        public EnemyAdaptationType AdaptationType;
        public float Strength;              // 0-1
        public int Duration;                // frames
    }
    
    public enum EnemyAdaptationType
    {
        IncreasedDefensiveness,
        EnhancedDetection,
        CloseRangePressure,
        DispersedFormations,
        AntiVehicleFocus,
        FlankingBehavior,
        AmbushTactics,
        ResourceDenial,
        PsychologicalWarfare,
        EvolutionMutation
    }
    
    public struct PlayerActionEvent
    {
        public PlayerActionType ActionType;
        public float Range;
        public float DamageTaken;
        public bool IsExplosive;
        public string WeaponUsed;
        public UnityEngine.Vector3 Position;
    }
    
    public enum PlayerActionType
    {
        WeaponFire,
        TakeDamage,
        UseCover,
        StealthKill,
        VehicleEnter,
        ResourceCollect,
        Flee,
        Charge,
        Heal,
        BuildStructure,
        CraftItem,
        Trade
    }
    
    public enum PlayerBehaviorMetric
    {
        AggressiveEngagement,
        DefensivePositioning,
        StealthPreference,
        LongRangeCombat,
        CloseQuartersCombat,
        ExplosiveUsage,
        VehicleDependency,
        ResourceHoarder,
        RiskTaking,
        TacticalRetreat,
        TeamCoordination,
        Adaptability
    }
    
    // Events
    public struct EnemyAdaptedEvent
    {
        public EnemyAdaptation Adaptation;
        public PlayerProfile PlayerProfile;
    }
}
