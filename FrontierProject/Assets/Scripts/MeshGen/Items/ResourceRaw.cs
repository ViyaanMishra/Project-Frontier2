using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Items {
    public static class ResourceRawGen {
        public static Mesh GenerateIronOre(int seed = 0) {
            ProceduralRandom.Init(seed);
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.5f, 2);
            MeshModifiers.NoiseDisplace(b, 0.1f, seed);
            return b.BuildFlat("IronOre");
        }
        public static Mesh GenerateCopperOre(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.5f, 2);
            MeshModifiers.NoiseDisplace(b, 0.12f, seed);
            return b.BuildFlat("CopperOre");
        }
        public static Mesh GenerateCoal(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.4f, 0.3f, 0.5f);
            MeshModifiers.NoiseDisplace(b, 0.05f, seed);
            return b.BuildFlat("Coal");
        }
        public static Mesh GenerateWoodLog(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.2f, 1.0f, 6);
            return b.BuildFlat("WoodLog");
        }
    }
}
