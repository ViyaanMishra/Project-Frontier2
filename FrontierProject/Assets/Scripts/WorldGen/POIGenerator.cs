using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Generates 40+ Points of Interest (POIs) with loot tables and faction control.
    /// </summary>
    public static class POIGenerator
    {
        public enum POIType
        {
            GasStation, MilitaryCheckpoint, CrashedSpacecraft, BuriedFactory, RefugeeCamp,
            BanditFortress, AnomalyShrine, TradingPost, ResearchStation, CollapsedHighway,
            FloodedSubway, IrradiatedHospital, BunkerEntrance, PowerPlant, WaterTreatment,
            CommunicationsTower, WarehouseComplex, Scrapyard, Farmstead, MiningOutpost,
            Lighthouse, BridgeRuins, TunnelEntrance, CaveSystem, AncientMonument,
            CrashSite, SupplyDrop, HiddenCache, OutpostAlpha, OutpostBeta, OutpostGamma,
            RuinedCity, OvergrownPark, FrozenLake, VolcanicVent, MagneticAnomaly
        }

        [System.Serializable]
        public struct POIDefinition
        {
            public POIType type;
            public string prefabName;
            public int minCount;
            public int maxCount;
            public BiomeType[] allowedBiomes;
            public LootTable lootTable;
            public bool factionControlled;
        }

        [System.Serializable]
        public struct LootTable
        {
            public int[] itemIds;
            public float[] dropChances;
            public int minStacks;
            public int maxStacks;
        }

        private static readonly POIDefinition[] POIDefinitions = new POIDefinition[]
        {
            new POIDefinition { type = POIType.GasStation, prefabName = "GasStation", minCount = 5, maxCount = 12, allowedBiomes = new[] { BiomeType.Plains, BiomeType.Desert, BiomeType.Forest }, lootTable = CreateLoot(new[] { 101, 102, 201 }, new[] { 0.8f, 0.6f, 0.3f }), factionControlled = false },
            new POIDefinition { type = POIType.MilitaryCheckpoint, prefabName = "MilCheckpoint", minCount = 3, maxCount = 8, allowedBiomes = new[] { BiomeType.Plains, BiomeType.Desert }, lootTable = CreateLoot(new[] { 301, 302, 401 }, new[] { 0.5f, 0.4f, 0.2f }), factionControlled = true },
            new POIDefinition { type = POIType.CrashedSpacecraft, prefabName = "CrashSite_Large", minCount = 1, maxCount = 3, allowedBiomes = new[] { BiomeType.Plains, BiomeType.Tundra, BiomeType.Desert }, lootTable = CreateLoot(new[] { 501, 502, 601 }, new[] { 0.9f, 0.7f, 0.5f }), factionControlled = false },
            new POIDefinition { type = POIType.BuriedFactory, prefabName = "Factory_Ruins", minCount = 2, maxCount = 5, allowedBiomes = new[] { BiomeType.Forest, BiomeType.Plains }, lootTable = CreateLoot(new[] { 101, 201, 301 }, new[] { 0.7f, 0.5f, 0.4f }), factionControlled = true },
            new POIDefinition { type = POIType.RefugeeCamp, prefabName = "RefugeeTents", minCount = 4, maxCount = 10, allowedBiomes = new[] { BiomeType.Plains, BiomeType.Forest, BiomeType.Tundra }, lootTable = CreateLoot(new[] { 701, 702, 801 }, new[] { 0.6f, 0.5f, 0.3f }), factionControlled = false },
            new POIDefinition { type = POIType.BanditFortress, prefabName = "BanditBase", minCount = 1, maxCount = 4, allowedBiomes = new[] { BiomeType.Desert, BiomeType.Wasteland }, lootTable = CreateLoot(new[] { 301, 302, 303 }, new[] { 0.8f, 0.6f, 0.4f }), factionControlled = true },
            new POIDefinition { type = POIType.AnomalyShrine, prefabName = "Anomaly_Altar", minCount = 1, maxCount = 3, allowedBiomes = new[] { BiomeType.Jungle, BiomeType.Tundra, BiomeType.Alpine }, lootTable = CreateLoot(new[] { 901, 902 }, new[] { 0.3f, 0.2f }), factionControlled = false },
            new POIDefinition { type = POIType.TradingPost, prefabName = "TradingHub", minCount = 3, maxCount = 6, allowedBiomes = new[] { BiomeType.Plains, BiomeType.Forest }, lootTable = CreateLoot(new[] { 101, 201, 701 }, new[] { 0.9f, 0.8f, 0.7f }), factionControlled = true },
            new POIDefinition { type = POIType.ResearchStation, prefabName = "Lab_Complex", minCount = 2, maxCount = 4, allowedBiomes = new[] { BiomeType.Alpine, BiomeType.Tundra }, lootTable = CreateLoot(new[] { 501, 601, 901 }, new[] { 0.6f, 0.5f, 0.4f }), factionControlled = true },
            new POIDefinition { type = POIType.IrradiatedHospital, prefabName = "Hospital_Ruins", minCount = 1, maxCount = 3, allowedBiomes = new[] { BiomeType.Plains, BiomeType.Forest }, lootTable = CreateLoot(new[] { 801, 802, 803 }, new[] { 0.7f, 0.5f, 0.3f }), factionControlled = false }
        };

        public struct POISpawnData
        {
            public POIType type;
            public string prefabName;
            public Vector3 position;
            public Quaternion rotation;
            public int factionId; // -1 if uncontrolled
            public LootTable lootTable;
            public bool hasLoot;
        }

        private static LootTable CreateLoot(int[] items, float[] chances)
        {
            return new LootTable
            {
                itemIds = items,
                dropChances = chances,
                minStacks = 1,
                maxStacks = 3
            };
        }

        public static List<POISpawnData> GenerateAllPOIs(int seed, byte[,] biomeMap)
        {
            var pois = new List<POISpawnData>();
            System.Random rand = new System.Random(seed);

            foreach (var def in POIDefinitions)
            {
                int count = rand.Next(def.minCount, def.maxCount + 1);

                for (int i = 0; i < count; i++)
                {
                    int attempts = 0;
                    while (attempts < 100)
                    {
                        int x = rand.Next(0, WorldGenerator.WorldSize);
                        int z = rand.Next(0, WorldGenerator.WorldSize);
                        
                        BiomeType biome = (BiomeType)biomeMap[z % 32, x % 32];
                        
                        if (IsBiomeAllowed(biome, def.allowedBiomes))
                        {
                            var spawn = new POISpawnData
                            {
                                type = def.type,
                                prefabName = def.prefabName,
                                position = new Vector3(x, 0, z),
                                rotation = Quaternion.Euler(0, rand.Next(0, 360), 0),
                                factionId = def.factionControlled ? rand.Next(1, 6) : -1,
                                lootTable = def.lootTable,
                                hasLoot = true
                            };
                            pois.Add(spawn);
                            break;
                        }
                        attempts++;
                    }
                }
            }

            return pois;
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
