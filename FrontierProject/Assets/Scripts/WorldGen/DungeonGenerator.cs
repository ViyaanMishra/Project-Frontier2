using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Procedural dungeon/bunker/ruin interior generator.
    /// Creates connected rooms with corridors, loot, and enemies.
    /// </summary>
    public static class DungeonGenerator
    {
        public enum RoomType
        {
            Entrance, Corridor, Storage, LivingQuarters, Laboratory, ControlRoom,
            Armory, MedicalBay, EngineRoom, Vault, SecretChamber, BossArena
        }

        [System.Serializable]
        public struct RoomDefinition
        {
            public RoomType type;
            public int minWidth;
            public int maxWidth;
            public int minHeight;
            public int maxHeight;
            public int minLength;
            public int maxLength;
            public int[] possibleLootItems;
            public float enemySpawnChance;
        }

        public struct GeneratedRoom
        {
            public RoomType type;
            public int x, y, z; // Grid position
            public int width, height, length;
            public List<int> connections; // Indices of connected rooms
            public bool hasLoot;
            public bool hasEnemies;
        }

        public struct DungeonResult
        {
            public List<GeneratedRoom> rooms;
            public Vector3 entrancePosition;
            public Vector3 bossPosition;
            public int totalLootCount;
            public int totalEnemyCount;
        }

        private static readonly RoomDefinition[] RoomDefinitions = new RoomDefinition[]
        {
            new RoomDefinition { type = RoomType.Entrance, minWidth = 3, maxWidth = 5, minHeight = 3, maxHeight = 4, minLength = 3, maxLength = 5, possibleLootItems = new[] { 101 }, enemySpawnChance = 0.2f },
            new RoomDefinition { type = RoomType.Corridor, minWidth = 2, maxWidth = 3, minHeight = 3, maxHeight = 3, minLength = 5, maxLength = 15, possibleLootItems = new int[0], enemySpawnChance = 0.3f },
            new RoomDefinition { type = RoomType.Storage, minWidth = 4, maxWidth = 8, minHeight = 3, maxHeight = 5, minLength = 4, maxLength = 8, possibleLootItems = new[] { 101, 102, 201 }, enemySpawnChance = 0.1f },
            new RoomDefinition { type = RoomType.LivingQuarters, minWidth = 4, maxWidth = 6, minHeight = 3, maxHeight = 4, minLength = 4, maxLength = 6, possibleLootItems = new[] { 701, 702, 801 }, enemySpawnChance = 0.2f },
            new RoomDefinition { type = RoomType.Laboratory, minWidth = 6, maxWidth = 10, minHeight = 4, maxHeight = 6, minLength = 6, maxLength = 10, possibleLootItems = new[] { 501, 601, 901 }, enemySpawnChance = 0.4f },
            new RoomDefinition { type = RoomType.ControlRoom, minWidth = 5, maxWidth = 8, minHeight = 4, maxHeight = 5, minLength = 5, maxLength = 8, possibleLootItems = new[] { 501, 401 }, enemySpawnChance = 0.3f },
            new RoomDefinition { type = RoomType.Armory, minWidth = 5, maxWidth = 8, minHeight = 3, maxHeight = 4, minLength = 5, maxLength = 8, possibleLootItems = new[] { 301, 302, 303, 401 }, enemySpawnChance = 0.5f },
            new RoomDefinition { type = RoomType.MedicalBay, minWidth = 5, maxWidth = 8, minHeight = 3, maxHeight = 4, minLength = 5, maxLength = 8, possibleLootItems = new[] { 801, 802, 803 }, enemySpawnChance = 0.2f },
            new RoomDefinition { type = RoomType.EngineRoom, minWidth = 6, maxWidth = 10, minHeight = 5, maxHeight = 8, minLength = 6, maxLength = 10, possibleLootItems = new[] { 102, 201 }, enemySpawnChance = 0.3f },
            new RoomDefinition { type = RoomType.Vault, minWidth = 4, maxWidth = 6, minHeight = 3, maxHeight = 4, minLength = 4, maxLength = 6, possibleLootItems = new[] { 901, 902, 501 }, enemySpawnChance = 0.6f },
            new RoomDefinition { type = RoomType.SecretChamber, minWidth = 4, maxWidth = 6, minHeight = 3, maxHeight = 4, minLength = 4, maxLength = 6, possibleLootItems = new[] { 901, 902, 601 }, enemySpawnChance = 0.7f },
            new RoomDefinition { type = RoomType.BossArena, minWidth = 10, maxWidth = 15, minHeight = 5, maxHeight = 10, minLength = 10, maxLength = 15, possibleLootItems = new[] { 901, 902, 301, 302, 303 }, enemySpawnChance = 1.0f }
        };

        public static DungeonResult GenerateDungeon(int seed, int minRooms, int maxRooms, Vector3 origin)
        {
            System.Random rand = new System.Random(seed);
            var result = new DungeonResult
            {
                rooms = new List<GeneratedRoom>(),
                entrancePosition = origin,
                bossPosition = Vector3.zero,
                totalLootCount = 0,
                totalEnemyCount = 0
            };

            int roomCount = rand.Next(minRooms, maxRooms + 1);
            var grid = new bool[100, 100]; // Simple occupancy grid

            // Generate rooms
            for (int i = 0; i < roomCount; i++)
            {
                RoomDefinition def = GetRoomDefinitionForIndex(i, roomCount);
                
                int width = rand.Next(def.minWidth, def.maxWidth + 1);
                int height = rand.Next(def.minHeight, def.maxHeight + 1);
                int length = rand.Next(def.minLength, def.maxLength + 1);

                // Find valid position
                int gx = rand.Next(5, 95 - width);
                int gz = rand.Next(5, 95 - length);

                if (!CanPlaceRoom(grid, gx, gz, width, length))
                {
                    continue; // Skip if can't place
                }

                // Mark grid as occupied
                MarkRoomOccupied(grid, gx, gz, width, length);

                var room = new GeneratedRoom
                {
                    type = def.type,
                    x = gx,
                    y = 0,
                    z = gz,
                    width = width,
                    height = height,
                    length = length,
                    connections = new List<int>(),
                    hasLoot = rand.NextDouble() < 0.7f && def.possibleLootItems.Length > 0,
                    hasEnemies = rand.NextDouble() < def.enemySpawnChance
                };

                // Connect to nearest existing room
                if (result.rooms.Count > 0)
                {
                    int nearestIdx = FindNearestRoom(result.rooms, room);
                    if (nearestIdx >= 0)
                    {
                        room.connections.Add(nearestIdx);
                        result.rooms[nearestIdx].connections.Add(result.rooms.Count);
                    }
                }

                if (room.hasLoot) result.totalLootCount++;
                if (room.hasEnemies) result.totalEnemyCount++;

                if (def.type == RoomType.BossArena)
                {
                    result.bossPosition = new Vector3(gx * 2, 0, gz * 2);
                }

                result.rooms.Add(room);
            }

            return result;
        }

        private static RoomDefinition GetRoomDefinitionForIndex(int index, int totalRooms)
        {
            if (index == 0) return RoomDefinitions[0]; // Entrance
            if (index == totalRooms - 1) return RoomDefinitions[11]; // Boss Arena
            
            System.Random rand = new System.Random(index * 31337);
            return RoomDefinitions[rand.Next(1, RoomDefinitions.Length - 1)];
        }

        private static bool CanPlaceRoom(bool[,] grid, int x, int z, int width, int length)
        {
            for (int dx = -1; dx <= width; dx++)
            {
                for (int dz = -1; dz <= length; dz++)
                {
                    if (grid[x + dx, z + dz]) return false;
                }
            }
            return true;
        }

        private static void MarkRoomOccupied(bool[,] grid, int x, int z, int width, int length)
        {
            for (int dx = 0; dx < width; dx++)
            {
                for (int dz = 0; dz < length; dz++)
                {
                    grid[x + dx, z + dz] = true;
                }
            }
        }

        private static int FindNearestRoom(List<GeneratedRoom> rooms, GeneratedRoom newRoom)
        {
            int nearestIdx = -1;
            float minDist = float.MaxValue;

            for (int i = 0; i < rooms.Count; i++)
            {
                float dist = math.distance(
                    new Vector3(rooms[i].x, 0, rooms[i].z),
                    new Vector3(newRoom.x, 0, newRoom.z)
                );
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestIdx = i;
                }
            }

            return nearestIdx;
        }
    }
}
