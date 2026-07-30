using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Destruction {
    public static class FractureFurnitureGen {
        public static Mesh[] GenerateFracturedTable(int seed = 0) {
            ProceduralRandom.Init(seed);
            Mesh[] shards = new Mesh[5];
            shards[0] = new LowPolyMeshBuilder().BuildFlat("TableTop");
            for (int i = 1; i < 5; i++) shards[i] = new LowPolyMeshBuilder().BuildFlat($"Leg_{i}");
            return shards;
        }
    }
}
