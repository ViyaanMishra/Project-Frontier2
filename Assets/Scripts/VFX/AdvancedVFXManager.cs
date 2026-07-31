using UnityEngine;
using System.Collections.Generic;

namespace AAA.LowPoly.VFX
{
    /// <summary>
    /// Advanced VFX Particle System Manager
    /// Provides AAA-quality particle effects with GPU instancing, soft particles, and dynamic weather integration
    /// </summary>
    public class AdvancedVFXManager : MonoBehaviour
    {
        [System.Serializable]
        public class VFXPreset
        {
            public string presetName;
            public ParticleSystem[] particleSystems;
            public Light[] vfxLights;
            public AudioSource[] vfxAudio;
            public float intensity = 1f;
            public bool loop = true;
        }

        [Header("Weather Integration")]
        public bool enableWeatherIntegration = true;
        
        [Tooltip("Rain particle system")]
        public ParticleSystem rainParticles;
        
        [Tooltip("Snow particle system")]
        public ParticleSystem snowParticles;
        
        [Tooltip("Fog volume")]
        public GameObject fogVolume;
        
        [Tooltip("Wind zone for particle simulation")]
        public WindZone windZone;
        
        [Range(0, 1)]
        public float weatherIntensity = 0.5f;

        [Header("Dynamic Lighting")]
        [Tooltip("Enable real-time lighting from particles")]
        public bool enableParticleLights = true;
        
        [Tooltip("Maximum number of dynamic lights")]
        public int maxDynamicLights = 8;
        
        [Tooltip("Light update frequency")]
        [Range(1, 60)]
        public int lightUpdateFrequency = 4;
        
        [Tooltip("Light intensity multiplier")]
        public float lightIntensityMultiplier = 1.5f;

        [Header("Soft Particles")]
        [Tooltip("Enable soft particle blending")]
        public bool enableSoftParticles = true;
        
        [Tooltip("Soft particle fade distance")]
        public float softParticleFadeDistance = 2f;
        
        [Tooltip("Camera near fade")]
        public float cameraNearFade = 1f;

        [Header("GPU Instancing")]
        [Tooltip("Enable GPU instancing for particles")]
        public bool enableGPUInstancing = true;
        
        [Tooltip("Maximum particles per batch")]
        public int maxParticlesPerBatch = 1024;

        [Header("Collision")]
        [Tooltip("Enable particle collision with environment")]
        public bool enableParticleCollision = true;
        
        [Tooltip("Collision layers")]
        public LayerMask collisionLayers = -1;
        
        [Tooltip("Spawn collision particles")]
        public ParticleSystem collisionParticleSystem;
        
        [Tooltip("Collision particle spawn chance")]
        [Range(0, 1)]
        public float collisionParticleChance = 0.3f;

        [Header("Performance")]
        [Tooltip("Auto-adjust quality based on framerate")]
        public bool autoQualityAdjustment = true;
        
        [Tooltip("Target framerate")]
        public int targetFPS = 60;
        
        [Tooltip("Minimum particle count")]
        public int minParticleCount = 100;
        
        [Tooltip("Maximum particle count")]
        public int maxParticleCount = 10000;
        
        [Tooltip("Update budget in milliseconds")]
        public float updateBudgetMS = 2f;

        [Header("Presets")]
        public VFXPreset[] presets;
        public int currentPresetIndex = 0;

        [Header("Debug")]
        public bool showDebugInfo = false;
        public bool visualizeParticleBounds = false;

        private Camera mainCamera;
        private ParticleSystem[] allParticleSystems;
        private List<Light> activeVFXLights = new List<Light>();
        private int frameCounter = 0;
        private float currentTimeScale = 1f;
        private Dictionary<ParticleSystem, ParticleSystem.MainModule> mainModules;
        private Dictionary<ParticleSystem, ParticleSystem.EmissionModule> emissionModules;

        void Awake()
        {
            Initialize();
        }

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
            mainCamera = Camera.main;
            
            // Collect all particle systems
            allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
            
