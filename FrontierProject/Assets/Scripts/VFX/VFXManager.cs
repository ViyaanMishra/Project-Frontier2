using System.Collections.Generic;
using UnityEngine;

namespace Frontier.VFX
{
    /// <summary>
    /// Visual effects manager for particle systems and environmental VFX.
    /// Handles pooling, spawning, and lifecycle of VFX instances.
    /// </summary>
    public class VFXManager
    {
        private Dictionary<string, Queue<GameObject>> _pooledVFX = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _vfxPrefabs = new Dictionary<string, GameObject>();
        private List<GameObject> _activeVFX = new List<GameObject>();
        
        // Parent transform for organization
        private Transform _vfxParent;

        public VFXManager(Transform parent = null)
        {
            _vfxParent = parent ?? new GameObject("VFX_Pool").transform;
        }

        /// <summary>
        /// Register a VFX prefab for pooling.
        /// </summary>
        public void RegisterVFX(string id, GameObject prefab, int initialPoolSize = 10)
        {
            if (_vfxPrefabs.ContainsKey(id))
            {
                Debug.LogWarning($"[VFXManager] VFX already registered: {id}");
                return;
            }

            _vfxPrefabs[id] = prefab;
            _pooledVFX[id] = new Queue<GameObject>();

            // Pre-instantiate pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                var instance = Object.Instantiate(prefab, _vfxParent);
                instance.SetActive(false);
                _pooledVFX[id].Enqueue(instance);
            }

            Debug.Log($"[VFXManager] Registered VFX: {id} with pool size {initialPoolSize}");
        }

        /// <summary>
        /// Spawn a VFX at position.
        /// </summary>
        public GameObject SpawnVFX(string id, Vector3 position, Quaternion rotation = default)
        {
            if (!_vfxPrefabs.ContainsKey(id))
            {
                Debug.LogError($"[VFXManager] Unknown VFX: {id}");
                return null;
            }

            GameObject instance;

            // Get from pool or create new
            if (_pooledVFX[id].Count > 0)
            {
                instance = _pooledVFX[id].Dequeue();
            }
            else
            {
                // Expand pool
                instance = Object.Instantiate(_vfxPrefabs[id], _vfxParent);
                Debug.Log($"[VFXManager] Expanded pool for: {id}");
            }

            instance.transform.position = position;
            instance.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            instance.SetActive(true);

            _activeVFX.Add(instance);

            // Auto-despawn after particle system duration
            var ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                float duration = main.duration + main.startLifetime.constantMax;
                
                // Use simple coroutine alternative via MonoBehaviour or timer
                // In production, would use a proper timing system
                DespawnAfter(instance, id, duration);
            }

