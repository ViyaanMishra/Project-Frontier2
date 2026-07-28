using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Distributes biomes using Perlin/Worley noise blending for smooth transitions.
    /// Handles 6 major biomes with sub-biome and micro-biome variations.
    /// </summary>
    public static class BiomeDistributor
    {
        [System.Serializable]
        public struct BiomeLayer
        {
            public BiomeType biome;
            public float minTemp;
            public float maxTemp;
            public float minMoisture;
            public float maxMoisture;
            public float minHeight;
            public float maxHeight;
        }

        private static readonly BiomeLayer[] BiomeLayers = new BiomeLayer[]
        {
            new BiomeLayer { biome = BiomeType.Plains, minTemp = 0.1f, maxTemp = 0.5f, minMoisture = -0.2f, maxMoisture = 0.3f, minHeight = 5f, maxHeight = 25f },
            new BiomeLayer { biome = BiomeType.Forest, minTemp = 0.2f, maxTemp = 0.6f, minMoisture = 0.2f, maxMoisture = 0.7f, minHeight = 5f, maxHeight = 35f },
            new BiomeLayer { biome = BiomeType.Desert, minTemp = 0.3f, maxTemp = 0.8f, minMoisture = -0.8f, maxMoisture = -0.3f, minHeight = 0f, maxHeight = 30f },
            new BiomeLayer { biome = BiomeType.Tundra, minTemp = -0.5f, maxTemp = -0.1f, minMoisture = -0.5f, maxMoisture = 0.2f, minHeight = 10f, maxHeight = 45f },
            new BiomeLayer { biome = BiomeType.Jungle, minTemp = 0.4f, maxTemp = 0.9f, minMoisture = 0.4f, maxMoisture = 0.9f, minHeight = 0f, maxHeight = 20f },
            new BiomeLayer { biome = BiomeType.Alpine, minTemp = -0.8f, maxTemp = -0.2f, minMoisture = -0.3f, maxMoisture = 0.5f, minHeight = 40f, maxHeight = 100f },
            new BiomeLayer { biome = BiomeType.Coastal, minTemp = -0.2f, maxTemp = 0.7f, minMoisture = -0.5f, maxMoisture = 0.8f, minHeight = 0f, maxHeight = 5f }
        };

        public static BiomeType GetBiomeAt(float x, float z, FastNoiseLite tempNoise, FastNoiseLite moistNoise, FastNoiseLite heightNoise)
        {
            float height = heightNoise.GetNoise(x * 0.01f, z * 0.01f);
            float normalizedHeight = height * 50f;

            float temp = tempNoise.GetNoise(x * 0.005f + 1000f, z * 0.005f + 1000f);
            float moisture = moistNoise.GetNoise(x * 0.005f + 2000f, z * 0.005f + 2000f);

            int bestMatch = -1;
            float bestScore = float.MinValue;

            for (int i = 0; i < BiomeLayers.Length; i++)
            {
                var layer = BiomeLayers[i];
                if (normalizedHeight >= layer.minHeight && normalizedHeight <= layer.maxHeight &&
                    temp >= layer.minTemp && temp <= layer.maxTemp &&
                    moisture >= layer.minMoisture && moisture <= layer.maxMoisture)
                {
                    float score = CalculateBiomeScore(temp, moisture, normalizedHeight, layer);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = i;
                    }
                }
            }

            return bestMatch >= 0 ? BiomeLayers[bestMatch].biome : BiomeType.Plains;
        }

        private static float CalculateBiomeScore(float temp, float moisture, float height, BiomeLayer layer)
        {
            float tempDist = 1f - math.abs((temp - (layer.minTemp + layer.maxTemp) * 0.5f) / ((layer.maxTemp - layer.minTemp) * 0.5f + 0.001f));
            float moistDist = 1f - math.abs((moisture - (layer.minMoisture + layer.maxMoisture) * 0.5f) / ((layer.maxMoisture - layer.minMoisture) * 0.5f + 0.001f));
            float heightDist = 1f - math.abs((height - (layer.minHeight + layer.maxHeight) * 0.5f) / ((layer.maxHeight - layer.minHeight) * 0.5f + 0.001f));
            return tempDist * 0.4f + moistDist * 0.4f + heightDist * 0.2f;
        }

        public static SubBiomeType GetSubBiome(BiomeType mainBiome, float x, float z, FastNoiseLite subNoise)
        {
            float subVal = subNoise.GetNoise(x * 0.02f, z * 0.02f);
            
            switch (mainBiome)
            {
                case BiomeType.Forest:
                    return subVal > 0.3f ? SubBiomeType.DenseForest : subVal < -0.3f ? SubBiomeType.SparseForest : SubBiomeType.MixedForest;
                case BiomeType.Plains:
                    return subVal > 0.2f ? SubBiomeType.Grassland : subVal < -0.2f ? SubBiomeType.Savanna : SubBiomeType.Meadow;
                case BiomeType.Desert:
                    return subVal > 0.4f ? SubBiomeType.Dunes : subVal < -0.4f ? SubBiomeType.RockyDesert : SubBiomeType.FlatDesert;
                default:
                    return SubBiomeType.Standard;
            }
        }
    }

    public enum SubBiomeType : byte
    {
        Standard, DenseForest, SparseForest, MixedForest, Grassland, Savanna, Meadow, Dunes, RockyDesert, FlatDesert
    }
}
