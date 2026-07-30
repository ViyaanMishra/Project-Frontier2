using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class BluntWeaponsGen {
        public static Mesh GenerateBat() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.06f, 0.9f, 12);
            return b.BuildFlat("Bat");
        }
        public static Mesh GenerateHammer() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.2f, 0.1f, 0.1f, new Vector3(0, 0.4f, 0));
            PrimitiveShapes.AddCylinder(b, 0.05f, 0.6f, 8, new Vector3(0, -0.2f, 0));
            return b.BuildFlat("Hammer");
        }
        public static Mesh GenerateSledgehammer() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.3f, 0.15f, 0.15f, new Vector3(0, 0.5f, 0));
            PrimitiveShapes.AddCylinder(b, 0.06f, 1.0f, 8, new Vector3(0, -0.3f, 0));
            return b.BuildFlat("Sledgehammer");
        }
    }
}
