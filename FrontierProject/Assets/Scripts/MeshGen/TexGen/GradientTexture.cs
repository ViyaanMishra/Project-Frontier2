using UnityEngine;

namespace Frontier.MeshGen.TexGen
{
    /// <summary>
    /// Gradient texture generator for skies, water, glows, and fades
    /// </summary>
    public static class GradientTexture
    {
        /// <summary>
        /// Generate linear gradient texture
        /// </summary>
        public static Texture2D GenerateLinear(int width, int height, Color startColor, Color endColor, bool vertical = true)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            
            for (int y = 0; y < height; y++)
            {
                float t = vertical ? (float)y / height : (float)y / width;
                Color color = Color.Lerp(startColor, endColor, t);
                
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate radial gradient from center
        /// </summary>
        public static Texture2D GenerateRadial(int size, Color centerColor, Color edgeColor, float softness = 1f)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center) / maxDist;
                    dist = Mathf.Pow(dist, softness);
                    
                    Color color = Color.Lerp(centerColor, edgeColor, dist);
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate angular/sweep gradient
        /// </summary>
        public static Texture2D GenerateAngular(int size, Color[] colors, float[] angles = null)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            
            if (angles == null)
            {
                angles = new float[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                    angles[i] = (float)i / colors.Length * 360f;
            }
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 dir = new Vector2(x - center.x, y - center.y);
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;
                    
                    Color color = SampleGradient(angle, colors, angles);
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate sky gradient (horizon to zenith)
        /// </summary>
        public static Texture2D GenerateSky(int width, int height, Color horizonColor, Color zenithColor, Color? horizonGlow = null)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            
            if (!horizonGlow.HasValue)
                horizonGlow = Color.Lerp(horizonColor, zenithColor, 0.5f);
            
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                
                // Non-linear interpolation for more natural sky
                float skyT = t * t * (3f - 2f * t);
                
                Color color;
                if (t < 0.3f)
                {
                    float localT = t / 0.3f;
                    color = Color.Lerp(horizonGlow.Value, horizonColor, localT);
                }
                else
                {
                    float localT = (t - 0.3f) / 0.7f;
                    color = Color.Lerp(horizonColor, zenithColor, localT);
                }
                
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate glow/aura texture for emissive effects
        /// </summary>
        public static Texture2D GenerateGlow(int size, Color glowColor, float intensity = 1f, string pattern = "soft")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center) / maxDist;
                    
                    float alpha;
                    switch (pattern)
                    {
                        case "sharp":
                            alpha = Mathf.Clamp01(1f - dist * 1.5f);
                            break;
                        case "ring":
                            float ringDist = Mathf.Abs(dist - 0.5f) * 2f;
                            alpha = Mathf.Clamp01(1f - ringDist * 3f);
                            break;
                        case "burst":
                            float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);
                            float burst = Mathf.Sin(angle * 8f) * 0.5f + 0.5f;
                            alpha = Mathf.Clamp01((1f - dist) * burst * intensity);
                            break;
                        default: // soft
                            alpha = Mathf.Clamp01(Mathf.Pow(1f - dist, 2f * intensity));
                            break;
                    }
                    
                    tex.SetPixel(x, y, new Color(glowColor.r, glowColor.g, glowColor.b, alpha));
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate fade transition texture
        /// </summary>
        public static Texture2D GenerateFade(int width, int height, float fadePosition = 0.5f, float fadeSoftness = 0.1f, Color fadeColor = default)
        {
            if (fadeColor == default) fadeColor = Color.black;
            
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fadePosition - fadeSoftness, fadePosition + fadeSoftness, t));
                
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        private static Color SampleGradient(float angle, Color[] colors, float[] angles)
        {
            for (int i = 0; i < angles.Length - 1; i++)
            {
                if (angle >= angles[i] && angle < angles[i + 1])
                {
                    float localT = Mathf.InverseLerp(angles[i], angles[i + 1], angle);
                    return Color.Lerp(colors[i], colors[i + 1], localT);
                }
            }
            
            // Wrap around
            float wrapT = Mathf.InverseLerp(angles[angles.Length - 1], 360f + angles[0], angle);
            return Color.Lerp(colors[colors.Length - 1], colors[0], wrapT);
        }
    }
}