            return instance;
        }

        /// <summary>
        /// Spawn VFX on a target (follows target briefly).
        /// </summary>
        public GameObject SpawnVFXOnTarget(string id, Transform target, float followDuration = 1f)
        {
            var instance = SpawnVFX(id, target.position);
            
            if (instance != null && followDuration > 0)
            {
                // Simple follow logic - in production would use proper tracking
                var followData = new VFxFollowData
                {
                    Instance = instance,
                    Target = target,
                    Duration = followDuration,
                    Elapsed = 0f
                };
                _followTargets.Add(followData);
            }

            return instance;
        }

        /// <summary>
        /// Spawn area VFX (spawns multiple in a radius).
        /// </summary>
        public List<GameObject> SpawnAreaVFX(string id, Vector3 center, float radius, int count)
        {
            var instances = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                Vector3 randomPos = center + Random.insideUnitSphere * radius;
                randomPos.y = center.y; // Keep on same plane
                instances.Add(SpawnVFX(id, randomPos));
            }

            return instances;
        }

        /// <summary>
        /// Despawn a VFX and return to pool.
        /// </summary>
        public void DespawnVFX(GameObject instance, string id)
        {
            if (instance == null || !_pooledVFX.ContainsKey(id))
                return;

            instance.SetActive(false);
            instance.transform.SetParent(_vfxParent);
            
            _pooledVFX[id].Enqueue(instance);
            _activeVFX.Remove(instance);
        }

        /// <summary>
        /// Despawn all active VFX.
        /// </summary>
        public void DespawnAll()
        {
            for (int i = _activeVFX.Count - 1; i >= 0; i--)
            {
                var instance = _activeVFX[i];
                if (instance != null)
                {
                    instance.SetActive(false);
                    instance.transform.SetParent(_vfxParent);
                }
            }
            _activeVFX.Clear();

            // Clear follow targets
            _followTargets.Clear();
        }

        /// <summary>
        /// Update VFX follow targets.
        /// </summary>
        public void Update(float deltaTime)
        {
            for (int i = _followTargets.Count - 1; i >= 0; i--)
            {
                var data = _followTargets[i];
                data.Elapsed += deltaTime;

                if (data.Target != null && data.Instance != null)
                {
                    data.Instance.transform.position = data.Target.position;
                }

                if (data.Elapsed >= data.Duration)
                {
                    // Find the ID for this VFX (simplified - would store ID in follow data)
                    DespawnVFX(data.Instance, "unknown");
                    _followTargets.RemoveAt(i);
                }
                else
                {
                    _followTargets[i] = data;
                }
            }
        }

        private void DespawnAfter(GameObject instance, string id, float delay)
        {
            // Simplified - in production would use proper timing system
            // This is a placeholder for coroutine or job-based timing
            Debug.Log($"[VFXManager] Will despawn {id} after {delay}s");
        }

        private List<VFxFollowData> _followTargets = new List<VFxFollowData>();

        private struct VFxFollowData
        {
            public GameObject Instance;
            public Transform Target;
            public float Duration;
            public float Elapsed;
        }

        /// <summary>
        /// Get count of active VFX instances.
        /// </summary>
        public int GetActiveCount() => _activeVFX.Count;

        /// <summary>
        /// Get pool statistics.
        /// </summary>
        public Dictionary<string, int> GetPoolStats()
        {
            var stats = new Dictionary<string, int>();
            foreach (var kvp in _pooledVFX)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }
            return stats;
        }
    }

    /// <summary>
    /// Environmental VFX controller for weather and ambient effects.
    /// </summary>
    public class EnvironmentalVFX
    {
        private ParticleSystem _rainPS;
        private ParticleSystem _snowPS;
        private ParticleSystem _dustPS;
        private ParticleSystem _embersPS;
        
        private AudioSource _ambientAudio;
        private float _currentIntensity = 0f;

        public void Initialize(ParticleSystem rain, ParticleSystem snow, ParticleSystem dust, ParticleSystem embers)
        {
            _rainPS = rain;
            _snowPS = snow;
            _dustPS = dust;
            _embersPS = embers;

            // Start all disabled
            SetWeatherEffect(WeatherType.Clear, 0f);
        }

        public enum WeatherType
        {
            Clear,
            Rain,
            HeavyRain,
            Snow,
            Blizzard,
            Sandstorm,
            Ash,
            Anomaly
        }

        /// <summary>
        /// Set weather effect with smooth transition.
        /// </summary>
        public void SetWeatherEffect(WeatherType type, float intensity)
        {
            intensity = Mathf.Clamp01(intensity);
            _currentIntensity = intensity;

            // Disable all first
            SetParticleSystemActive(_rainPS, false);
            SetParticleSystemActive(_snowPS, false);
            SetParticleSystemActive(_dustPS, false);
            SetParticleSystemActive(_embersPS, false);

            // Enable appropriate effect
            switch (type)
            {
                case WeatherType.Rain:
                case WeatherType.HeavyRain:
                    SetParticleSystemActive(_rainPS, true);
                    AdjustEmissionRate(_rainPS, intensity * (type == WeatherType.HeavyRain ? 2f : 1f));
                    break;

                case WeatherType.Snow:
                case WeatherType.Blizzard:
                    SetParticleSystemActive(_snowPS, true);
                    AdjustEmissionRate(_snowPS, intensity * (type == WeatherType.Blizzard ? 2f : 1f));
                    break;

                case WeatherType.Sandstorm:
                    SetParticleSystemActive(_dustPS, true);
                    AdjustEmissionRate(_dustPS, intensity);
                    break;

                case WeatherType.Ash:
                    SetParticleSystemActive(_embersPS, true);
                    AdjustEmissionRate(_embersPS, intensity);
                    break;

                case WeatherType.Anomaly:
                    // Special handling for anomaly storms
                    SetParticleSystemActive(_rainPS, true);
                    SetParticleSystemActive(_embersPS, true);
                    AdjustEmissionRate(_rainPS, intensity * 0.5f);
                    AdjustEmissionRate(_embersPS, intensity);
                    break;
            }
        }

        private void SetParticleSystemActive(ParticleSystem ps, bool active)
        {
            if (ps == null) return;

            if (active)
            {
                if (!ps.isPlaying)
                    ps.Play();
            }
            else
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }

        private void AdjustEmissionRate(ParticleSystem ps, float multiplier)
        {
            if (ps == null) return;

            var emission = ps.emission;
            var rate = emission.rateOverTime;
            rate.constant *= multiplier;
            emission.rateOverTime = rate;
        }

        /// <summary>
        /// Trigger localized VFX (explosion, impact, etc.).
        /// </summary>
        public void TriggerLocalEffect(Vector3 position, LocalEffectType type, float scale = 1f)
        {
            switch (type)
            {
                case LocalEffectType.Explosion:
                    // Spawn explosion VFX
                    break;
                case LocalEffectType.Impact:
                    // Spawn impact sparks/debris
                    break;
                case LocalEffectType.Blood:
                    // Spawn blood splatter
                    break;
            }
        }

        public enum LocalEffectType
        {
            Explosion,
            Impact,
            Blood,
            Smoke,
            Fire,
            Electricity
        }
    }
}
