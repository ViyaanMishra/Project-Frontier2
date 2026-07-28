using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Per-biome prop placement system (trees, rocks, wrecks, ruins).
    /// </summary>
    public static class ClutterSpawner
    {
        [System.Serializable]
        public struct ClutterPrefab
        {
            public string prefabName;
            public BiomeType[] allowedBiomes;
            public float minScale;
            public float maxScale;
            public int density; // per chunk
        }

        private static readonly ClutterPrefab[] ClutterPrefabs = new ClutterPrefab[]
        {
            new ClutterPrefab { prefabName = "PineTree", allowedBiomes = new[] { BiomeType.Forest, BiomeType.Tundra, BiomeType.Alpine }, minScale = 0.8f, maxScale = 1.5f, density = 20 },
            new ClutterPrefab { prefabName = "OakTree", allowedBiomes = new[] { BiomeType.Plains, BiomeType.Forest }, minScale = 1f, maxScale = 2f, density = 15 },
            new ClutterPrefab { prefabName = "Cactus", allowedBiomes = new[] { BiomeType.Desert }, minScale = 0.5f, maxScale = 1.2f, density = 25 },
            new ClutterPrefab { prefabName = "PalmTree", allowedBiomes = new[] { BiomeType.Jungle, BiomeType.Coastal }, minScale = 1f, maxScale = 1.8f, density = 18 },
            new ClutterPrefab { prefabName = "RockLarge", allowedBiomes = new[] { BiomeType.Alpine, BiomeType.Desert, BiomeType.Tundra }, minScale = 1f, maxScale = 3f, density = 10 },
            new ClutterPrefab { prefabName = "RockSmall", allowedBiomes = new[] { BiomeType.Plains, BiomeType.Forest, BiomeType.Desert }, minScale = 0.3f, maxScale = 0.8f, density = 30 },
            new ClutterPrefab { prefabName = "VehicleWreck", allowedBiomes = new[] { BiomeType.Plains, BiomeType.Desert, BiomeType.Forest }, minScale = 1f, maxScale = 1f, density = 2 },
            new ClutterPrefab { prefabName = "Ruins", allowedBiomes = new[] { BiomeType.Forest, BiomeType.Jungle, BiomeType.Desert }, minScale = 1f, maxScale = 1f, density = 1 }
        };

        public struct SpawnData
        {
            public string prefabName;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        public static NativeList<SpawnData> GenerateClutterForChunk(int chunkX, int chunkZ, byte[,] biomes, FastNoiseLite clutterNoise)
        {
            var spawns = new NativeList<SpawnData>(Allocator.Temp);

            foreach (var prefab in ClutterPrefabs)
            {
                for (int i = 0; i < prefab.density; i++)
                {
                    float nx = (chunkX * WorldGenerator.ChunkSize + clutterNoise.GetNoise(i * 100f, chunkX)) % WorldGenerator.ChunkSize;
                    float nz = (chunkZ * WorldGenerator.ChunkSize + clutterNoise.GetNoise(i * 200f, chunkZ)) % WorldGenerator.ChunkSize;

                    int bx = (int)nx % 32;
                    int bz = (int)nz % 32;
                    
                    BiomeType biome = (BiomeType)biomes[bz, bx];

                    if (IsBiomeAllowed(biome, prefab.allowedBiomes))
                    {
                        float worldX = chunkX * WorldGenerator.ChunkSize + nx;
                        float worldZ = chunkZ * WorldGenerator.ChunkSize + nz;
                        
                        float scale = math.lerp(prefab.minScale, prefab.maxScale, clutterNoise.GetNoise(worldX * 0.1f, worldZ * 0.1f) * 0.5f + 0.5f);
                        
                        var spawn = new SpawnData
                        {
                            prefabName = prefab.prefabName,
                            position = new Vector3(worldX, 0, worldZ),
                            rotation = Quaternion.Euler(0, clutterNoise.GetNoise(worldX, worldZ) * 360f, 0),
                            scale = new Vector3(scale, scale, scale)
                        };
                        spawns.Add(spawn);
                    }
                }
            }

            return spawns;
        }

        private static bool IsBiomeAllowed(BiomeType biome, BiomeType[] allowed)
        {
            for (int i = 0; i < allowed.Length; i++)
            {
                if (allowed[i] == biome) return true;
            }
            return false;
        }
    }
}
