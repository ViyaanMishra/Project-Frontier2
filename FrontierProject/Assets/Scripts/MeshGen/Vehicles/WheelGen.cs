using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class WheelGen {
        public static Mesh GenerateTire() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddTorus(b, 0.4f, 0.15f, 16);
            return b.BuildFlat("Tire");
        }
        public static Mesh GenerateOffRoadWheel() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.45f, 0.4f, 12);
            return b.BuildFlat("OffRoadWheel");
        }
        public static Mesh GenerateTankTrack() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.8f, 0.05f, 1.5f);
            return b.BuildFlat("TankTrack");
        }
    }
}
