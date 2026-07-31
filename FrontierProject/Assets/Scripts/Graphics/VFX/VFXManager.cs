using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Frontier.Graphics.VFX
{
    /// <summary>
    /// Advanced VFX Manager for AAA-quality particle effects and visual enhancements.
    /// Handles volumetric particles, dynamic lighting integration, and post-processing effects.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("Volumetric Particle Systems")]
        public ParticleSystem[] volumetricParticles;
        public Material volumetricParticleMaterial;
        
        [Header("Dynamic Lighting")]
        public Light[] dynamicLights;
        public float lightFlickerIntensity = 0.1f;
        public float lightFlickerSpeed = 2f;
        
        [Header("Screen Space Effects")]
        public bool enableSSR = true;
        public bool enableSSAO = true;
        public bool enableMotionBlur = true;
        
        [Header("Atmospheric Effects")]
        public float godRaysIntensity = 0.5f;
        public float atmosphericScattering = 0.3f;
        public Color atmosphericTint = new Color(0.8f, 0.85f, 0.9f);
        
        [Header("Weather VFX")]
        public ParticleSystem rainSystem;
        public ParticleSystem snowSystem;
        public ParticleSystem dustStormSystem;
        public float weatherIntensity = 1f;
        
        [Header("Combat VFX")]
        public ParticleSystem muzzleFlash;
        public ParticleSystem impactSparks;
        public ParticleSystem smokeTrails;
        
        [Header("Environmental VFX")]
        public ParticleSystem floatingDebris;
        public ParticleSystem insects;
        public ParticleSystem leaves;
        
        private Volume globalVolume;
        private Vignette vignette;
        private ChromaticAberration chromaAb;
        private FilmGrain filmGrain;
        private Bloom bloom;
        
        private float timeScale = 1f;
        
        private void Awake()
        {
            InitializePostProcessing();
            SetupParticleSystems();
        }
        
        private void InitializePostProcessing()
        {
            globalVolume = FindObjectOfType<Volume>();
            if (globalVolume != null && globalVolume.profile != null)
            {
                globalVolume.profile.TryGet(out vignette);
                globalVolume.profile.TryGet(out chromaAb);
                globalVolume.profile.TryGet(out filmGrain);
                globalVolume.profile.TryGet(out bloom);
            }
        }
        
        private void SetupParticleSystems()
        {
            // Configure all particle systems for AAA quality
            foreach (var ps in volumetricParticles)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    main.maxParticles = 10000;
                    
                    var emission = ps.emission;
                    emission.rateOverTimeMultiplier *= 2f;
                    
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null && volumetricParticleMaterial != null)
                    {
                        renderer.material = volumetricParticleMaterial;
                    }
                }
            }
        }
        
        private void Update()
        {
            UpdateDynamicLights();
            UpdateAtmosphericEffects();
            UpdateWeatherVFX();
            UpdateScreenSpaceEffects();
        }
        
        private void UpdateDynamicLights()
        {
            float flicker = Mathf.PerlinNoise(Time.time * lightFlickerSpeed, 0) * lightFlickerIntensity;
            
            foreach (var light in dynamicLights)
            {
                if (light != null)
                {
                    light.intensity = Mathf.Lerp(light.intensity, light.intensity * (1 + flicker), Time.deltaTime * 5f);
                }
            }
        }
        
        private void UpdateAtmosphericEffects()
        {
            // Dynamic vignette based on player health/stamina
            float healthVignette = Mathf.Lerp(0.2f, 0.6f, 1 - (PlayerHealth.current / PlayerHealth.max));
            if (vignette != null)
            {
                vignette.intensity.overrideState = true;
                vignette.intensity.value = healthVignette;
            }
            
            // Chromatic aberration for damage/anomaly effects
            if (chromaAb != null)
            {
                chromaAb.intensity.overrideState = true;
                chromaAb.intensity.value = IsInAnomalyZone() ? 0.4f : 0.05f;
            }
        }
        
        private void UpdateWeatherVFX()
        {
            if (rainSystem != null)
            {
                var emission = rainSystem.emission;
                emission.rateOverTimeMultiplier = Mathf.Lerp(0, 1000, weatherIntensity);
            }
            
            if (snowSystem != null)
            {
                var emission = snowSystem.emission;
                emission.rateOverTimeMultiplier = Mathf.Lerp(0, 500, weatherIntensity);
            }
            
            if (dustStormSystem != null)
            {
                var emission = dustStormSystem.emission;
                emission.rateOverTimeMultiplier = Mathf.Lerp(0, 800, weatherIntensity);
            }
        }
        
        private void UpdateScreenSpaceEffects()
        {
            // Motion blur based on velocity
            if (enableMotionBlur)
            {
                float velocity = GetPlayerVelocity();
                if (bloom != null)
                {
                    bloom.intensity.overrideState = true;
                    bloom.intensity.value = Mathf.Lerp(0.5f, 1.5f, Mathf.Clamp01(velocity / 50f));
                }
            }
        }
        
        #region Public API
        
        public void TriggerExplosionVFX(Vector3 position, float magnitude)
        {
            // Spawn explosion particles
            SpawnParticleAtPosition(impactSparks, position);
            
            // Dynamic light flash
            CreateTemporaryLight(position, Color.orange, magnitude * 2f);
            
            // Screen shake
            CameraShake.Instance.Shake(magnitude * 0.5f, 0.5f);
            
            // Post-process flash
            StartCoroutine(FlashEffect(magnitude));
        }
        
        public void EnableGodRays(bool enabled)
        {
            godRaysIntensity = enabled ? 0.8f : 0f;
            UpdateVolumetricLighting();
        }
        
        public void SetWeatherType(string weatherType, float intensity)
        {
            weatherIntensity = Mathf.Clamp01(intensity);
            
            rainSystem?.gameObject.SetActive(weatherType == "rain");
            snowSystem?.gameObject.SetActive(weatherType == "snow");
            dustStormSystem?.gameObject.SetActive(weatherType == "storm");
        }
        
        public void PlayMuzzleFlash(Transform muzzlePoint)
        {
            if (muzzleFlash != null)
            {
                var ps = Instantiate(muzzleFlash, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(ps.gameObject, 2f);
            }
        }
        
        #endregion
        
        #region Helpers
        
        private void SpawnParticleAtPosition(ParticleSystem ps, Vector3 position)
        {
            if (ps != null)
            {
                var instance = Instantiate(ps, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, 5f);
            }
        }
        
        private void CreateTemporaryLight(Vector3 position, Color color, float intensity)
        {
            GameObject lightObj = new GameObject("TempLight");
            lightObj.transform.position = position;
            Light light = lightObj.AddComponent<Light>();
            light.color = color;
            light.intensity = intensity;
            light.range = 20f;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            
            Destroy(lightObj, 0.5f);
        }
        
        private System.Collections.IEnumerator FlashEffect(float magnitude)
        {
            float duration = 0.3f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                if (filmGrain != null)
                {
                    filmGrain.intensity.overrideState = true;
                    filmGrain.intensity.value = Mathf.Lerp(magnitude, 0, t);
                }
                
                yield return null;
            }
        }
        
        private float GetPlayerVelocity()
        {
            // Placeholder - integrate with actual player controller
            return 0f;
        }
        
        private bool IsInAnomalyZone()
        {
            // Placeholder - integrate with anomaly detection system
            return false;
        }
        
        private void UpdateVolumetricLighting()
        {
            // Update volumetric fog/shader parameters
            Shader.SetGlobalFloat("_VolumetricIntensity", godRaysIntensity);
            Shader.SetGlobalColor("_AtmosphericTint", atmosphericTint);
        }
        
        #endregion
    }
}
