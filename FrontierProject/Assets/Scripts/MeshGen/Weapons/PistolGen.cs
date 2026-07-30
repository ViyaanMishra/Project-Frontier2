using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class PistolGen {
        public static Mesh GeneratePistol() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.06f, 0.1f, 0.25f);
            PrimitiveShapes.AddBox(b, 0.05f, 0.15f, 0.08f, new Vector3(0, -0.12f, 0));
            return b.BuildFlat("Pistol");
        }
        public static Mesh GenerateRevolver() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.07f, 0.12f, 0.2f);
            PrimitiveShapes.AddCylinder(b, 0.06f, 0.1f, 8, new Vector3(0, 0.05f, 0.1f));
            PrimitiveShapes.AddBox(b, 0.05f, 0.15f, 0.08f, new Vector3(0, -0.12f, 0));
            return b.BuildFlat("Revolver");
        }
    }
}
