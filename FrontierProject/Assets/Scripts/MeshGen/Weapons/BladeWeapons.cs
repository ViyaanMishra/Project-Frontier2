using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class BladeWeaponsGen {
        public static Mesh GenerateKnife() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.03f, 0.2f, 0.08f);
            PrimitiveShapes.AddCylinder(b, 0.04f, 0.15f, 8, new Vector3(0, -0.2f, 0));
            return b.BuildFlat("Knife");
        }
        public static Mesh GenerateMachete() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.04f, 0.5f, 0.12f);
            PrimitiveShapes.AddCylinder(b, 0.05f, 0.2f, 8, new Vector3(0, -0.3f, 0));
            return b.BuildFlat("Machete");
        }
        public static Mesh GenerateAxe() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.15f, 0.08f, 0.2f, new Vector3(0, 0.3f, 0));
            PrimitiveShapes.AddCylinder(b, 0.05f, 0.8f, 8, new Vector3(0, -0.3f, 0));
            return b.BuildFlat("Axe");
        }
    }
}
