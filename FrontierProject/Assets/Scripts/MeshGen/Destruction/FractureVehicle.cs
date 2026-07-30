using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Destruction {
    public static class FractureVehicleGen {
        public static Mesh[] GenerateFracturedVehicle(int seed = 0) {
            ProceduralRandom.Init(seed);
            Mesh[] shards = new Mesh[6];
            for (int i = 0; i < 6; i++) {
                var b = new LowPolyMeshBuilder();
                PrimitiveShapes.AddBox(b, 0.5f, 0.3f, 0.8f);
                shards[i] = b.BuildFlat($"VehiclePanel_{i}");
            }
            return shards;
        }
    }
}
