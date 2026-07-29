using UnityEngine;

namespace Frontier.MeshGen.TexGen
{
    /// <summary>
    /// Procedural texture generator using noise algorithms
    /// </summary>
    public static class NoiseTexture
    {
        private static int[] permutation;
        
        /// <summary>
        /// Generate Perlin noise texture
        /// </summary>
        public static Texture2D GeneratePerlin(int width, int height, float scale = 1f, Color gradientStart = default, Color gradientEnd = default, int seed = 0)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            
            if (gradientStart == default) gradientStart = new Color(0.8f, 0.75f, 0.7f);
            if (gradientEnd == default) gradientEnd = new Color(0.4f, 0.35f, 0.3f);
            
            float offsetX = seed * 100f;
            float offsetY = seed * 100f + 5000f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sampleX = (x + offsetX) / width * scale;
                    float sampleY = (y + offsetY) / height * scale;
                    float noise = Mathf.PerlinNoise(sampleX, sampleY);
                    
                    Color color = Color.Lerp(gradientStart, gradientEnd, noise);
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate Simplex noise texture (better for 3D)
        /// </summary>
        public static Texture2D GenerateSimplex(int width, int height, float scale = 1f, int octaves = 4, float persistence = 0.5f, int seed = 0)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Bilinear;
            
            System.Random rng = new System.Random(seed);
            float offsetX = rng.Next(0, 10000);
            float offsetY = rng.Next(0, 10000);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = 0f;
                    float amplitude = 1f;
                    float frequency = 1f;
                    float maxValue = 0f;
                    
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x + offsetX) / width * scale * frequency;
                        float sampleY = (y + offsetY) / height * scale * frequency;
                        
                        noise += SimplexNoise.Noise2D(sampleX, sampleY) * amplitude;
                        maxValue += amplitude;
                        amplitude *= persistence;
                        frequency *= 2f;
                    }
                    
                    noise = noise / maxValue * 0.5f + 0.5f;
                    float gray = Mathf.Clamp01(noise);
                    tex.SetPixel(x, y, new Color(gray, gray, gray));
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate Worley/Voronoi noise for cellular patterns
        /// </summary>
        public static Texture2D GenerateWorley(int width, int height, int cellCount = 10, int seed = 0)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            
            System.Random rng = new System.Random(seed);
            Vector2[] points = new Vector2[cellCount];
            
            for (int i = 0; i < cellCount; i++)
            {
                points[i] = new Vector2((float)rng.NextDouble() * width, (float)rng.NextDouble() * height);
            }
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float minDist = float.MaxValue;
                    
                    foreach (var point in points)
                    {
                        float dist = Vector2.Distance(pos, point);
                        if (dist < minDist) minDist = dist;
                    }
                    
                    float value = Mathf.InverseLerp(0, width / cellCount * 1.5f, minDist);
                    tex.SetPixel(x, y, new Color(value, value, value));
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate marble pattern using sine-distorted Perlin
        /// </summary>
        public static Texture2D GenerateMarble(int width, int height, float scale = 1f, Color baseColor = default, Color veinColor = default, int seed = 0)
        {
            if (baseColor == default) baseColor = new Color(0.9f, 0.85f, 0.8f);
            if (veinColor == default) veinColor = new Color(0.6f, 0.55f, 0.5f);
            
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            
            float offsetX = seed * 100f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + offsetX) / width * scale;
                    float ny = y / height * scale;
                    
                    float noise = Mathf.PerlinNoise(nx, ny);
                    float marble = Mathf.Sin(ny * Mathf.PI * 4f + noise * 2f) * 0.5f + 0.5f;
                    
                    Color color = Color.Lerp(baseColor, veinColor, marble);
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate wood grain pattern
        /// </summary>
        public static Texture2D GenerateWood(int width, int height, float scale = 1f, Color lightWood = default, Color darkWood = default, int seed = 0)
        {
            if (lightWood == default) lightWood = new Color(0.8f, 0.65f, 0.4f);
            if (darkWood == default) darkWood = new Color(0.5f, 0.35f, 0.2f);
            
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            
            float offsetX = seed * 100f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + offsetX) / width * scale;
                    float ny = y / height * scale;
                    
                    float noise = Mathf.PerlinNoise(nx * 2f, ny * 2f);
                    float grain = Mathf.Sin(ny * Mathf.PI * 8f + noise * 3f) * 0.5f + 0.5f;
                    
                    Color color = Color.Lerp(lightWood, darkWood, grain);
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate metal surface with scratches
        /// </summary>
        public static Texture2D GenerateMetal(int width, int height, Color baseColor = default, float scratchDensity = 0.1f, int seed = 0)
        {
            if (baseColor == default) baseColor = new Color(0.7f, 0.7f, 0.75f);
            
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            
            System.Random rng = new System.Random(seed);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x / 50f + seed, y / 50f) * 0.1f;
                    float value = 1f + noise;
                    
                    // Add scratches
                    if (rng.NextDouble() < scratchDensity / (width * height))
                    {
                        int scratchLength = rng.Next(5, 20);
                        for (int i = 0; i < scratchLength && x + i < width; i++)
                        {
                            value += 0.3f;
                        }
                    }
                    
                    Color color = baseColor * value;
                    tex.SetPixel(x, y, color);
                }
            }
            
            tex.Apply();
            return tex;
        }
    }
    
    /// <summary>
    /// Simplex noise implementation
    /// </summary>
    public static class SimplexNoise
    {
        private static int[] perm = new int[512];
        private static double F2, G2;
        
        static SimplexNoise()
        {
            F2 = 0.5 * (Mathf.Sqrt(3) - 1);
            G2 = (3 - Mathf.Sqrt(3)) / 6;
            
            System.Random rng = new System.Random();
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;
            
            for (int i = 255; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int temp = p[i];
                p[i] = p[j];
                p[j] = temp;
            }
            
            for (int i = 0; i < 512; i++)
                perm[i] = perm[i % 256] = p[i % 256];
        }
        
        public static float Noise2D(float xin, float yin)
        {
            int n0, n1, n2;
            
            float s = (xin + yin) * F2;
            int i = Mathf.FloorToInt(xin + s);
            int j = Mathf.FloorToInt(yin + s);
            
            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;
            float x0 = xin - X0;
            float y0 = yin - Y0;
            
            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }
            
            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1f + 2f * G2;
            float y2 = y0 - 1f + 2f * G2;
            
            int ii = i & 255;
            int jj = j & 255;
            
            float n = 0;
            
            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 >= 0)
            {
                int gi0 = perm[ii + perm[jj]] % 12;
                t0 *= t0;
                n += t0 * t0 * Grad(gi0, x0, y0);
            }
            
            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 >= 0)
            {
                int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
                t1 *= t1;
                n += t1 * t1 * Grad(gi1, x1, y1);
            }
            
            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 >= 0)
            {
                int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
                t2 *= t2;
                n += t2 * t2 * Grad(gi2, x2, y2);
            }
            
            return n * 70f;
        }
        
        private static float Grad(int hash, float x, float y)
        {
            int h = hash & 7;
            float u = h < 4 ? x : y;
            float v = h < 4 ? y : x;
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2f * v : 2f * v);
        }
    }
}
