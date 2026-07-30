using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class MeleeBaseGen {
        public static Mesh GenerateHandle() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.04f, 0.3f, 8);
            return b.BuildFlat("MeleeHandle");
        }
        public static Mesh GenerateBlade(float length = 0.6f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.05f, length, 0.15f);
            return b.BuildFlat("Blade");
        }
    }
}
