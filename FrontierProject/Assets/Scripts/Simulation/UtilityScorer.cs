using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// AI utility evaluation system using Burst jobs for parallel scoring.
    /// Evaluates actions based on multiple weighted factors.
    /// </summary>
    public class UtilityScorer : IDisposable
    {
        [Serializable]
        public struct UtilityFactor
        {
            public string Name;
            public float Weight;
            public Func<float> Evaluator; // Callback to evaluate this factor
        }
        
        [Serializable]
        public struct ActionOption
        {
            public string ActionName;
            public NativeArray<float> FactorScores; // Normalized 0-1 scores per factor
            public float TotalScore;
            public int Priority;
        }
        
        private NativeList<UtilityFactor> _factors;
        private NativeList<ActionOption> _actionOptions;
        private int _maxFactors;
        private int _maxActions;
        
        public int FactorCount => _factors.Length;
        public int ActionCount => _actionOptions.Length;
        
        public UtilityScorer(int maxFactors = 16, int maxActions = 32)
        {
            _maxFactors = maxFactors;
            _maxActions = maxActions;
            _factors = new NativeList<UtilityFactor>(maxFactors, Allocator.Persistent);
            _actionOptions = new NativeList<ActionOption>(maxActions, Allocator.Persistent);
        }
        
        public void AddFactor(string name, float weight, Func<float> evaluator)
        {
            if (_factors.Length >= _maxFactors)
            {
                Debug.LogWarning($"UtilityScorer: Max factors ({_maxFactors}) exceeded!");
                return;
            }
            
            _factors.Add(new UtilityFactor
            {
                Name = name,
                Weight = weight,
                Evaluator = evaluator
            });
        }
        
        public void ClearFactors()
        {
            _factors.Clear();
        }
        
        public void AddAction(string actionName)
        {
            if (_actionOptions.Length >= _maxActions)
            {
                Debug.LogWarning($"UtilityScorer: Max actions ({_maxActions}) exceeded!");
                return;
            }
            
            var scores = new NativeArray<float>(_factors.Length, Allocator.Temp);
            for (int i = 0; i < _factors.Length; i++)
            {
                scores[i] = 0f;
            }
            
            _actionOptions.Add(new ActionOption
            {
                ActionName = actionName,
                FactorScores = scores,
                TotalScore = 0f,
                Priority = 0
            });
        }
        
        public void SetFactorScore(int actionIndex, int factorIndex, float score)
        {
            if (actionIndex < 0 || actionIndex >= _actionOptions.Length)
                return;
            if (factorIndex < 0 || factorIndex >= _factors.Length)
                return;
            
            ActionOption action = _actionOptions[actionIndex];
            action.FactorScores[factorIndex] = Mathf.Clamp01(score);
            _actionOptions[actionIndex] = action;
        }
        
        public void CalculateAllScores()
        {
            for (int i = 0; i < _actionOptions.Length; i++)
            {
                ActionOption action = _actionOptions[i];
                float totalScore = 0f;
                
                for (int j = 0; j < _factors.Length; j++)
                {
                    float factorScore = action.FactorScores[j];
                    float weight = _factors[j].Weight;
                    totalScore += factorScore * weight;
                }
                
                action.TotalScore = totalScore;
                _actionOptions[i] = action;
            }
        }
        
        public int GetBestActionIndex()
        {
            if (_actionOptions.Length == 0)
                return -1;
            
            int bestIndex = 0;
            float bestScore = _actionOptions[0].TotalScore;
            
            for (int i = 1; i < _actionOptions.Length; i++)
            {
                if (_actionOptions[i].TotalScore > bestScore)
                {
                    bestScore = _actionOptions[i].TotalScore;
                    bestIndex = i;
                }
            }
            
            return bestIndex;
        }
        
        public string GetBestActionName()
        {
            int bestIndex = GetBestActionIndex();
            if (bestIndex < 0)
                return "None";
            return _actionOptions[bestIndex].ActionName;
        }
        
        public void ClearActions()
        {
            for (int i = 0; i < _actionOptions.Length; i++)
            {
                _actionOptions[i].FactorScores.Dispose();
            }
            _actionOptions.Clear();
        }
        
        public void Dispose()
        {
            ClearActions();
            _factors.Dispose();
            _actionOptions.Dispose();
        }
    }
    
    /// <summary>
    /// Burst job for parallel utility scoring of multiple AI agents.
    /// </summary>
    [BurstCompile]
    public struct UtilityScoringJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> FactorData; // [factorIndex][weight, score]
        [WriteOnly] public NativeArray<float> TotalScores;
        public int FactorCount;
        
        public void Execute(int index)
        {
            int offset = index * FactorCount;
            float totalScore = 0f;
            
            for (int i = 0; i < FactorCount; i++)
            {
                float2 factor = FactorData[offset + i];
                totalScore += factor.x * factor.y; // weight * score
            }
            
            TotalScores[index] = totalScore;
        }
    }
    
    /// <summary>
    /// Common utility factors for AI decision making.
    /// </summary>
    public static class UtilityFactors
    {
        public const string Survival_Hunger = "Survival.Hunger";
        public const string Survival_Thirst = "Survival.Thirst";
        public const string Survival_Health = "Survival.Health";
        public const string Survival_Safety = "Survival.Safety";
        public const string Social_Isolation = "Social.Isolation";
        public const string Social_Bonding = "Social.Bonding";
        public const string Resource_Food = "Resource.Food";
        public const string Resource_Water = "Resource.Water";
        public const string Resource_Shelter = "Resource.Shelter";
        public const string Combat_Threat = "Combat.Threat";
        public const string Combat_Opportunity = "Combat.Opportunity";
        public const string Task_Urgency = "Task.Urgency";
        public const string Task_Reward = "Task.Reward";
        public const string Task_Distance = "Task.Distance";
        public const string Emotional_Stress = "Emotional.Stress";
        public const string Emotional_Boredom = "Emotional.Boredom";
    }
}
