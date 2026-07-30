using UnityEngine;
namespace Frontier.MeshGen.Items {
    public static class FoodRawGen {
        public static Mesh GenerateCarrot() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCone(b, 0.08f, 0.3f, 8);
            return b.BuildFlat("Carrot");
        }
        public static Mesh GeneratePotato() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.15f, 1);
            MeshModifiers.NoiseDisplace(b, 0.03f, 0);
            return b.BuildFlat("Potato");
        }
        public static Mesh GenerateApple() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.12f, 2);
            return b.BuildFlat("Apple");
        }
    }
}
