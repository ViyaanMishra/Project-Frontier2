using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Destruction {
    public static class DebrisMeshLib {
        public static Mesh GenerateConcreteChunk(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.4f, 0.3f, 0.5f);
            MeshModifiers.NoiseDisplace(b, 0.08f, seed);
            return b.BuildFlat("ConcreteChunk");
        }
        public static Mesh GenerateMetalScrap(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.3f, 0.05f, 0.4f);
            MeshModifiers.Bend(b, 0.5f);
            return b.BuildFlat("MetalScrap");
        }
        public static Mesh GenerateWoodSplinter(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.05f, 0.05f, 0.4f);
            return b.BuildFlat("WoodSplinter");
        }
        public static Mesh GenerateGlassShard(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.15f, 0.02f, 0.2f);
            return b.BuildFlat("GlassShard");
        }
    }
}
