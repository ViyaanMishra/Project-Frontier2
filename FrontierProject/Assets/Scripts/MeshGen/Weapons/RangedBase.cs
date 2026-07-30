using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class RangedBaseGen {
        public static Mesh GenerateStock() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.08f, 0.1f, 0.3f);
            return b.BuildFlat("Stock");
        }
        public static Mesh GenerateBarrel(float length = 0.5f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.04f, length, 12);
            return b.BuildFlat("Barrel");
        }
        public static Mesh GenerateMagazine() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.06f, 0.2f, 0.1f);
            return b.BuildFlat("Magazine");
        }
    }
}
