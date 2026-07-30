using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Destruction {
    public static class FractureWallGen {
        public static Mesh[] GenerateFracturedWall(int seed = 0, int shardCount = 8) {
            ProceduralRandom.Init(seed);
            Mesh[] shards = new Mesh[shardCount];
            for (int i = 0; i < shardCount; i++) {
                var b = new LowPolyMeshBuilder();
                float w = ProceduralRandom.Range(0.3f, 0.8f);
                float h = ProceduralRandom.Range(0.4f, 1.0f);
                float d = 0.2f;
                PrimitiveShapes.AddBox(b, w, h, d);
                MeshModifiers.NoiseDisplace(b, 0.05f, seed + i);
                shards[i] = b.BuildFlat($"WallShard_{i}");
            }
            return shards;
        }
    }
}
