using UnityEngine;
namespace Frontier.VFX {
    public static class ElectricArcGen {
        public static Mesh GenerateArc() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.02f, 1.0f, 6);
            MeshModifiers.Bend(b, 0.3f);
            return b.BuildFlat("ElectricArc");
        }
    }
}
