using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Seed-based deterministic world generation controller.
    /// Generates terrain, biomes, POIs, and entities from a single seed.
    /// </summary>
    public static class WorldGenerator
    {
        public const int WorldSize = 512;
        public const int ChunkSize = 32;
        public const int ChunksPerSide = WorldSize / ChunkSize;

        private static int _seed;
        private static FastNoiseLite _noise;

        public static void Initialize(int seed)
        {
            _seed = seed;
            _noise = new FastNoiseLite(seed);
        }

        public static float GetHeight(float x, float z)
        {
            float baseHeight = _noise.GetNoise(x * 0.01f, z * 0.01f) * 50f;
            float detail = _noise.GetNoise(x * 0.05f, z * 0.05f) * 10f;
            float roughness = _noise.GetNoise(x * 0.2f, z * 0.2f) * 2f;
            return math.max(0, baseHeight + detail + roughness);
        }

        public static BiomeType GetBiome(float x, float z, float height)
        {
            float temp = _noise.GetNoise(x * 0.005f + 1000f, z * 0.005f + 1000f);
            float moisture = _noise.GetNoise(x * 0.005f + 2000f, z * 0.005f + 2000f);

            if (height < 5f) return BiomeType.Coastal;
            if (height > 40f && temp < -0.3f) return BiomeType.Alpine;
            if (temp < -0.1f) return BiomeType.Tundra;
            if (moisture < -0.3f) return BiomeType.Desert;
            if (moisture > 0.3f && temp > 0.2f) return BiomeType.Jungle;
            if (temp > 0.1f) return BiomeType.Plains;
            return BiomeType.Forest;
        }

        public static NativeArray<byte> GenerateBiomeMap()
        {
            var map = new NativeArray<byte>(WorldSize * WorldSize, Allocator.Persistent);
            for (int z = 0; z < WorldSize; z++)
            {
                for (int x = 0; x < WorldSize; x++)
                {
                    float height = GetHeight(x, z);
                    BiomeType biome = GetBiome(x, z, height);
                    map[z * WorldSize + x] = (byte)biome;
                }
            }
            return map;
        }

        public static int GetPOIIndex(float x, float z, int poiCount)
        {
            float noiseVal = _noise.GetNoise(x * 0.002f + 5000f, z * 0.002f + 5000f);
            if (noiseVal > 0.7f)
            {
                return (int)(math.abs(noiseVal * 1000f) % poiCount);
            }
            return -1;
        }
    }

    public enum BiomeType : byte
    {
        Plains, Forest, Desert, Tundra, Jungle, Alpine, Coastal
    }

    // Simple noise implementation for standalone use
    public class FastNoiseLite
    {
        private int _seed;
        private System.Random _rand;

        public FastNoiseLite(int seed)
        {
            _seed = seed;
            _rand = new System.Random(seed);
        }

        public float GetNoise(float x, float y)
        {
            int xi = (int)math.floor(x);
            int yi = (int)math.floor(y);
            float xf = x - xi;
            float yf = y - yi;

            float tl = Hash(xi, yi);
            float tr = Hash(xi + 1, yi);
            float bl = Hash(xi, yi + 1);
            float br = Hash(xi + 1, yi + 1);

            float u = SmoothStep(xf);
            float v = SmoothStep(yf);

            return math.lerp(math.lerp(tl, tr, u), math.lerp(bl, br, u), v);
        }

        private float Hash(int x, int y)
        {
            int h = _seed + x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0x7FFFFFFF) / (float)0x7FFFFFFF * 2f - 1f;
        }

        private float SmoothStep(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
    }
}
