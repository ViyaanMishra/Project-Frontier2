using UnityEngine;
namespace Frontier.MeshGen.Items {
    public static class MedicalMeshGen {
        public static Mesh GenerateSyringe() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.03f, 0.3f, 8);
            return b.BuildFlat("Syringe");
        }
        public static Mesh GenerateMedkit() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.3f, 0.1f, 0.2f);
            return b.BuildFlat("Medkit");
        }
    }
}
