using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Frontier.Simulation
{
    /// <summary>
    /// Fixed-tick simulation driver running at 60 ticks/sec base.
    /// Manages time accumulation and triggers simulation updates.
    /// </summary>
    public struct MasterClock
    {
        public const int TicksPerSecond = 60;
        public const float TickDeltaTime = 1f / TicksPerSecond;
        
        private double _accumulatedTime;
        private long _totalTicks;
        private bool _isPaused;
        private float _timeScale;
        
        public long TotalTicks => _totalTicks;
        public double AccumulatedTime => _accumulatedTime;
        public bool IsPaused => _isPaused;
        public float TimeScale => _timeScale;
        
        public void Initialize()
        {
            _accumulatedTime = 0.0;
            _totalTicks = 0;
            _isPaused = false;
            _timeScale = 1.0f;
        }
        
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }
        
        public void SetTimeScale(float scale)
        {
            _timeScale = Mathf.Max(0f, scale);
        }
        
        public void Advance(float deltaTime)
        {
            if (_isPaused || _timeScale <= 0f)
                return;
            
            _accumulatedTime += deltaTime * _timeScale;
            
            // Catch up simulation ticks
            while (_accumulatedTime >= TickDeltaTime)
            {
                _accumulatedTime -= TickDeltaTime;
                _totalTicks++;
                
                // Trigger tick event via EventBus
                var tickEvent = new OnSimulationTick
                {
                    TickNumber = _totalTicks,
                    DeltaTime = TickDeltaTime
                };
                EventBus.Publish(tickEvent);
            }
        }
        
        public void ForceTick()
        {
            _totalTicks++;
            var tickEvent = new OnSimulationTick
            {
                TickNumber = _totalTicks,
                DeltaTime = TickDeltaTime
            };
            EventBus.Publish(tickEvent);
        }
        
        public double GetInterpolationAlpha()
        {
            return _accumulatedTime / TickDeltaTime;
        }
    }
    
    /// <summary>
    /// Event fired on each simulation tick.
    /// </summary>
    public struct OnSimulationTick
    {
        public long TickNumber;
        public float DeltaTime;
    }
    
    /// <summary>
    /// MonoBehaviour wrapper for MasterClock to integrate with Unity lifecycle.
    /// </summary>
    public class MasterClockBehaviour : MonoBehaviour
    {
        private static MasterClockBehaviour _instance;
        public static MasterClock Instance => _instance._clock;
        
        private MasterClock _clock;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _clock.Initialize();
        }
        
        private void Update()
        {
            _clock.Advance(Time.deltaTime);
        }
        
        public void SetPaused(bool paused) => _clock.SetPaused(paused);
        public void SetTimeScale(float scale) => _clock.SetTimeScale(scale);
        public void ForceTick() => _clock.ForceTick();
    }
}
