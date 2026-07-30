using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class VehicleBaseGen {
        public static Mesh GenerateChassis(float length = 4f, float width = 2f, float height = 1.5f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, width, height, length);
            return b.BuildFlat("VehicleChassis");
        }
        public static Mesh GenerateWheel(float radius = 0.4f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, radius, 0.3f, 16);
            return b.BuildFlat("Wheel");
        }
    }
}
