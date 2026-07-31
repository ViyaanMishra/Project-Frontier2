using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frontier.Core
{
    /// <summary>
    /// Strongly-typed C# event system for decoupled communication.
    /// Supports typed events with payload and listener lifecycle management.
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _listeners;
        private readonly Dictionary<Type, List<Delegate>> _pendingAdditions;
        private readonly HashSet<Type> _dispatchingTypes;
        private bool _isDisposed;

        public int TotalListeners { get; private set; }
        public bool IsDispatching { get; private set; }

        public EventBus()
        {
            _listeners = new Dictionary<Type, List<Delegate>>();
            _pendingAdditions = new Dictionary<Type, List<Delegate>>();
            _dispatchingTypes = new HashSet<Type>();
            _isDisposed = false;
            TotalListeners = 0;
            IsDispatching = false;
        }

        /// <summary>
        /// Subscribe to an event type. Listener must match Action&lt;T&gt; signature.
        /// </summary>
        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EventBus));

            var eventType = typeof(T);

            if (_dispatchingTypes.Contains(eventType))
            {
                if (!_pendingAdditions.TryGetValue(eventType, out var pendingList))
                {
                    pendingList = new List<Delegate>();
                    _pendingAdditions[eventType] = pendingList;
                }
                pendingList.Add(listener);
                return;
            }

            if (!_listeners.TryGetValue(eventType, out var listenerList))
            {
                listenerList = new List<Delegate>();
                _listeners[eventType] = listenerList;
            }

            if (!listenerList.Contains(listener))
            {
                listenerList.Add(listener);
                TotalListeners++;
            }
        }

        /// <summary>
        /// Subscribe without payload (simple signal events).
        /// </summary>
        public void Subscribe<T>(Action listener) where T : struct
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EventBus));

            var eventType = typeof(T);

            if (_dispatchingTypes.Contains(eventType))
            {
                if (!_pendingAdditions.TryGetValue(eventType, out var pendingList))
                {
                    pendingList = new List<Delegate>();
                    _pendingAdditions[eventType] = pendingList;
                }
                pendingList.Add(listener);
                return;
            }

            if (!_listeners.TryGetValue(eventType, out var listenerList))
            {
                listenerList = new List<Delegate>();
                _listeners[eventType] = listenerList;
            }

            if (!listenerList.Contains(listener))
            {
                listenerList.Add(listener);
                TotalListeners++;
            }
        }

        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            if (_isDisposed) return;

            var eventType = typeof(T);

            if (_listeners.TryGetValue(eventType, out var listenerList))
            {
                if (listenerList.Remove(listener))
                {
                    TotalListeners--;
                    
                    if (listenerList.Count == 0 && !_dispatchingTypes.Contains(eventType))
                    {
                        _listeners.Remove(eventType);
                    }
                }
            }

            if (_pendingAdditions.TryGetValue(eventType, out var pendingList))
            {
                pendingList.Remove(listener);
            }
        }

        /// <summary>
        /// Unsubscribe without payload.
        /// </summary>
        public void Unsubscribe<T>(Action listener) where T : struct
        {
            if (_isDisposed) return;

            var eventType = typeof(T);

            if (_listeners.TryGetValue(eventType, out var listenerList))
            {
                if (listenerList.Remove(listener))
                {
                    TotalListeners--;
                    
                    if (listenerList.Count == 0 && !_dispatchingTypes.Contains(eventType))
                    {
                        _listeners.Remove(eventType);
                    }
                }
            }

            if (_pendingAdditions.TryGetValue(eventType, out var pendingList))
            {
                pendingList.Remove(listener);
            }
        }

        /// <summary>
        /// Publish an event with payload to all subscribers.
        /// </summary>
        public void Publish<T>(T eventData) where T : struct
        {
            if (_isDisposed) return;

            var eventType = typeof(T);
            
            if (!_listeners.ContainsKey(eventType))
                return;

            _dispatchingTypes.Add(eventType);
            IsDispatching = true;

            try
            {
                var listenerList = _listeners[eventType];
                var listenersCopy = new List<Delegate>(listenerList);

                foreach (var listener in listenersCopy)
                {
                    try
                    {
                        if (listener is Action<T> typedListener)
                        {
                            typedListener.Invoke(eventData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error in listener for {typeof(T).Name}: {ex.Message}");
                    }
                }
            }
            finally
            {
                _dispatchingTypes.Remove(eventType);
                
                if (_dispatchingTypes.Count == 0)
                {
                    IsDispatching = false;
                    ProcessPendingAdditions();
                }
            }
        }

        /// <summary>
        /// Publish a signal event without payload.
        /// </summary>
        public void Publish<T>() where T : struct
        {
            if (_isDisposed) return;

            var eventType = typeof(T);
            
            if (!_listeners.ContainsKey(eventType))
                return;

            _dispatchingTypes.Add(eventType);
            IsDispatching = true;

            try
            {
                var listenerList = _listeners[eventType];
                var listenersCopy = new List<Delegate>(listenerList);

                foreach (var listener in listenersCopy)
                {
                    try
                    {
                        if (listener is Action typedListener)
                        {
                            typedListener.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error in listener for {typeof(T).Name}: {ex.Message}");
                    }
                }
            }
            finally
            {
                _dispatchingTypes.Remove(eventType);
                
                if (_dispatchingTypes.Count == 0)
                {
                    IsDispatching = false;
                    ProcessPendingAdditions();
                }
            }
        }

        private void ProcessPendingAdditions()
        {
            foreach (var kvp in _pendingAdditions)
            {
                var eventType = kvp.Key;
                var pendingListeners = kvp.Value;

                if (!_listeners.TryGetValue(eventType, out var listenerList))
                {
                    listenerList = new List<Delegate>();
                    _listeners[eventType] = listenerList;
                }

                foreach (var listener in pendingListeners)
                {
                    if (!listenerList.Contains(listener))
                    {
                        listenerList.Add(listener);
                        TotalListeners++;
                    }
                }
            }

            _pendingAdditions.Clear();
        }

        /// <summary>
        /// Check if any listeners are registered for an event type.
        /// </summary>
        public bool HasListeners<T>()
        {
            return _listeners.ContainsKey(typeof(T)) && _listeners[typeof(T)].Count > 0;
        }

        /// <summary>
        /// Get the number of listeners for an event type.
        /// </summary>
        public int GetListenerCount<T>()
        {
            if (_listeners.TryGetValue(typeof(T), out var list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// Clear all listeners for a specific event type.
        /// </summary>
        public void ClearListeners<T>()
        {
            if (_isDisposed) return;

            var eventType = typeof(T);
            
            if (_dispatchingTypes.Contains(eventType))
            {
                Debug.LogWarning($"[EventBus] Cannot clear listeners for {eventType.Name} while dispatching");
                return;
            }

            if (_listeners.TryGetValue(eventType, out var list))
            {
                TotalListeners -= list.Count;
                _listeners.Remove(eventType);
            }

            _pendingAdditions.Remove(eventType);
        }

        /// <summary>
        /// Clear all listeners for all event types.
        /// </summary>
        public void ClearAllListeners()
        {
            if (_isDisposed) return;

            if (IsDispatching)
            {
                Debug.LogWarning("[EventBus] Cannot clear all listeners while dispatching");
                return;
            }

            _listeners.Clear();
            _pendingAdditions.Clear();
            _dispatchingTypes.Clear();
            TotalListeners = 0;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            ClearAllListeners();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Static convenience wrapper for EventBus to enable quick event raising without dependency injection.
    /// Automatically uses the EventBus instance from GameSession when available.
    /// </summary>
    public static class EventBus<T> where T : struct
    {
        private static Action<T> _cachedDelegate;
        
        /// <summary>
        /// Raise an event through the central EventBus.
        /// Falls back to a null check if GameSession is not initialized.
        /// </summary>
        public static void Raise(T eventData)
        {
            if (GameSession.Instance != null && GameSession.Instance.IsInitialized)
            {
                GameSession.Instance.Events.Publish(eventData);
            }
            else
            {
                // Fallback: direct delegate invocation for early initialization scenarios
                _cachedDelegate?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Subscribe to this event type.
        /// </summary>
        public static void Subscribe(Action<T> listener)
        {
            if (GameSession.Instance != null && GameSession.Instance.IsInitialized)
            {
                GameSession.Instance.Events.Subscribe(listener);
            }
            else
            {
                _cachedDelegate += listener;
            }
        }

        /// <summary>
        /// Unsubscribe from this event type.
        /// </summary>
        public static void Unsubscribe(Action<T> listener)
        {
            if (GameSession.Instance != null && GameSession.Instance.IsInitialized)
            {
                GameSession.Instance.Events.Unsubscribe(listener);
            }
            else
            {
                _cachedDelegate -= listener;
            }
        }
    }

    #region Common Event Types

    public enum DamageType
    {
        Physical,
        Energy,
        Fire,
        Explosion,
        Radiation,
        Toxic,
        Environmental
    }

    public struct EntityDamagedEvent
    {
        public EntityGUID EntityId;
        public float DamageAmount;
        public DamageType DamageType;
        public EntityGUID SourceId;
        public Vector3Int HitPosition;
    }

    public struct ChunkLoadedEvent
    {
        public int ChunkX;
        public int ChunkZ;
        public bool IsGenerated;
    }

    public struct ChunkUnloadedEvent
    {
        public int ChunkX;
        public int ChunkZ;
    }

    public struct EntityDiedEvent
    {
        public EntityGUID EntityId;
        public EntityGUID KillerId;
        public Vector3Int Position;
    }

    public struct BuildingConstructedEvent
    {
        public EntityGUID BuildingId;
        public ushort BuildingType;
        public Vector3Int Position;
        public EntityGUID BuilderId;
    }

    public enum WeatherState
    {
        Clear,
        Overcast,
        LightRain,
        HeavyRain,
        Thunderstorm,
        Hail,
        Snow,
        Blizzard,
        Sandstorm,
        AcidRain,
        Fog,
        AnomalyStorm
    }

    public struct WeatherChangedEvent
    {
        public WeatherState PreviousWeather;
        public WeatherState NewWeather;
        public float TransitionDuration;
    }

    #endregion
}