            // Cache modules
            mainModules = new Dictionary<ParticleSystem, ParticleSystem.MainModule>();
            emissionModules = new Dictionary<ParticleSystem, ParticleSystem.EmissionModule>();
            
            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    var emission = ps.emission;
                    mainModules[ps] = main;
                    emissionModules[ps] = emission;
                }
            }

            // Setup soft particles
            if (enableSoftParticles)
            {
                SetupSoftParticles();
            }

            // Setup GPU instancing
            if (enableGPUInstancing)
            {
                SetupGPUInstancing();
            }

            // Load preset
            if (presets.Length > 0 && currentPresetIndex < presets.Length)
            {
                LoadPreset(currentPresetIndex);
            }
        }

        void LateUpdate()
        {
            frameCounter++;

            // Update dynamic lights
            if (enableParticleLights && frameCounter % lightUpdateFrequency == 0)
            {
                UpdateVFXLights();
            }

            // Weather integration
            if (enableWeatherIntegration)
            {
                UpdateWeatherEffects();
            }

            // Auto quality adjustment
            if (autoQualityAdjustment)
            {
                AdjustQualityBasedOnFPS();
            }

            // Collision handling
            if (enableParticleCollision)
            {
                HandleParticleCollisions();
            }

            if (showDebugInfo)
            {
                DebugVFXInfo();
            }
        }

        void SetupSoftParticles()
        {
            // Enable depth texture for soft particles
            if (mainCamera != null)
            {
                mainCamera.depthTextureMode |= DepthTextureMode.Depth;
            }

            // Configure particle materials
            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        // Set soft particle properties via material
                        Material[] materials = renderer.sharedMaterials;
                        foreach (var mat in materials)
                        {
                            if (mat != null)
                            {
                                mat.SetFloat("_SoftParticleFade", softParticleFadeDistance);
                                mat.SetFloat("_CameraFade", cameraNearFade);
                            }
                        }
                    }
                }
            }
        }

        void SetupGPUInstancing()
        {
            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.enableGPUInstancing = true;
                        
                        // Set batch limits
                        var main = ps.main;
                        main.maxParticles = Mathf.Min(maxParticlesPerBatch, main.maxParticles);
                    }
                }
            }
        }

        void UpdateVFXLights()
        {
            // Limit number of active lights
            int lightCount = 0;
            
            foreach (var preset in presets)
            {
                if (preset.vfxLights != null)
                {
                    foreach (var light in preset.vfxLights)
                    {
                        if (light != null && lightCount < maxDynamicLights)
                        {
                            // Adjust light intensity based on particle intensity
                            float intensity = preset.intensity * lightIntensityMultiplier;
                            
                            // Distance-based attenuation
                            if (mainCamera != null)
                            {
                                float distance = Vector3.Distance(light.transform.position, mainCamera.transform.position);
                                float attenuation = Mathf.Clamp01(1f - distance / 50f);
                                light.intensity = intensity * attenuation;
                            }
                            
                            lightCount++;
                        }
                    }
                }
            }
        }

        void UpdateWeatherEffects()
        {
            // Rain
            if (rainParticles != null)
            {
                var emission = rainParticles.emission;
                emission.rateOverTime = Mathf.Lerp(0, 1000, weatherIntensity);
                rainParticles.gameObject.SetActive(weatherIntensity > 0.01f);
            }

            // Snow
            if (snowParticles != null)
            {
                var emission = snowParticles.emission;
                emission.rateOverTime = Mathf.Lerp(0, 500, weatherIntensity);
                snowParticles.gameObject.SetActive(weatherIntensity > 0.01f);
            }

            // Fog
            if (fogVolume != null)
            {
                fogVolume.SetActive(weatherIntensity > 0.1f);
                
                // Adjust fog density
                RenderSettings.fogDensity = Mathf.Lerp(0.01f, 0.1f, weatherIntensity);
                RenderSettings.fogStartDistance = Mathf.Lerp(10f, 50f, weatherIntensity);
                RenderSettings.fogEndDistance = Mathf.Lerp(100f, 500f, weatherIntensity);
            }

            // Wind influence
            if (windZone != null)
            {
                windZone.windMain = Mathf.Lerp(0, 1, weatherIntensity);
                windZone.windPulseMagnitude = Mathf.Lerp(0, 0.5f, weatherIntensity);
            }
        }

        void HandleParticleCollisions()
        {
            foreach (var ps in allParticleSystems)
            {
                if (ps != null && ps.collision.enabled)
                {
                    // Check for collisions and spawn impact particles
                }
            }
        }

        void AdjustQualityBasedOnFPS()
        {
            float currentFPS = 1f / Time.deltaTime;
            
            if (currentFPS < targetFPS * 0.8f)
            {
                ReduceParticleQuality(0.8f);
            }
            else if (currentFPS > targetFPS * 1.1f)
            {
                IncreaseParticleQuality(1.2f);
            }
        }

        void ReduceParticleQuality(float factor)
        {
            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    int newMaxParticles = Mathf.Max(minParticleCount, (int)(main.maxParticles * factor));
                    main.maxParticles = newMaxParticles;
                }
            }
        }

        void IncreaseParticleQuality(float factor)
        {
            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    int newMaxParticles = Mathf.Min(maxParticleCount, (int)(main.maxParticles * factor));
                    main.maxParticles = newMaxParticles;
                }
            }
        }

        public void LoadPreset(int index)
        {
            if (index < 0 || index >= presets.Length)
                return;

            currentPresetIndex = index;
            VFXPreset preset = presets[index];

            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    ps.Stop();
                }
            }

            if (preset.particleSystems != null)
            {
                foreach (var ps in preset.particleSystems)
                {
                    if (ps != null)
                    {
                        ps.Play();
                        
                        var main = ps.main;
                        main.startSpeed = preset.intensity * 10f;
                    }
                }
            }

            foreach (var presetObj in presets)
            {
                if (presetObj.vfxLights != null)
                {
                    foreach (var light in presetObj.vfxLights)
                    {
                        if (light != null)
                        {
                            light.enabled = (presetObj == preset);
                        }
                    }
                }
            }
        }

        public void SetWeatherIntensity(float intensity)
        {
            weatherIntensity = Mathf.Clamp01(intensity);
        }

        public void PlayEffect(string effectName)
        {
            foreach (var preset in presets)
            {
                if (preset.presetName == effectName && preset.particleSystems != null)
                {
                    foreach (var ps in preset.particleSystems)
                    {
                        if (ps != null)
                        {
                            ps.Play();
                        }
                    }
                    break;
                }
            }
        }

        public void StopEffect(string effectName)
        {
            foreach (var preset in presets)
            {
                if (preset.presetName == effectName && preset.particleSystems != null)
                {
                    foreach (var ps in preset.particleSystems)
                    {
                        if (ps != null)
                        {
                            ps.Stop();
                        }
                    }
                    break;
                }
            }
        }

        void DebugVFXInfo()
        {
            int totalParticles = 0;
            int activeSystems = 0;
            
            foreach (var ps in allParticleSystems)
            {
                if (ps != null && ps.isPlaying)
                {
                    totalParticles += ps.particleCount;
                    activeSystems++;
                }
            }

            string debugText = $"Active VFX Systems: {activeSystems}\n";
            debugText += $"Total Particles: {totalParticles}\n";
            debugText += $"Weather Intensity: {weatherIntensity:P0}\n";
            debugText += $"FPS: {1f / Time.deltaTime:F1}";
        }

        void OnDrawGizmosSelected()
        {
            if (!visualizeParticleBounds)
                return;

            foreach (var ps in allParticleSystems)
            {
                if (ps != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(ps.transform.position, 5f);
                }
            }
        }

        private static AdvancedVFXManager _instance;
        public static AdvancedVFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AdvancedVFXManager>();
                }
                return _instance;
            }
        }
    }
}
