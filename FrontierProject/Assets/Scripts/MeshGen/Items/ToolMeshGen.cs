using UnityEngine;
namespace Frontier.MeshGen.Items {
    public static class ToolMeshGen {
        public static Mesh GenerateHammer() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.05f, 0.8f, 8);
            PrimitiveShapes.AddBox(b, 0.15f, 0.08f, 0.2f, new Vector3(0, 0.3f, 0));
            return b.BuildFlat("Hammer");
        }
        public static Mesh GenerateWrench() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.04f, 0.02f, 0.5f);
            return b.BuildFlat("Wrench");
        }
        public static Mesh GeneratePickaxe() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.04f, 1.0f, 8);
            PrimitiveShapes.AddBox(b, 0.3f, 0.05f, 0.1f, new Vector3(0, 0.4f, 0));
            return b.BuildFlat("Pickaxe");
        }
    }
}
