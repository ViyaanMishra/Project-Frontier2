using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class RifleGen {
        public static Mesh GenerateAssaultRifle() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.08f, 0.12f, 0.5f);
            PrimitiveShapes.AddCylinder(b, 0.04f, 0.4f, 12, new Vector3(0, 0.08f, 0.2f));
            PrimitiveShapes.AddBox(b, 0.06f, 0.2f, 0.1f, new Vector3(0, -0.15f, 0.1f));
            return b.BuildFlat("AssaultRifle");
        }
        public static Mesh GenerateSniperRifle() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.07f, 0.1f, 0.8f);
            PrimitiveShapes.AddCylinder(b, 0.05f, 0.6f, 12, new Vector3(0, 0.08f, 0.3f));
            PrimitiveShapes.AddCylinder(b, 0.04f, 0.3f, 8, new Vector3(0, 0.15f, 0.4f));
            return b.BuildFlat("SniperRifle");
        }
    }
}
