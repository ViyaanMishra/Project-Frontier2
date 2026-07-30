using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Frontier.Graphics
{
    /// <summary>
    /// Master controller for dynamic post-processing volumes based on game state.
    /// Automatically adjusts bloom, exposure, and color grading for biomes, weather, and time of day.
    /// </summary>
    public class DynamicPostProcessManager : MonoBehaviour
    {
        [Header("References")]
        public Volume globalVolume;
        public VolumeProfile baseProfile;
        
        [Header("Dynamic Overrides")]
        public bool enableDynamicExposure = true;
        public bool enableBiomeGrading = true;
        public bool enableWeatherEffects = true;

        private VolumeProfile activeProfile;
        private ColorAdjustment colorAdjustment;
        private Bloom bloom;
        private Vignette vignette;
        private FilmGrain filmGrain;
        private ChromaticAberration chromaAb;
        private DepthOfField dof;

        private void Awake()
        {
            if (globalVolume == null)
                globalVolume = FindObjectOfType<Volume>();

            if (globalVolume != null && baseProfile != null)
            {
                activeProfile = Instantiate(baseProfile);
                globalVolume.profile = activeProfile;
                CacheSettings();
            }
        }

        private void CacheSettings()
        {
            activeProfile.TryGet(out colorAdjustment);
            activeProfile.TryGet(out bloom);
            activeProfile.TryGet(out vignette);
            activeProfile.TryGet(out filmGrain);
            activeProfile.TryGet(out chromaAb);
            activeProfile.TryGet(out dof);
        }

        public void UpdateBiomeContext(string biomeType, float intensity = 1.0f)
        {
            if (!enableBiomeGrading || activeProfile == null) return;

            switch (biomeType.ToLower())
            {
                case "wasteland":
                    SetColorGrading(new Color(1.0f, 0.9f, 0.8f), 0.1f, -0.2f);
                    SetFilmGrain(0.4f);
                    break;
                case "forest":
                    SetColorGrading(new Color(0.95f, 1.0f, 0.95f), 0.15f, 0.1f);
                    SetFilmGrain(0.1f);
                    break;
                case "arctic":
                    SetColorGrading(new Color(0.9f, 0.95f, 1.0f), -0.1f, 0.2f);
                    SetBloom(1.2f, 0.8f);
                    break;
                case "anomaly":
                    SetColorGrading(new Color(1.2f, 0.8f, 1.2f), 0.5f, -0.5f);
                    SetChromaticAberration(0.4f);
                    SetBloom(2.0f, 1.0f);
                    break;
            }
        }

        public void UpdateWeatherContext(string weatherType, float intensity = 1.0f)
        {
            if (!enableWeatherEffects || activeProfile == null) return;

            switch (weatherType.ToLower())
            {
                case "rain":
                case "storm":
                    SetVignette(0.4f, 0.6f);
                    SetColorGrading(Color.white, -0.3f, 0.0f);
                    SetFilmGrain(0.3f * intensity);
                    break;
                case "fog":
                    SetVignette(0.2f, 0.4f);
                    SetColorGrading(Color.white, -0.4f, 0.1f);
                    break;
                case "clear":
                    SetVignette(0.15f, 0.3f);
                    SetBloom(0.8f, 0.5f);
                    break;
            }
        }

        public void UpdateTimeOfDayContext(float hour, float sunriseStart, float sunsetEnd)
        {
            if (!enableDynamicExposure || activeProfile == null) return;

            float targetExposure = 0.0f;
            Color targetTone = Color.white;

            if (hour >= sunriseStart && hour <= sunsetEnd)
            {
                targetExposure = 0.0f;
                targetTone = new Color(1.0f, 0.98f, 0.95f);
            }
            else if (hour < sunriseStart + 1.5f || hour > sunsetEnd - 1.5f)
            {
                targetExposure = -0.5f;
                targetTone = new Color(1.0f, 0.8f, 0.6f);
                SetBloom(1.5f, 0.8f);
            }
            else
            {
                targetExposure = -1.5f;
                targetTone = new Color(0.8f, 0.85f, 1.0f);
                SetBloom(1.2f, 0.9f);
            }

            if (colorAdjustment != null)
            {
                colorAdjustment.colorFilter.overrideState = true;
                colorAdjustment.colorFilter.value = targetTone;
            }
        }

        #region Helpers
        private void SetColorGrading(Color filter, float saturation, float contrast)
        {
            if (colorAdjustment == null) return;
            colorAdjustment.colorFilter.overrideState = true;
            colorAdjustment.colorFilter.value = filter;
            colorAdjustment.saturation.overrideState = true;
            colorAdjustment.saturation.value = saturation * 100f;
            colorAdjustment.contrast.overrideState = true;
            colorAdjustment.contrast.value = contrast * 100f;
        }

        private void SetBloom(float intensity, float threshold)
        {
            if (bloom == null) return;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = intensity;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = threshold;
        }

        private void SetVignette(float intensity, float smoothness)
        {
            if (vignette == null) return;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = intensity;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = smoothness;
        }

        private void SetFilmGrain(float intensity)
        {
            if (filmGrain == null) return;
            filmGrain.intensity.overrideState = true;
            filmGrain.intensity.value = intensity;
        }

        private void SetChromaticAberration(float intensity)
        {
            if (chromaAb == null) return;
            chromaAb.intensity.overrideState = true;
            chromaAb.intensity.value = intensity;
        }
        #endregion
    }
}
