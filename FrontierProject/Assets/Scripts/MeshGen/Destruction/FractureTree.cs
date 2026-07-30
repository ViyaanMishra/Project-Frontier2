using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Destruction {
    public static class FractureTreeGen {
        public static Mesh[] GenerateFracturedTree(int seed = 0) {
            ProceduralRandom.Init(seed);
            Mesh[] shards = new Mesh[5];
            shards[0] = new LowPolyMeshBuilder().BuildFlat("Stump");
            for (int i = 1; i < 5; i++) {
                var b = new LowPolyMeshBuilder();
                PrimitiveShapes.AddCylinder(b, 0.15f, 1.0f, 6);
                shards[i] = b.BuildFlat($"Branch_{i}");
            }
            return shards;
        }
    }
}
