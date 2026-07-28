using System;
using UnityEngine;

namespace Frontier.Core
{
    /// <summary>
    /// Time controller with variable time scale (1x/2x/4x/Pause/DebugStep).
    /// Uses fixed-tick accumulator for deterministic simulation.
    /// </summary>
    public class TimeController
    {
        // Base tick rate (60 ticks/sec)
        private const float BASE_TICK_RATE = 60f;
        private const float MIN_TICK_RATE = 30f;
        private const float MAX_TICK_RATE = 240f;

        // Time scales
        public enum TimeScale
        {
            Pause = 0,
            Normal = 1f,
            Fast = 2f,
            UltraFast = 4f,
            DebugStep = 0.1f
        }

        private float _tickAccumulator;
        private float _fixedDeltaTime;
        private float _timeScaleMultiplier;
        private float _unscaledTime;
        private float _scaledTime;
        private long _tickCount;
        private bool _isStepping;
        private TimeScale _currentTimeScale;

        public float TickRate => BASE_TICK_RATE;
        public float FixedDeltaTime => _fixedDeltaTime;
        public float TimeScaleMultiplier => _timeScaleMultiplier;
        public float UnscaledTime => _unscaledTime;
        public float ScaledTime => _scaledTime;
        public long TickCount => _tickCount;
        public bool IsStepping => _isStepping;
        public TimeScale CurrentTimeScale => _currentTimeScale;

        // Events
        public event Action OnTick;
        public event Action<float> OnTimeScaleChanged;

        public TimeController()
        {
            _fixedDeltaTime = 1f / BASE_TICK_RATE;
            _timeScaleMultiplier = 1f;
            _unscaledTime = 0f;
            _scaledTime = 0f;
            _tickCount = 0;
            _isStepping = false;
            _currentTimeScale = TimeScale.Normal;
        }

        public void Initialize()
        {
            SetTimeScale(TimeScale.Normal);
            Debug.Log($"[TimeController] Initialized with tick rate {BASE_TICK_RATE} Hz");
        }

        /// <summary>
        /// Update scaled time (call in MonoBehaviour.Update).
        /// </summary>
        public void Update(float deltaTime)
        {
            _unscaledTime += deltaTime;
            _scaledTime += deltaTime * _timeScaleMultiplier;
        }

        /// <summary>
        /// Fixed update with tick accumulation (call in MonoBehaviour.FixedUpdate).
        /// Returns number of ticks processed.
        /// </summary>
        public int FixedUpdate()
        {
            if (_timeScaleMultiplier <= 0f || _isStepping)
            {
                return 0;
            }

            float scaledDelta = Time.fixedDeltaTime * _timeScaleMultiplier;
            _tickAccumulator += scaledDelta;

            int ticksProcessed = 0;
            int maxTicksPerFrame = 10; // Prevent spiral of death

            while (_tickAccumulator >= _fixedDeltaTime && ticksProcessed < maxTicksPerFrame)
            {
                DoTick();
                _tickAccumulator -= _fixedDeltaTime;
                ticksProcessed++;
            }

            // Catch up if we're falling behind
            if (ticksProcessed >= maxTicksPerFrame && _tickAccumulator > _fixedDeltaTime * 2f)
            {
                Debug.LogWarning($"[TimeController] Falling behind: {_tickAccumulator / _fixedDeltaTime:F0} ticks queued");
                _tickAccumulator = _fixedDeltaTime; // Reset to prevent further backlog
            }

            return ticksProcessed;
        }

        /// <summary>
        /// Execute a single simulation tick.
        /// </summary>
        private void DoTick()
        {
            _tickCount++;
            OnTick?.Invoke();
        }

        /// <summary>
        /// Set the time scale multiplier.
        /// </summary>
        public void SetTimeScale(TimeScale scale)
        {
            _currentTimeScale = scale;
            _timeScaleMultiplier = (float)scale;
            _isStepping = false;

            OnTimeScaleChanged?.Invoke(_timeScaleMultiplier);
            Debug.Log($"[TimeController] Time scale set to {scale} ({_timeScaleMultiplier}x)");
        }

        /// <summary>
        /// Set custom time scale multiplier.
        /// </summary>
        public void SetCustomTimeScale(float multiplier)
        {
            multiplier = Mathf.Clamp(multiplier, 0f, 10f);
            _timeScaleMultiplier = multiplier;
            _currentTimeScale = TimeScale.Normal;
            _isStepping = false;

            OnTimeScaleChanged?.Invoke(_timeScaleMultiplier);
        }

        /// <summary>
        /// Execute a single tick in pause mode (for debug stepping).
        /// </summary>
        public void Step()
        {
            _isStepping = true;
            _timeScaleMultiplier = 0f;
            DoTick();
        }

        /// <summary>
        /// Toggle pause state.
        /// </summary>
        public void TogglePause()
        {
            if (_timeScaleMultiplier > 0f)
            {
                SetTimeScale(TimeScale.Pause);
            }
            else
            {
                SetTimeScale(TimeScale.Normal);
            }
        }

        /// <summary>
        /// Get interpolated time between ticks (for rendering).
        /// </summary>
        public float GetInterpolationAlpha()
        {
            if (_fixedDeltaTime <= 0f) return 0f;
            return Mathf.Clamp01(_tickAccumulator / _fixedDeltaTime);
        }

        /// <summary>
        /// Reset the tick accumulator (useful after loading a save).
        /// </summary>
        public void ResetAccumulator()
        {
            _tickAccumulator = 0f;
        }

        /// <summary>
        /// Set the tick count (for save/load synchronization).
        /// </summary>
        public void SetTickCount(long count)
        {
            _tickCount = count;
        }

        /// <summary>
        /// Get timing statistics for debugging.
        /// </summary>
        public TimingStats GetStats()
        {
            return new TimingStats
            {
                TickCount = _tickCount,
                Accumulator = _tickAccumulator,
                TicksQueued = (int)(_tickAccumulator / _fixedDeltaTime),
                TimeScale = _timeScaleMultiplier,
                UnscaledTime = _unscaledTime,
                ScaledTime = _scaledTime
            };
        }

        public struct TimingStats
        {
            public long TickCount;
            public float Accumulator;
            public int TicksQueued;
            public float TimeScale;
            public float UnscaledTime;
            public float ScaledTime;
        }
    }

    /// <summary>
    /// Extension methods for Unity's Time using TimeController.
    /// </summary>
    public static class TimeExtensions
    {
        private static TimeController _globalTimeController;

        public static void SetGlobalTimeController(TimeController controller)
        {
            _globalTimeController = controller;
        }

        public static float GameTime => _globalTimeController?.ScaledTime ?? Time.time;
        public static long GameTickCount => _globalTimeController?.TickCount ?? 0;
    }
}
