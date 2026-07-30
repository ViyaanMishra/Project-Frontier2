using UnityEngine;
namespace Frontier.MeshGen.Items {
    public static class ComponentMeshGen {
        public static Mesh GenerateGear() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.15f, 0.05f, 12);
            return b.BuildFlat("Gear");
        }
        public static Mesh GenerateSpring() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.08f, 0.2f, 16);
            return b.BuildFlat("Spring");
        }
    }
}
