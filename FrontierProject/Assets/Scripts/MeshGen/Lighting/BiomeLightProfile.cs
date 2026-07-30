using UnityEngine;

namespace Frontier.MeshGen.Lighting
{
    /// <summary>
    /// Per-biome lighting and atmosphere profile.
    /// Controls ambient color, fog, sky gradient, and exposure.
    /// </summary>
    [System.Serializable]
    public class BiomeLightProfile
    {
        [Header("Biome Identification")]
        public string biomeName;
        
        [Header("Ambient Lighting")]
        public Color ambientColor = Color.white;
        [Range(0f, 2f)] public float ambientIntensity = 1f;
        public Color hemisphereSkyColor = Color.cyan;
        public Color hemisphereGroundColor = Color.green;
        
        [Header("Fog Settings")]
        public bool enableFog = true;
        public Color fogColor = Color.white;
        [Range(0f, 0.1f)] public float fogDensity = 0.01f;
        public float fogStartDistance = 10f;
        public float fogEndDistance = 500f;
        public FogMode fogMode = FogMode.ExponentialSquared;
        
        [Header("Sky Gradient")]
        public Gradient skyGradient;
        public float skyExposure = 1.3f;
        
        [Header("Exposure")]
        [Range(0f, 4f)] public float exposure = 1f;
        public bool autoExposure = true;
        [Range(0f, 1f)] public float exposureCompensation = 0f;
        
        [Header("Environment Reflections")]
        public Cubemap reflectionCubemap;
        [Range(0f, 1f)] public float reflectionIntensity = 1f;
        
        /// <summary>
        /// Apply this profile to the render settings.
        /// </summary>
        public void Apply()
        {
            RenderSettings.ambientLight = ambientColor * ambientIntensity;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = hemisphereSkyColor;
            RenderSettings.ambientEquatorColor = (hemisphereSkyColor + hemisphereGroundColor) * 0.5f;
            RenderSettings.ambientGroundColor = hemisphereGroundColor;
            
            if (enableFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
                RenderSettings.fogMode = fogMode;
            }
            else
            {
                RenderSettings.fog = false;
            }
            
            // Note: Sky gradient and exposure require URP Volume or post-processing stack
            // These values are stored for reference by those systems
        }
        
        /// <summary>
        /// Create a default profile for a specific biome type.
        /// </summary>
        public static BiomeLightProfile CreateDefault(BiomeType biomeType)
        {
            var profile = new BiomeLightProfile();
            
            switch (biomeType)
            {
                case BiomeType.Forest:
                    profile.biomeName = "Temperate Forest";
                    profile.ambientColor = new Color(0.7f, 0.8f, 0.7f);
                    profile.hemisphereSkyColor = new Color(0.5f, 0.7f, 0.9f);
                    profile.hemisphereGroundColor = new Color(0.2f, 0.3f, 0.1f);
                    profile.fogColor = new Color(0.8f, 0.9f, 0.8f);
                    profile.fogDensity = 0.005f;
                    break;
                    
                case BiomeType.Desert:
                    profile.biomeName = "Arid Desert";
                    profile.ambientColor = new Color(1f, 0.95f, 0.8f);
                    profile.hemisphereSkyColor = new Color(0.9f, 0.8f, 0.6f);
                    profile.hemisphereGroundColor = new Color(0.8f, 0.6f, 0.4f);
                    profile.fogColor = new Color(1f, 0.9f, 0.7f);
                    profile.fogDensity = 0.008f;
                    profile.exposure = 1.5f;
                    break;
                    
                case BiomeType.Tundra:
                    profile.biomeName = "Frozen Tundra";
                    profile.ambientColor = new Color(0.8f, 0.85f, 0.9f);
                    profile.hemisphereSkyColor = new Color(0.6f, 0.7f, 0.85f);
                    profile.hemisphereGroundColor = new Color(0.7f, 0.75f, 0.8f);
                    profile.fogColor = new Color(0.9f, 0.95f, 1f);
                    profile.fogDensity = 0.012f;
                    profile.exposure = 1.2f;
                    break;
                    
                case BiomeType.Wasteland:
                    profile.biomeName = "Radioactive Wasteland";
                    profile.ambientColor = new Color(0.7f, 0.8f, 0.6f);
                    profile.hemisphereSkyColor = new Color(0.6f, 0.7f, 0.5f);
                    profile.hemisphereGroundColor = new Color(0.5f, 0.5f, 0.4f);
                    profile.fogColor = new Color(0.7f, 0.8f, 0.5f);
                    profile.fogDensity = 0.015f;
                    profile.exposure = 0.9f;
                    break;
                    
                case BiomeType.Anomaly:
                    profile.biomeName = "Anomaly Zone";
                    profile.ambientColor = new Color(0.8f, 0.6f, 1f);
                    profile.hemisphereSkyColor = new Color(0.5f, 0.3f, 0.7f);
                    profile.hemisphereGroundColor = new Color(0.4f, 0.2f, 0.5f);
                    profile.fogColor = new Color(0.7f, 0.4f, 0.9f);
                    profile.fogDensity = 0.02f;
                    profile.exposure = 0.8f;
                    break;
                    
                case BiomeType.Coastal:
                    profile.biomeName = "Coastal Region";
                    profile.ambientColor = new Color(0.85f, 0.9f, 0.95f);
                    profile.hemisphereSkyColor = new Color(0.6f, 0.8f, 0.95f);
                    profile.hemisphereGroundColor = new Color(0.7f, 0.75f, 0.7f);
                    profile.fogColor = new Color(0.85f, 0.9f, 0.95f);
                    profile.fogDensity = 0.01f;
                    profile.exposure = 1.3f;
                    break;
            }
            
            return profile;
        }
    }
    
    /// <summary>
    /// Enum of supported biome types.
    /// </summary>
    public enum BiomeType
    {
        Forest,
        Desert,
        Tundra,
        Wasteland,
        Anomaly,
        Coastal
    }
}
