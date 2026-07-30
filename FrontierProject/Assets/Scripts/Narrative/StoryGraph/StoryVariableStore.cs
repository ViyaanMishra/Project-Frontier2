using System;
using Unity.Collections;
using Unity.Entities;
using Frontier.Core;

namespace Frontier.Narrative.StoryGraph
{
    /// <summary>
    /// Centralized storage for all narrative variables, flags, and state.
    /// Supports nested scopes, temporal tracking, and reactive conditions.
    /// </summary>
    [Serializable]
    public class StoryVariableStore : IService
    {
        private NativeHashMap<FixedString64Bytes, StoryVariable> _variables;
        private NativeHashMap<FixedString64Bytes, StoryCondition> _conditions;
        private NativeHashMap<FixedString64Bytes, StoryEffect> _effects;
        private NativeList<VariableScope> _scopeStack;
        
        public int Priority => 5;

        public void Initialize()
        {
            _variables = new NativeHashMap<FixedString64Bytes, StoryVariable>(256, Allocator.Persistent);
            _conditions = new NativeHashMap<FixedString64Bytes, StoryCondition>(128, Allocator.Persistent);
            _effects = new NativeHashMap<FixedString64Bytes, StoryEffect>(128, Allocator.Persistent);
            _scopeStack = new NativeList<VariableScope>(Allocator.Persistent);
            
            // Push global scope
            _scopeStack.Add(new VariableScope { Name = "GLOBAL", Depth = 0 });
        }

        public void Tick(double deltaTime)
        {
            // Process temporal variables
            var enumerator = _variables.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var var = enumerator.Current.Value;
                if (var.ExpirationTicks > 0 && MasterClock.Instance.TotalTicks >= var.ExpirationTicks)
                {
                    var.IsExpired = true;
                    _variables[enumerator.Current.Key] = var;
                }
            }
        }

        public void Shutdown()
        {
            if (_variables.IsCreated) _variables.Dispose();
            if (_conditions.IsCreated) _conditions.Dispose();
            if (_effects.IsCreated) _effects.Dispose();
            if (_scopeStack.IsCreated) _scopeStack.Dispose();
        }

        public void SetVariable(FixedString64Bytes name, object value, int durationTicks = 0)
        {
            var var = new StoryVariable
            {
                Name = name,
                Value = value,
                CreationTicks = MasterClock.Instance.TotalTicks,
                ExpirationTicks = durationTicks > 0 ? MasterClock.Instance.TotalTicks + durationTicks : 0,
                ScopeDepth = _scopeStack.Length - 1,
                IsExpired = false
            };
            _variables[name] = var;
            
            EventBus.Publish(new VariableChangedEvent { VariableName = name, NewValue = value });
        }

        public T GetVariable<T>(FixedString64Bytes name)
        {
            if (_variables.TryGetValue(name, out var var) && !var.IsExpired)
            {
                return (T)var.Value;
            }
            return default;
        }

        public bool HasVariable(FixedString64Bytes name)
        {
            return _variables.TryGetValue(name, out var var) && !var.IsExpired;
        }

        public void RegisterCondition(FixedString64Bytes id, StoryCondition condition)
        {
            _conditions[id] = condition;
        }

        public bool EvaluateCondition(FixedString64Bytes id)
        {
            if (!_conditions.TryGetValue(id, out var condition))
                return false;

            return condition.Evaluate(this);
        }

        public void RegisterEffect(FixedString64Bytes id, StoryEffect effect)
        {
            _effects[id] = effect;
        }

        public void ExecuteEffect(FixedString64Bytes id)
        {
            if (_effects.TryGetValue(id, out var effect))
            {
                effect.Execute(this);
            }
        }

        public void PushScope(FixedString64Bytes name)
        {
            _scopeStack.Add(new VariableScope 
            { 
                Name = name, 
                Depth = _scopeStack.Length 
            });
        }

        public void PopScope()
        {
            if (_scopeStack.Length > 1)
            {
                var scope = _scopeStack[_scopeStack.Length - 1];
                // Clean up variables in this scope
                var enumerator = _variables.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Value.ScopeDepth >= scope.Depth)
                    {
                        _variables.Remove(enumerator.Current.Key);
                    }
                }
                _scopeStack.RemoveAt(_scopeStack.Length - 1);
            }
        }

        public void SaveState(NativeBuffer<byte> buffer)
        {
            // Serialization logic for save games
        }

        public void LoadState(NativeBuffer<byte> buffer)
        {
            // Deserialization logic for save games
        }
    }

    [Serializable]
    public struct StoryVariable
    {
        public FixedString64Bytes Name;
        public object Value;
        public double CreationTicks;
        public double ExpirationTicks;
        public int ScopeDepth;
        public bool IsExpired;
    }

    [Serializable]
    public struct VariableScope
    {
        public FixedString64Bytes Name;
        public int Depth;
    }

    public abstract class StoryCondition
    {
        public abstract bool Evaluate(StoryVariableStore store);
    }

    public abstract class StoryEffect
    {
        public abstract void Execute(StoryVariableStore store);
    }

    #region Events
    public struct VariableChangedEvent : IEvent
    {
        public FixedString64Bytes VariableName;
        public object NewValue;
    }
    #endregion
}
