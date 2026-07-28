using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Frontier.AI
{
    /// <summary>
    /// Utility-based AI decision maker.
    /// Evaluates actions based on weighted scores considering needs, threats, and goals.
    /// </summary>
    public struct AITask
    {
        public string taskId;
        public float priority;
        public float urgency;
        public float successChance;
        public float rewardValue;
        public float costValue;
        public bool isInterruptible;
        public float executionTime;
    }

    public class AIBrain : IDisposable
    {
        private NativeList<AITask> _availableTasks;
        private AITask _currentTask;
        private int _entityId;
        private float _lastDecisionTime;
        private readonly float _decisionInterval = 1f; // Decide every second
        
        public AIBrain(int entityId)
        {
            _entityId = entityId;
            _availableTasks = new NativeList<AITask>(Allocator.Persistent);
            _currentTask = default;
            _lastDecisionTime = 0f;
        }
        
        public void AddTask(AITask task)
        {
            _availableTasks.Add(task);
        }
        
        public void ClearTasks()
        {
            _availableTasks.Clear();
        }
        
        public AITask EvaluateBestTask(float gameTime)
        {
            if (gameTime - _lastDecisionTime < _decisionInterval && _currentTask.taskId != null)
                return _currentTask;
            
            _lastDecisionTime = gameTime;
            
            if (_availableTasks.Length == 0)
                return default;
            
            AITask bestTask = default;
            float bestScore = float.MinValue;
            
            for (int i = 0; i < _availableTasks.Length; i++)
            {
                var task = _availableTasks[i];
                float score = CalculateUtilityScore(task);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTask = task;
                }
            }
            
            // Check if current task should be interrupted
            if (_currentTask.taskId != null && _currentTask.isInterruptible && bestScore > _currentTask.priority * 1.5f)
            {
                // Interrupt current task
                _currentTask = bestTask;
                return bestTask;
            }
            else if (_currentTask.taskId == null || _currentTask.executionTime <= 0)
            {
                _currentTask = bestTask;
                return bestTask;
            }
            
            _currentTask.executionTime -= gameTime - _lastDecisionTime;
            return _currentTask;
        }
        
        [BurstCompile]
        public struct UtilityCalculationJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<AITask> tasks;
            [WriteOnly] public NativeArray<float> scores;
            [ReadOnly] public NativeArray<float> needWeights; // Hunger, Safety, Social, etc.
            
            public void Execute(int index)
            {
                var task = tasks[index];
                
                // Base utility calculation
                float score = task.priority * 0.3f +
                             task.urgency * 0.3f +
                             task.rewardValue * 0.2f -
                             task.costValue * 0.1f +
                             task.successChance * 0.1f;
                
                scores[index] = score;
            }
        }
        
        private float CalculateUtilityScore(AITask task)
        {
            // Weighted scoring based on entity needs
            float score = task.priority * 0.25f;
            score += task.urgency * 0.25f;
            score += task.rewardValue * 0.2f;
            score -= task.costValue * 0.15f;
            score += task.successChance * 0.15f;
            
            return score;
        }
        
        public void UpdateCurrentTask(float deltaTime)
        {
            if (_currentTask.taskId != null)
            {
                _currentTask.executionTime -= deltaTime;
                if (_currentTask.executionTime <= 0)
                {
                    // Task completed
                    _currentTask = default;
                }
            }
        }
        
        public AITask GetCurrentTask() => _currentTask;
        
        public bool HasActiveTask() => _currentTask.taskId != null && _currentTask.executionTime > 0;
        
        public void Dispose()
        {
            if (_availableTasks.IsCreated) _availableTasks.Dispose();
        }
    }
}
