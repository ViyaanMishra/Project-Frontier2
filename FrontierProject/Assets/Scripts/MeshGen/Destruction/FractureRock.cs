using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Destruction {
    public static class FractureRockGen {
        public static Mesh[] GenerateFracturedRock(int seed = 0, int shardCount = 6) {
            ProceduralRandom.Init(seed);
            Mesh[] shards = new Mesh[shardCount];
            for (int i = 0; i < shardCount; i++) {
                var b = new LowPolyMeshBuilder();
                PrimitiveShapes.AddIcosphere(b, ProceduralRandom.Range(0.3f, 0.6f), 2);
                MeshModifiers.NoiseDisplace(b, 0.1f, seed + i);
                shards[i] = b.BuildFlat($"RockShard_{i}");
            }
            return shards;
        }
    }
}
